using LiveStreamingServerNet.Rtmp.Relay;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Installer;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;

namespace LiveStreamingServerNet.RtmpRelayDemo
{
    public class Program
    {
        public static async Task Main()
        {
            using var originServer = CreateOriginServer();
            using var relayServer = CreateRelayServer();

            await Task.WhenAll(
                originServer.RunAsync(new IPEndPoint(IPAddress.Any, 1935)),
                relayServer.RunAsync(new IPEndPoint(IPAddress.Any, 1936)));
        }

        private static ILiveStreamingServer CreateOriginServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }

        private static ILiveStreamingServer CreateRelayServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options =>
                {
                    options.UseRtmpRelay<RtmpOriginResolver>()
                        .ConfigureDownstream(options => options.MaximumIdleTime = TimeSpan.FromSeconds(15));
                })
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }
    }

    public partial class RtmpOriginResolver : IRtmpOriginResolver
    {
        [GeneratedRegex(@"/?(?<appName>[^/]+)/(?<streamName>.+)$", RegexOptions.IgnoreCase)]
        private static partial Regex NamesExtractionRegex();

        public ValueTask<RtmpOrigin?> ResolveUpstreamOriginAsync(string streamPath, CancellationToken cancellationToken)
        {
            return ResolveOriginAsync(streamPath);
        }

        public ValueTask<RtmpOrigin?> ResolveDownstreamOriginAsync(string streamPath, CancellationToken cancellationToken)
        {
            return ResolveOriginAsync(streamPath);
        }

        private static ValueTask<RtmpOrigin?> ResolveOriginAsync(string streamPath)
        {
            var match = NamesExtractionRegex().Match(streamPath);

            if (!match.Success)
                return ValueTask.FromResult<RtmpOrigin?>(null);

            var appName = match.Groups["appName"].Value;
            var streamName = match.Groups["streamName"].Value;

            var result = new RtmpOrigin(new IPEndPoint(IPAddress.Loopback, 1935), appName, streamName);
            return ValueTask.FromResult<RtmpOrigin?>(result);
        }
    }
}
