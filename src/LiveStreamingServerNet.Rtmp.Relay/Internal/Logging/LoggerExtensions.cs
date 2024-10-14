using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Rtmp.Relay;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred while processing the downstream (StreamPath={StreamPath})")]
        public static partial void RtmpDownstreamError(this ILogger logger, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while processing the media data (StreamPath={StreamPath}, MediaType={MediaType})")]
        public static partial void RtmpDownstreamMediaDataProcessingError(this ILogger logger, string streamPath, MediaType mediaType);

        [LoggerMessage(LogLevel.Information, "Downstream idle timeout (StreamPath={StreamPath})")]
        public static partial void RtmpDownstreamIdleTimeout(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Error, "The origin for the downstream could not be resolved (StreamPath={StreamPath})")]
        public static partial void RtmpDownstreamOriginNotResolved(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "Downstream origin resolved (StreamPath={StreamPath}, RtmpOrigin={Origin})")]
        public static partial void RtmpDownstreamOriginResolved(this ILogger logger, string streamPath, RtmpOrigin origin);

        [LoggerMessage(LogLevel.Information, "Connecting to the origin for the downstream (StreamPath={StreamPath}, OriginEndPoint={OriginEndPoint})")]
        public static partial void RtmpDownstreamConnecting(this ILogger logger, string streamPath, ServerEndPoint originEndPoint);

        [LoggerMessage(LogLevel.Information, "Creating downstream (StreamPath={StreamPath})")]
        public static partial void RtmpDownstreamCreating(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "Downstream created (StreamPath={StreamPath})")]
        public static partial void RtmpDownstreamCreated(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "Downstream stopped (StreamPath={StreamPath})")]
        public static partial void RtmpDownstreamStopped(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Error, "An error occurred while processing the upstream (StreamPath={StreamPath})")]
        public static partial void RtmpUpstreamError(this ILogger logger, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while sending the media data (StreamPath={StreamPath}, MediaType={MediaType})")]
        public static partial void RtmpUpstreamMediaDataSendingError(this ILogger logger, string streamPath, MediaType mediaType, Exception ex);

        [LoggerMessage(LogLevel.Information, "Upstream idle timeout (StreamPath={StreamPath})")]
        public static partial void RtmpUpstreamIdleTimeout(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Error, "The origin for the upstream could not be resolved (StreamPath={StreamPath})")]
        public static partial void RtmpUpstreamOriginNotResolved(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "Upstream origin resolved (StreamPath={StreamPath}, RtmpOrigin={Origin})")]
        public static partial void RtmpUpstreamOriginResolved(this ILogger logger, string streamPath, RtmpOrigin origin);

        [LoggerMessage(LogLevel.Information, "Connecting to the origin for the upstream (StreamPath={StreamPath}, OriginEndPoint={OriginEndPoint})")]
        public static partial void RtmpUpstreamConnecting(this ILogger logger, string streamPath, ServerEndPoint originEndPoint);

        [LoggerMessage(LogLevel.Information, "Creating upstream (StreamPath={StreamPath})")]
        public static partial void RtmpUpstreamCreating(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "Upstream created (StreamPath={StreamPath})")]
        public static partial void RtmpUpstreamCreated(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "Upstream stopped (StreamPath={StreamPath})")]
        public static partial void RtmpUpstreamStopped(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Debug, "Begin upstream media packet discard (StreamPath={StreamPath}, OutstandingSize={OutstandingSize}, OutstandingCount={OutstandingCount})")]
        public static partial void BeginUpstreamMediaPacketDiscard(this ILogger logger, string streamPath, long outstandingSize, long outstandingCount);

        [LoggerMessage(LogLevel.Debug, "End upstream media packet discard (StreamPath={StreamPath}, OutstandingSize={OutstandingSize}, OutstandingCount={OutstandingCount})")]
        public static partial void EndUpstreamMediaPacketDiscard(this ILogger logger, string streamPath, long outstandingSize, long outstandingCount);

        [LoggerMessage(LogLevel.Error, "An error occurred while initializing the downstream (StreamPath={StreamPath})")]
        public static partial void RtmpDownstreamInitializationError(this ILogger logger, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while resolving the origin for the upstream (StreamPath={StreamPath})")]
        public static partial void RtmpUpstreamOriginResolveError(this ILogger logger, string streamPath, Exception ex);
    }
}
