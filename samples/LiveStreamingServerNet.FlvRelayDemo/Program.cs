using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Rtmp.Relay;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Installer;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LiveStreamingServerNet.FlvRelayDemo
{
    public static class Program
    {
        public static async Task Main()
        {
            await using var origin = CreateOriginServer(1935);
            await using var relay1 = CreateRelayServer(1936);
            await using var relay2 = CreateRelayServer(1937);

            await Task.WhenAll(
                origin.RunAsync("http://+:9000"),
                relay1.RunAsync("http://+:9001"),
                relay2.RunAsync("http://+:9002")
            );
        }

        private static WebApplication CreateOriginServer(int rtmpPort)
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddLiveStreamingServer(
                new IPEndPoint(IPAddress.Any, rtmpPort),
                options => options.AddFlv()
            );

            var app = builder.Build();

            app.UseWebSockets();

            app.UseWebSocketFlv();

            app.UseHttpFlv();

            return app;
        }

        private static WebApplication CreateRelayServer(int rtmpPort)
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddLiveStreamingServer(
                new IPEndPoint(IPAddress.Any, rtmpPort),
                options => options
                    .AddFlv()
                    .UseRtmpRelay<RtmpOriginResolver>()
            );

            var app = builder.Build();

            app.UseWebSockets();

            app.UseWebSocketFlv();

            app.UseHttpFlv();

            return app;
        }
    }

    public partial class RtmpOriginResolver : IRtmpOriginResolver
    {
        [GeneratedRegex(@"/?(?<appName>[^/]+)/(?<streamName>.+)$", RegexOptions.IgnoreCase)]
        private static partial Regex NamesExtractionRegex();

        public ValueTask<RtmpOrigin?> ResolveUpstreamOriginAsync(
            string streamPath, IReadOnlyDictionary<string, string> streamArguments, CancellationToken cancellationToken)
        {
            return ResolveOriginAsync(streamPath, streamArguments);
        }

        public ValueTask<RtmpOrigin?> ResolveDownstreamOriginAsync(string streamPath, CancellationToken cancellationToken)
        {
            return ResolveOriginAsync(streamPath, null);
        }

        private static ValueTask<RtmpOrigin?> ResolveOriginAsync(string streamPath, IReadOnlyDictionary<string, string>? streamArguments)
        {
            var match = NamesExtractionRegex().Match(streamPath);

            if (!match.Success)
                return ValueTask.FromResult<RtmpOrigin?>(null);

            var appName = match.Groups["appName"].Value;
            var streamName = CreateStreamName(match.Groups["streamName"].Value, streamArguments);

            var result = new RtmpOrigin(new IPEndPoint(IPAddress.Loopback, 1935), appName, streamName);
            return ValueTask.FromResult<RtmpOrigin?>(result);
        }

        private static string CreateStreamName(string streamName, IReadOnlyDictionary<string, string>? streamArguments)
        {
            var streamNameBuilder = new StringBuilder();
            streamNameBuilder.Append(streamName);

            if (streamArguments?.Any() == true)
            {
                streamNameBuilder.Append('?');

                foreach (var (key, value) in streamArguments)
                {
                    streamNameBuilder.Append(key);
                    streamNameBuilder.Append('=');
                    streamNameBuilder.Append(value);
                    streamNameBuilder.Append('&');
                }

                streamNameBuilder.Length--;
            }

            return streamNameBuilder.ToString();
        }
    }
}
