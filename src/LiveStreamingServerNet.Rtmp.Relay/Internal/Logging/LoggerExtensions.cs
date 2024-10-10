using LiveStreamingServerNet.Networking;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "StreamPath: {StreamPath} | An error occurred while processing the downstream")]
        public static partial void RtmpDownstreamError(this ILogger logger, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "StreamPath: {StreamPath} | MediaType: {MediaType} | An error occurred while processing the media data")]
        public static partial void RtmpDownstreamMediaDataProcessingError(this ILogger logger, string streamPath, MediaType mediaType);

        [LoggerMessage(LogLevel.Information, "StreamPath: {StreamPath} | Downstream idle timeout")]
        public static partial void RtmpDownstreamIdleTimeout(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "StreamPath: {StreamPath} | Origin: {OriginEndPoint} | Origin resolved")]
        public static partial void RtmpDownstreamOriginResolved(this ILogger logger, string streamPath, ServerEndPoint originEndPoint);

        [LoggerMessage(LogLevel.Information, "StreamPath: {StreamPath} | Origin: {OriginEndPoint} | Connecting to the origin for the downstream")]
        public static partial void RtmpDownstreamConnecting(this ILogger logger, string streamPath, ServerEndPoint originEndPoint);

        [LoggerMessage(LogLevel.Information, "StreamPath: {StreamPath} | Creating downstream")]
        public static partial void RtmpDownstreamCreating(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "StreamPath: {StreamPath} | Downstream created")]
        public static partial void RtmpDownstreamCreated(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "StreamPath: {StreamPath} | Downstream stopped")]
        public static partial void RtmpDownstreamStopped(this ILogger logger, string streamPath);
    }
}
