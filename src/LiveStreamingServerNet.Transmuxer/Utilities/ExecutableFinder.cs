namespace LiveStreamingServerNet.Transmuxer.Internal.Utilities
{
    public static class ExecutableFinder
    {
        public static string? FindExecutableFromPATH(string executableName)
        {
            var paths = Environment.GetEnvironmentVariable("PATH")!.Split(Path.PathSeparator);

            foreach (string path in paths)
            {
                if (FindExecutable(path, executableName))
                {
                    return path;
                }
            }

            return null;
        }

        public static bool FindExecutable(string directoryPath, string executableName)
        {
            if (Directory.Exists(directoryPath))
            {
                IEnumerable<FileInfo> files = new DirectoryInfo(directoryPath).GetFiles();
                return HasExecutableFile(files, executableName);
            }
            return false;
        }

        private static bool HasExecutableFile(IEnumerable<FileInfo> files, string executableName)
        {
            return files.FirstOrDefault((x) =>
                x.Name.Equals(executableName, StringComparison.InvariantCultureIgnoreCase) ||
                x.Name.Equals(executableName + ".exe", StringComparison.InvariantCultureIgnoreCase)) != null;
        }
    }
}
