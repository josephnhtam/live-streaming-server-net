using LiveStreamingServerNet.AdminPanelUI;
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Standalone;
using LiveStreamingServerNet.Standalone.Insatller;
using System.Net;

namespace LiveStreamingServerNet.StandaloneDemo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var liveStreamingServer = CreateLiveStreamingServer();

            builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader())
            );

            var app = builder.Build();

            app.UseCors();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseWebSockets();
            app.UseWebSocketFlv(liveStreamingServer);

            app.UseHttpFlv(liveStreamingServer);

            app.MapStandaloneServerApiEndPoints(liveStreamingServer);
            app.UseAdminPanelUI(new AdminPanelUIOptions { BasePath = "/ui", HasHttpFlvPreview = true });

            app.Run();
        }

        private static IServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options.AddStandaloneServices().AddFlv())
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }
    }
}
