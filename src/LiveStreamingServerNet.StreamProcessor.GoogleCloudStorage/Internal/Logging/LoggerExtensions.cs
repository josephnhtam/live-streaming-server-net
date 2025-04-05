using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred while uploading segment (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath}, SegmentPath={SegmentPath})")]
        public static partial void UploadingSegmentError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, string segmentPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while deleting segment (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath})")]
        public static partial void DeletingSegmentError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while uploading manifest file (StreamProcessor={StreamProcessor}, Identifier={Identifier}, InputPath={InputPath}, OutputPath={OutputPath}, StreamPath={StreamPath}, ManifestName={ManifestName})")]
        public static partial void UploadingManifestFileError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, string manifestName, Exception ex);
    }
}
