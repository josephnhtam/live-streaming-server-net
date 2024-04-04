using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Transmuxer.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Information, "Transmuxer Started | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void TransmuxerStarted(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Information, "Transmuxer Stopped | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void TransmuxerStopped(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Error, "Transmuxer Error | InputPath: {InputPath} | StreamPath: {StreamPath}")]
        public static partial void TransmuxerError(this ILogger logger, string inputPath, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching transmuxer started event | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DispatchingTransmuxerStartedEventError(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching transmuxer stopped event | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DispatchingTransmuxerStoppedEventError(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while running HLS uploader | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void RunningHlsUploaderError(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while uploading HLS to store | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void UploadingHlsToStoreError(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching HLS files stored event | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DispatchingHlsFilesStoredEventError(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching HLS files storing complete event | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DispatchingHlsFilesStoringCompleteEventError(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);
    }
}
