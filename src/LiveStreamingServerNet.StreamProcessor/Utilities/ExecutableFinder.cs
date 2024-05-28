namespace LiveStreamingServerNet.StreamProcessor.Utilities
{
    public static class ExecutableFinder
    {
        public static string? FindExecutableFromPATH(string executableName)
        {
            foreach (string path in GetPaths())
            {
                var executable = FindExecutable(path, executableName);
                if (executable != null)
                    return executable.FullName;
            }

            return null;
        }

        public static FileInfo? FindExecutable(string directoryPath, string executableName)
        {
            if (!Directory.Exists(directoryPath))
                return null;

            IEnumerable<FileInfo> files = new DirectoryInfo(directoryPath).GetFiles();
            return DoFindExecutable(files, executableName);
        }

        private static FileInfo? DoFindExecutable(IEnumerable<FileInfo> files, string executableName)
        {
            return files.FirstOrDefault((x) =>
                x.Name.Equals(executableName, StringComparison.InvariantCultureIgnoreCase) ||
                x.Name.Equals(executableName + ".exe", StringComparison.InvariantCultureIgnoreCase));
        }

        private static IEnumerable<string> GetPaths()
        {
            var pathEnvVar = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnvVar))
                foreach (var path in pathEnvVar.Split(Path.PathSeparator))
                    yield return path;

            var entryPath = Directory.GetCurrentDirectory();
            if (!string.IsNullOrEmpty(entryPath))
                yield return entryPath;
        }
    }
}
