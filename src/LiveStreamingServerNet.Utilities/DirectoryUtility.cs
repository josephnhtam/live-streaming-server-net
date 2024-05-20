namespace LiveStreamingServerNet.Utilities
{
    public static class DirectoryUtility
    {
        public static void CreateDirectoryIfNotExists(string? path)
        {
            if (!string.IsNullOrEmpty(path))
                new DirectoryInfo(path).Create();
        }
    }
}
