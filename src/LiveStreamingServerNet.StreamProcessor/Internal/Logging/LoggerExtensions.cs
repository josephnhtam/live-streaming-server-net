using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Information, "Stream processor started (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void StreamProcessorStarted(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Information, "Stream processor stopped (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void StreamProcessorStopped(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Error, "Stream processor error (InputPath={InputPath}, StreamPath={StreamPath})")]
        public static partial void StreamProcessorError(this ILogger logger, string inputPath, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching stream processor started event (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void DispatchingStreamProcessorStartedEventError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching stream processor stopped event (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void DispatchingStreamProcessorStoppedEventError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while running HLS uploader (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void RunningHlsUploaderError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while uploading HLS to store (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void UploadingHlsToStoreError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while delete outdated segments (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void DeletingOutdatedSegmentsError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching HLS files stored event (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void DispatchingHlsFilesStoredEventError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching HLS files storing complete event (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void DispatchingHlsFilesStoringCompleteEventError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Warning, "Failed to register HLS transmuxer (StreamPath={StreamPath})")]
        public static partial void RegisteringHlsTransmuxerFailed(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Information, "HLS transmuxer started (Transmuxer={Transmuxer}, Identifier={Identifier}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void HlsTransmuxerStarted(this ILogger logger, string transmuxer, Guid identifier, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Information, "HLS transmuxer ended (Transmuxer={Transmuxer}, Identifier={Identifier}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void HlsTransmuxerEnded(this ILogger logger, string transmuxer, Guid identifier, string outputPath, string streamPath);

        [LoggerMessage(LogLevel.Error, "An error occurred while processing HLS transmuxing (Transmuxer={Transmuxer}, Identifier={Identifier}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void ProcessingHlsTransmuxingError(this ILogger logger, string transmuxer, Guid identifier, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Trace, "Ts segment is flushed partially (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath}, Path={Path}, SequenceNumber={SequenceNumber})")]
        public static partial void TsSegmentFlushedPartially(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path, uint sequenceNumber);

        [LoggerMessage(LogLevel.Trace, "Ts segment is flushed (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath}, Path={Path}, SequenceNumber={SequenceNumber}, Duration={Duration})")]
        public static partial void TsSegmentFlushed(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path, uint sequenceNumber, uint duration);

        [LoggerMessage(LogLevel.Trace, "An outdated segment is deleted (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath}, Path={Path})")]
        public static partial void OutdatedSegmentDeleted(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path);

        [LoggerMessage(LogLevel.Trace, "HLS manifest is updated (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath}, Path={Path})")]
        public static partial void HlsManifestUpdated(this ILogger logger, string transmuxer, Guid identifier, string streamPath, string path);

        [LoggerMessage(LogLevel.Information, "HLS files are cleaned up (ManifestPath={ManifestPath})")]
        public static partial void HlsCleanedUp(this ILogger logger, string manifestPath);

        [LoggerMessage(LogLevel.Error, "An error occurred while cleaning up HLS (ManifestPath={ManifestPath})")]
        public static partial void HlsCleanupError(this ILogger logger, string manifestPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while obtaining stream information (InputPath={InputPath})")]
        public static partial void ObtainingStreamInformationError(this ILogger logger, string inputPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while scheduling HLS cleanup (ManifestPath={ManifestPath})")]
        public static partial void SchedulingHlsCleanupError(this ILogger logger, string manifestPath, Exception ex);

        [LoggerMessage(LogLevel.Information, "Starting FFmpeg process (Arguments={Arguments})")]
        public static partial void StartingFFmpegProcess(this ILogger logger, string arguments);

        [LoggerMessage(LogLevel.Information, "FFmpeg process started with Process ID {ProcessId}")]
        public static partial void FFmpegProcessStarted(this ILogger logger, int processId);

        [LoggerMessage(Level = LogLevel.Debug, Message = "FFmpeg process stopping with Process ID {ProcessId}")]
        public static partial void FFmpegProcessStopping(this ILogger logger, int processId);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Killing FFmpeg process with Process ID {ProcessId}")]
        public static partial void KillingFFmpegProcess(this ILogger logger, int processId);

        [LoggerMessage(Level = LogLevel.Debug, Message = "FFmpegTranscodingStream transcoding process started")]
        public static partial void TranscodingProcessStarted(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Transcoding process was canceled")]
        public static partial void TranscodingProcessCanceled(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred during the transcoding process")]
        public static partial void TranscodingProcessError(this ILogger logger, Exception ex);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Starting WriteBufferAsync")]
        public static partial void WriteBufferAsyncStarted(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Exiting WriteBufferAsync")]
        public static partial void WriteBufferAsyncEnding(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Starting ReceiveTranscodedBufferAsync")]
        public static partial void ReceiveTranscodedBufferAsyncStarted(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Exiting ReceiveTranscodedBufferAsync")]
        public static partial void ReceiveTranscodedBufferAsyncEnding(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Read {BytesRead} bytes from FFmpeg output stream")]
        public static partial void BytesReadFromOutput(this ILogger logger, int bytesRead);

        [LoggerMessage(Level = LogLevel.Error, Message = "Error occurred in ReceiveTranscodedBufferAsync")]
        public static partial void ReceiveTranscodedBufferAsyncError(this ILogger logger, Exception ex);

        [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while writing media buffer to FFmpegTranscodingStream")]
        public static partial void WriteMediaBufferError(this ILogger logger, Exception ex);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Header sent to FFmpeg process")]
        public static partial void HeaderSentToFFmpeg(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Releasing remaining buffers from send buffer channel")]
        public static partial void ReleasingBuffers(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while starting the FFmpeg process")]
        public static partial void StartingFFmpegProcessError(this ILogger logger, Exception ex);

        [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while stopping the FFmpeg process with Process ID {ProcessId}")]
        public static partial void StoppingProcessError(this ILogger logger, int processId, Exception ex);

        [LoggerMessage(Level = LogLevel.Information, Message = "Subtitle transcriber started (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath})")]
        public static partial void SubtitleTranscriberStarted(this ILogger logger, string transmuxer, Guid identifier, string streamPath);

        [LoggerMessage(Level = LogLevel.Information, Message = "Subtitle transcriber is stopping (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath})")]
        public static partial void SubtitleTranscriberStopping(this ILogger logger, string transmuxer, Guid identifier, string streamPath);

        [LoggerMessage(Level = LogLevel.Information, Message = "Subtitle transcriber stopped (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath})")]
        public static partial void SubtitleTranscriberStopped(this ILogger logger, string transmuxer, Guid identifier, string streamPath);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Subtitle segment created (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath}, SequenceNumber={SequenceNumber}, Path={OutputPath}, Timestamp={Timestamp}ms, Duration={Duration}ms)")]
        public static partial void SubtitleSegmentCreated(this ILogger logger, string transmuxer, Guid identifier, string streamPath, uint sequenceNumber, string outputPath, uint timestamp, uint duration);

        [LoggerMessage(Level = LogLevel.Information, Message = "Audio publishing started (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath})")]
        public static partial void AudioPublishingStarted(this ILogger logger, string transmuxer, Guid identifier, string streamPath);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcription processing started (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath})")]
        public static partial void TranscriptionProcessingStarted(this ILogger logger, string transmuxer, Guid identifier, string streamPath);

        [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while publishing audio (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath})")]
        public static partial void AudioPublishingError(this ILogger logger, string transmuxer, Guid identifier, string streamPath, Exception ex);

        [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while processing transcription results (Transmuxer={Transmuxer}, Identifier={Identifier}, StreamPath={StreamPath})")]
        public static partial void TranscriptionProcessingError(this ILogger logger, string transmuxer, Guid identifier, string streamPath, Exception ex);
    }
}
