using LiveStreamingServerNet.Networking;
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

        [LoggerMessage(LogLevel.Information, "Origin resolved (StreamPath={StreamPath}, OriginEndPoint={OriginEndPoint})")]
        public static partial void RtmpDownstreamOriginResolved(this ILogger logger, string streamPath, ServerEndPoint originEndPoint);

        [LoggerMessage(LogLevel.Information, "Connecting to the origin for the downstream (StreamPath={StreamPath}, OriginEndPoint={OriginEndPoint})")]
        public static partial void RtmpDownstreamConnecting(this ILogger logger, string streamPath, ServerEndPoint originEndPoint);

        [LoggerMessage(LogLevel.Information, "Creating downstream (StreamPath={StreamPath})")]
        public static partial void RtmpDownstreamCreating(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "Downstream created (StreamPath={StreamPath})")]
        public static partial void RtmpDownstreamCreated(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "Downstream stopped (StreamPath={StreamPath})")]
        public static partial void RtmpDownstreamStopped(this ILogger logger, string streamPath);
    }
}
