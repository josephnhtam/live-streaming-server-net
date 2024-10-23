﻿using LiveStreamingServerNet.Rtmp.Relay;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Installer;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LiveStreamingServerNet.RtmpRelayDemo
{
    /// <summary>
    /// Create one origin server (port 1935) and two relay servers (ports 1936, 1937).
    /// Relay servers push published streams to the origin and pull requested streams from the origin if not locally available.
    /// </summary>
    public class Program
    {
        public static async Task Main()
        {
            using var originServer = CreateOriginServer();
            using var relayServer1 = CreateRelayServer();
            using var relayServer2 = CreateRelayServer();

            await Task.WhenAll(
                originServer.RunAsync(new IPEndPoint(IPAddress.Any, 1935)),
                relayServer1.RunAsync(new IPEndPoint(IPAddress.Any, 1936)),
                relayServer2.RunAsync(new IPEndPoint(IPAddress.Any, 1937)));
        }

        private static ILiveStreamingServer CreateOriginServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureLogging(options => options.AddConsole())
                .ConfigureRtmpServer(options => options.Configure(options =>
                    // Setting a non-zero timeout enables the stream to be resumed within this period,
                    // preventing immediate notifications to subscribers if the stream is temporarily unavailable.
                    options.PublishStreamContinuationTimeout = TimeSpan.FromSeconds(30)
                ))
                .Build();
        }

        private static ILiveStreamingServer CreateRelayServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options =>
                {
                    options.UseRtmpRelay<RtmpOriginResolver>()
                        .ConfigureDownstream(options => options.MaximumIdleTime = TimeSpan.FromSeconds(30));
                })
                .ConfigureLogging(options => options.AddConsole())
                .Build();
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
