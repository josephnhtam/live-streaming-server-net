using LiveStreamingServerNet.KubernetesPod.Installer;
using LiveStreamingServerNet.KubernetesPod.Redis.Installer;
using LiveStreamingServerNet.Networking.Helpers;
using StackExchange.Redis;
using System.Net;

namespace LiveStreamingServerNet.KubernetesPodDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            using var liveStreamingServer = CreateLiveStreamingServer();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

            builder.Services.AddHealthChecks();

            var app = builder.Build();

            app.UseHealthChecks("/hc");

            await app.RunAsync();
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            var redisConn = ConnectionMultiplexer.Connect("redis-master:6379");

            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .AddKubernetesPodServices(podOptions =>
                        podOptions.AddStreamRegistry(registryOptions =>
                            registryOptions.UseRedisStore(redisConn)
                        )
                    )
                )
                .ConfigureLogging(options => options.SetMinimumLevel(LogLevel.Trace).AddConsole())
                .Build();
        }
    }
}
