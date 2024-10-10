using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Client;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.RtmpClientPublishDemo
{
    public class Program
    {
        /// <summary>
        /// Connect to rtmp://127.0.0.1/live/demo and publish the stream from input.flv.
        /// </summary>
        public static async Task Main()
        {
            var rtmpUrl = "rtmp://127.0.0.1/live/demo";
            var parsedRtmpUrl = await RtmpUrlParser.ParseAsync(rtmpUrl);

            var serverEndPoint = parsedRtmpUrl.ServerEndPoint;
            var information = new Dictionary<string, object> { ["tcUrl"] = parsedRtmpUrl.TcUrl };
            var appName = parsedRtmpUrl.AppName;
            var streamName = parsedRtmpUrl.StreamName;

            await using var rtmpClient = RtmpClientBuilder.Create()
                .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Trace))
                .Build();

            var logger = rtmpClient.Services.GetRequiredService<ILogger<Program>>();

            rtmpClient.OnBandwidthLimitUpdated += (sender, e) =>
                logger.LogInformation($"Bandwidth limit updated: {e.BandwidthLimit}");

            await rtmpClient.ConnectAsync(serverEndPoint, appName, information);

            var rtmpStream = await rtmpClient.CreateStreamAsync();

            await PublishStreamFromFlvAsync(streamName, rtmpClient, rtmpStream, logger);
        }

        private static async Task PublishStreamFromFlvAsync(string streamName, IRtmpClient rtmpClient, IRtmpStream rtmpStream, ILogger<Program> logger)
        {
            using var cts = new CancellationTokenSource();
            using var _ = cts.Token.Register(rtmpClient.Stop);

            using var fileStream = new FileStream("input.flv", FileMode.Open, FileAccess.Read, FileShare.Read, 512 * 1024);
            using var flvReader = new FlvReader(fileStream);

            rtmpStream.OnUserControlEventReceived += (sender, e) =>
                logger.LogInformation($"User control event received: {e.EventType}");

            rtmpStream.OnStatusReceived += (sender, e) =>
            {
                logger.LogInformation($"Status received: {e.Code}");

                if (e.Level == RtmpStatusLevels.Error)
                    cts.Cancel();
            };

            rtmpStream.Publish.Publish(streamName);

            try
            {
                await Task.WhenAny(SendMediaDataAsync(rtmpStream, flvReader, cts), rtmpClient.UntilStoppedAsync());
            }
            finally
            {
                cts.Cancel();
            }

            async Task SendMediaDataAsync(IRtmpStream stream, FlvReader flvReader, CancellationTokenSource cts)
            {
                var header = await flvReader.ReadHeaderAsync(cts.Token);

                if (header == null)
                    return;

                var timeController = new TimeSynchronizer();

                while (!cts.IsCancellationRequested)
                {
                    var tag = await flvReader.ReadTagAsync(cts.Token);

                    if (tag == null)
                        return;

                    try
                    {
                        if (tag.Header.TagType == FlvTagType.Audio)
                        {
                            await stream.Publish.SendAudioDataAsync(tag.Payload, tag.Header.Timestamp);
                        }
                        else if (tag.Header.TagType == FlvTagType.Video)
                        {
                            await stream.Publish.SendVideoDataAsync(tag.Payload, tag.Header.Timestamp);
                        }

                        await timeController.SyncWithTimestampAsync(tag.Header.Timestamp);
                    }
                    finally
                    {
                        tag.Payload.Unclaim();
                    }
                }
            }
        }

        private class TimeSynchronizer
        {
            private (uint Timestamp, DateTime Time)? start = null;

            public async ValueTask SyncWithTimestampAsync(uint timestamp)
            {
                if (!start.HasValue)
                    start = (timestamp, DateTime.UtcNow);

                var intendedTime = timestamp - start.Value.Timestamp;
                var elapsedTime = (DateTime.UtcNow - start.Value.Time).TotalMilliseconds;

                var delay = intendedTime - elapsedTime;

                if (delay > 0)
                    await Task.Delay((int)delay);
            }
        }
    }
}
