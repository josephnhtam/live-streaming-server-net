using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.KubernetesPod.Redis.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "StreamPath: {StreamPath} | An error occurred when registering the stream")]
        public static partial void RegisteringStreamError(this ILogger logger, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Error, "StreamPath: {StreamPath} | An error occurred when unregistering the stream")]
        public static partial void UnregisteringStreamError(this ILogger logger, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Error, "StreamPath: {StreamPath} | An error occurred when deserializing the stream info")]
        public static partial void DeserializingStreamInfoError(this ILogger logger, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Error, "StreamPath: {StreamPath} | An error occurred when revalidating the stream")]
        public static partial void RevalidatingStreamError(this ILogger logger, string streamPath, Exception exception);
    }
}
