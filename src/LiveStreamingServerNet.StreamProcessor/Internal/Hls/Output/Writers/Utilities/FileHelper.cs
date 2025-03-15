using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Utilities
{
    internal static class FileHelper
    {
        public static async Task WriteToFileAsync(
           string outputPath, string content, CancellationToken cancellationToken = default)
        {
            var tempPath = $"{outputPath}.tmp";

            try
            {
                await File.WriteAllTextAsync(tempPath, content, cancellationToken);
                File.Move(tempPath, outputPath, true);
            }
            catch
            {
                ErrorBoundary.Execute(() => File.Delete(tempPath));
                await File.WriteAllTextAsync(outputPath, content, cancellationToken);
            }
        }
    }
}
