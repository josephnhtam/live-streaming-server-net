namespace LiveStreamingServerNet.Transmuxer.Internal.Utilities
{
    public static class ExecutableFinder
    {
        public static string? FindExecutableFromPATH(string executableName)
        {
            var paths = Environment.GetEnvironmentVariable("PATH")!.Split(Path.PathSeparator);

            foreach (string path in paths)
            {
                var executable = FindExecutable(path, executableName);
                if (executable != null)
                {
                    return executable.FullName;
                }
            }

            return null;
        }

        public static FileInfo? FindExecutable(string directoryPath, string executableName)
        {
            if (Directory.Exists(directoryPath))
            {
                IEnumerable<FileInfo> files = new DirectoryInfo(directoryPath).GetFiles();
                return DoFindExecutable(files, executableName);
            }
            return null;
        }

        private static FileInfo? DoFindExecutable(IEnumerable<FileInfo> files, string executableName)
        {
            return files.FirstOrDefault((x) =>
                x.Name.Equals(executableName, StringComparison.InvariantCultureIgnoreCase) ||
                x.Name.Equals(executableName + ".exe", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
