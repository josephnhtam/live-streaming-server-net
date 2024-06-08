using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Information, "Stream processor started | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void StreamProcessorStarted(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Information, "Stream processor stopped | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void StreamProcessorStopped(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Error, "Stream processor error | InputPath: {InputPath} | StreamPath: {StreamPath}")]
        public static partial void StreamProcessorError(this ILogger logger, string inputPath, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching stream processor started event | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DispatchingStreamProcessorStartedEventError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching stream processor stopped event | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DispatchingStreamProcessorStoppedEventError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while running HLS uploader | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void RunningHlsUploaderError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while uploading HLS to store | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void UploadingHlsToStoreError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while delete outdated TS segments | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DeletingOutdatedTsSegmentsError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching HLS files stored event | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DispatchingHlsFilesStoredEventError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching HLS files storing complete event | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DispatchingHlsFilesStoringCompleteEventError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Warning, "Failed to register HLS transmuxer | StreamPath: {StreamPath}")]
        public static partial void RegisteringHlsTransmuxerFailed(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "HLS transmuxer started | Transmuxer: {Transmuxer} | Identifier: {Identifier} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void HlsTransmuxerStarted(this ILogger logger, string transmuxer, Guid identifier, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Information, "HLS transmuxer ended | Transmuxer: {Transmuxer} | Identifier: {Identifier} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void HlsTransmuxerEnded(this ILogger logger, string transmuxer, Guid identifier, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Error, "An error occurred while processing HLS transmuxing | Transmuxer: {Transmuxer} | Identifier: {Identifier} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void ProcessingHlsTransmuxingError(this ILogger logger, string transmuxer, Guid identifier, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Trace, "Ts segment is flushed partially | Transmuxer: {Transmuxer} | Identifier: {Identifier} | StreamPath: {StreamPath} | Path: {Path} | SequenceNumber: {SequenceNumber}")]
        public static partial void TsSegmentFlushedPartially(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path, uint sequenceNumber);

        [LoggerMessage(LogLevel.Trace, "Ts segment is flushed | Transmuxer: {Transmuxer} | Identifier: {Identifier} | StreamPath: {StreamPath} | Path: {Path} | SequenceNumber: {SequenceNumber} | Duration: {Duration}")]
        public static partial void TsSegmentFlushed(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path, uint sequenceNumber, int duration);

        [LoggerMessage(LogLevel.Trace, "An outdated ts segment is deleted | Transmuxer: {Transmuxer} | Identifier: {Identifier} | StreamPath: {StreamPath} | Path: {Path}")]
        public static partial void OutdatedTsSegmentDeleted(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path);

        [LoggerMessage(LogLevel.Trace, "HLS manifest is updated | Transmuxer: {Transmuxer} | Identifier: {Identifier} | StreamPath: {StreamPath} | Path: {Path}")]
        public static partial void HlsManifestUpdated(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path);

        [LoggerMessage(LogLevel.Information, "HLS files are cleaned up | ManifestPath: {ManifestPath}")]
        public static partial void HlsCleanedUp(this ILogger logger, string manifestPath);

        [LoggerMessage(LogLevel.Error, "An error occurred while cleaning up HLS | ManifestPath: {ManifestPath}")]
        public static partial void HlsCleanupError(this ILogger logger, string manifestPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while obtaining stream information | InputPath: {InputPath}")]
        public static partial void ObtainingStreamInformationError(this ILogger logger, string inputPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while scheduling HLS cleanup | ManifestPath: {ManifestPath}")]
        public static partial void SchedulingHlsCleanupError(this ILogger logger, string manifestPath, Exception ex);
    }
}
