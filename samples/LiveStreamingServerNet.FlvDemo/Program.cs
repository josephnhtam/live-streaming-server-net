
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Helpers;
using System.Net;

namespace LiveStreamingServerNet.FlvDemo
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

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseWebSockets();
            app.UseWebSocketFlv(liveStreamingServer);

            app.UseHttpFlv(liveStreamingServer);

            app.Run();
        }

        private static IServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options.AddFlv())
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }
    }
}
