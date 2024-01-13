
using LiveStreamingServerNet.Demo.BackgroundServices;
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Demo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            IServer liveStreamingServer = CreateLiveStreamingServer();

            builder.Services.AddKeyedSingleton("live-streaming", liveStreamingServer);
            builder.Services.AddHostedService<LiveStreamingServerService>();

            builder.Services.AddHttpFlv();

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
