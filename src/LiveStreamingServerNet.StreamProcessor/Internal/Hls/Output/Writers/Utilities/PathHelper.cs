namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Utilities
{
    internal static class PathHelper
    {
        public static string GetRelativePath(string filePath, string dirPath)
        {
            return Path.GetRelativePath(dirPath, filePath).Replace('\\', '/');
        }
    }
}
