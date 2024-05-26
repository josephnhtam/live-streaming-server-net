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

        [LoggerMessage(LogLevel.Error, "An error occurred while delete outdated TS files | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DeletingOutdatedTsFilesError(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching HLS files stored event | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DispatchingHlsFilesStoredEventError(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching HLS files storing complete event | Transmuxer: {Transmuxer} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DispatchingHlsFilesStoringCompleteEventError(this ILogger logger, string transmuxer, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Warning, "Failed to register HLS transmuxer | StreamPath: {StreamPath}")]
        public static partial void RegisteringHlsTransmuxerFailed(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "HLS transmuxer started | Transmuxer: {Transmuxer} | Identifier: {Identifier} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void HlsTransmuxerStarted(this ILogger logger, string transmuxer, Guid identifier, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Information, "HLS transmuxer ended | Transmuxer: {Transmuxer} | Identifier: {Identifier} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void HlsTransmuxerEnded(this ILogger logger, string transmuxer, Guid identifier, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Error, "An error occurred while processing HLS transmuxing | Transmuxer: {Transmuxer} | Identifier: {Identifier} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void ProcessingHlsTransmuxingError(this ILogger logger, string transmuxer, Guid identifier, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Trace, "A ts segment is created | Transmuxer: {Transmuxer} | Identifier: {Identifier} | StreamPath: {StreamPath} | Path: {Path} | SequenceNumber: {SequenceNumber} | Duration: {Duration}")]
        public static partial void TsSegmentCreated(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path, uint sequenceNumber, int duration);

        [LoggerMessage(LogLevel.Trace, "An outdated ts segment is deleted | Transmuxer: {Transmuxer} | Identifier: {Identifier} | StreamPath: {StreamPath} | Path: {Path}")]
        public static partial void OutdatedTsSegmentDeleted(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path);

        [LoggerMessage(LogLevel.Trace, "HLS manifest is updated | Transmuxer: {Transmuxer} | Identifier: {Identifier} | StreamPath: {StreamPath} | Path: {Path}")]
        public static partial void HlsManifestUpdated(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path);
    }
}
