using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Transmuxer.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Debug, "Transmuxer Error | InputPath: {InputPath} | OutputDirPath: {OutputDirPath} | StreamPath: {StreamPath}")]
        public static partial void TransmuxerError(this ILogger logger, string inputPath, string outputDirPath, string streamPath, Exception exception);
    }
}
