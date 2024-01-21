using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Transmuxer.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Information, "Transmuxer Started | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void TransmuxerStarted(this ILogger logger, string identifier, string inputPath, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Information, "Transmuxer Stopped | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void TransmuxerStopped(this ILogger logger, string identifier, string inputPath, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Error, "Transmuxer Error | InputPath: {InputPath} | StreamPath: {StreamPath}")]
        public static partial void TransmuxerError(this ILogger logger, string inputPath, string streamPath, Exception exception);
    }
}
