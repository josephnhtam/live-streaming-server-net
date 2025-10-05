using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Utilities.Contracts;
using System.Net;

namespace LiveStreamingServerNet.FlvDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLiveStreamingServer(
                new IPEndPoint(IPAddress.Any, 1935),
                options => options.AddFlv(configure =>
                    configure.AddStreamEventHandler<FlvServerStreamEventHandler>()
                )
            );

            var app = builder.Build();

            app.UseWebSockets();

            app.UseWebSocketFlv();

            app.UseHttpFlv();

            await app.RunAsync();
        }
    }

    public class FlvServerStreamEventHandler : IFlvServerStreamEventHandler
    {
        private readonly IFlvStreamInfoManager _streamInfoManager;
        private readonly ILogger<FlvServerStreamEventHandler> _logger;

        public FlvServerStreamEventHandler(IFlvStreamInfoManager streamInfoManager, ILogger<FlvServerStreamEventHandler> logger)
        {
            _streamInfoManager = streamInfoManager;
            _logger = logger;
        }

        public ValueTask OnFlvStreamSubscribedAsync(IEventContext context, IFlvClientHandle client)
        {
            var streamInfo = _streamInfoManager.GetStreamInfo(client.StreamPath);
            _logger.LogInformation("Client {ClientId} subscribed to stream {StreamPath}. Total subscribers: {SubscriberCount}",
                client.ClientId, client.StreamPath, streamInfo?.Subscribers.Count() ?? 0);

            return ValueTask.CompletedTask;
        }

        public ValueTask OnFlvStreamUnsubscribedAsync(IEventContext context, string clientId, string streamPath)
        {
            var streamInfo = _streamInfoManager.GetStreamInfo(streamPath);
            _logger.LogInformation("Client {ClientId} unsubscribed from stream {StreamPath}. Total subscribers: {SubscriberCount}",
                clientId, streamPath, streamInfo?.Subscribers.Count() ?? 0);
            return ValueTask.CompletedTask;
        }
    }
}
