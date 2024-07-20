using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer;
using LiveStreamingServerNet.StreamProcessor.Installer;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

namespace LiveStreamingServerNet.HlsAuthenticationDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var liveStreamingServer = CreateLiveStreamingServer();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyHeader()
                          .AllowAnyOrigin()
                          .AllowAnyMethod()
                )
            );

            builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            "THIS_IS_A_SUPER_SECRET_KEY_FOR_DEMO_PURPOSE_ONLY")),
                    };
                }
            );

            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseAuthorization();

            app.UseAuthentication();

            app.UseCors();

            // Given that the scheme is https, the port is 7138, and the stream path is live/demo,
            // the HLS stream will be available at https://localhost:7138/hls/live/demo/output.m3u8
            app.UseHlsFiles(liveStreamingServer, new HlsServingOptions
            {
                RequestPath = "/hls",

                // Use bearer token authentication for HLS files
                OnProcessRequestAsync = async (context) =>
                {
                    if (context.Context.User.Identity?.IsAuthenticated != true)
                    {
                        context.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Context.Response.CompleteAsync();

                        context.Context.RequestServices
                            .GetRequiredService<ILogger<Program>>()
                            .LogWarning($"Unauthorized request for {context.StreamPath} from {context.Context.Connection.RemoteIpAddress}");
                    }
                }
            });

            await app.RunAsync();
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .Configure(options => options.EnableGopCaching = false)
                    .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC))
                    .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
                    .AddStreamProcessor()
                    .AddHlsTransmuxer()
                )
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }
    }
}
