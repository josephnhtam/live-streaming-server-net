using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Trace, "Message handler not found (ChunkStreamId={ChunkStreamId}, MessageTypeId={MessageTypeId})")]
        public static partial void MessageHandlerNotFound(this ILogger logger, uint chunkStreamId, byte messageTypeId);
    }
}
