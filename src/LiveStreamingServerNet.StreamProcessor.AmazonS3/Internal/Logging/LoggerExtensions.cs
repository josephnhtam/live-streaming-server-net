using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.AmazonS3.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred while uploading TS segment | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath} | TsSegmentPath: {TsSegmentPath}")]
        public static partial void UploadingTsSegmentError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, string tsSegmentPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while deleting TS segment | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath}")]
        public static partial void DeletingTsSegmentError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while uploading manifest file | StreamProcessor: {StreamProcessor} | Identifier: {Identifier} | InputPath: {InputPath} | OutputPath: {OutputPath} | StreamPath: {StreamPath} | ManifestName: {ManifestName}")]
        public static partial void UploadingManifestFileError(this ILogger logger, string streamProcessor, Guid identifier, string inputPath, string outputPath, string streamPath, string manifestName, Exception ex);
    }
}
