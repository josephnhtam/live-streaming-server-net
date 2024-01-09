
using LiveStreamingServerNet.Demo.BackgroundServices;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Flv.Installer;

namespace LiveStreamingServerNet.Demo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddKeyedSingleton("live-streaming", CreateLiveStreamingServer());
            builder.Services.AddHostedService<LiveStreamingServerService>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseHttpFlv();

            app.Run();
        }

        private static IServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options.AddHttpFlv())
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }
    }
}
