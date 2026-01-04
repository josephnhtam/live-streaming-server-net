using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Auth;
using LiveStreamingServerNet.Rtmp.Server.Auth.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.CustomStreamKeyDemo
{
    /// <summary>
    /// This demo allows a publisher to publish a stream to /demo_user/demo_key and
    /// allows subscribers to watch the stream at /demo_user/live_stream.
    /// </summary>
    public static class Program
    {
        public static async Task Main()
        {
            using var liveStreamingServer = CreateLiveStreamingServer();

            await liveStreamingServer.RunAsync(new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false));
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options =>
                {
                    options.Services.AddKeyedSingleton<IInternalStreamPathConverter, PublisherInternalStreamPathConverter>("publisher");
                    options.Services.AddKeyedSingleton<IInternalStreamPathConverter, SubscriberInternalStreamPathConverter>("subscriber");

                    options.AddAuthorizationHandler<DemoAuthorizationHandler>();
                })
                .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .Build();
        }
    }

    /// <summary>
    /// Defines a contract for converting an external stream path (provided by the client)
    /// into an internal stream path (used by the server).
    /// </summary>
    public interface IInternalStreamPathConverter
    {
        ValueTask<string?> ConvertAsync(string streamPath);
    }

    /// <summary>
    /// Handles logic for converting Publisher stream paths.
    /// Example: OBS Studio pushing a stream to rtmp://server/demo_user/demo_key
    /// </summary>
    public class PublisherInternalStreamPathConverter : IInternalStreamPathConverter
    {
        public ValueTask<string?> ConvertAsync(string streamPath)
        {
            // Expected format: "/AppName/StreamName" (e.g., "/demo_user/demo_key")
            var split = streamPath.Split('/');

            if (split.Length == 3)
            {
                var username = split[1];
                var streamKey = split[2];

                if (username == "__internal__")
                {
                    // Prevent publishing to reserved internal path
                    return ValueTask.FromResult<string?>(null);
                }

                // Validate the credentials. In a real app, you would check a database here.
                if (username == "demo_user" && streamKey == "demo_key")
                {
                    // This maps the stream path to the actual stream path "/{username}/singleton".
                    // Must be started with a '/'.
                    return ValueTask.FromResult<string?>($"/__internal__/{username}");
                }
            }

            // Return null if validation fails
            return ValueTask.FromResult<string?>(null);
        }
    }

    /// <summary>
    /// Handles logic for converting Subscriber stream paths.
    /// Example: VLC Player watching rtmp://server/demo_user/live_stream
    /// </summary>
    public class SubscriberInternalStreamPathConverter : IInternalStreamPathConverter
    {
        public ValueTask<string?> ConvertAsync(string streamPath)
        {
            // Expected format: "/AppName/StreamName" (e.g., "/demo_user/live_stream")
            var split = streamPath.Split('/');

            if (split.Length == 3)
            {
                var username = split[1];
                var streamKey = split[2];

                if (username == "__internal__")
                {
                    // Prevent subscribing to reserved internal path
                    return ValueTask.FromResult<string?>(null);
                }

                // Validate the request.
                // Notice the streamKey here is "live_stream", not the secret "demo_key".
                // This allows viewers to watch without knowing the publisher's secret key.
                if (username == "demo_user" && streamKey == "live_stream")
                {
                    // Map the public viewing path back to the same internal path used by the publisher.
                    return ValueTask.FromResult<string?>($"/__internal__/{username}");
                }
            }

            return ValueTask.FromResult<string?>(null);
        }
    }

    /// <summary>
    /// The main authorization handler hooked into the LiveStreamingServerNet pipeline.
    /// It intercepts Publish and Subscribe requests to validate them and rewrite paths.
    /// </summary>
    public class DemoAuthorizationHandler : IAuthorizationHandler
    {
        private readonly IInternalStreamPathConverter _publisherStreamPathConverter;
        private readonly IInternalStreamPathConverter _subscriberStreamPathConverter;

        public DemoAuthorizationHandler(
            [FromKeyedServices("publisher")] IInternalStreamPathConverter publisherStreamPathConverter,
            [FromKeyedServices("subscriber")] IInternalStreamPathConverter subscriberStreamPathConverter)
        {
            _publisherStreamPathConverter = publisherStreamPathConverter;
            _subscriberStreamPathConverter = subscriberStreamPathConverter;
        }

        public async Task<AuthorizationResult> AuthorizePublishingAsync(
            ISessionInfo client,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            string publishingType)
        {
            var internalStreamPath = await _publisherStreamPathConverter.ConvertAsync(streamPath);

            if (!string.IsNullOrEmpty(internalStreamPath))
            {
                return AuthorizationResult.Authorized(streamPathOverride: internalStreamPath);
            }

            return AuthorizationResult.Unauthorized("incorrect password");
        }

        public async Task<AuthorizationResult> AuthorizeSubscribingAsync(
            ISessionInfo client,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var internalStreamPath = await _subscriberStreamPathConverter.ConvertAsync(streamPath);

            if (!string.IsNullOrEmpty(internalStreamPath))
            {
                return AuthorizationResult.Authorized(streamPathOverride: internalStreamPath);
            }

            return AuthorizationResult.Unauthorized("incorrect stream path");
        }
    }
}
