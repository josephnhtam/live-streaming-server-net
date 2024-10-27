namespace LiveStreamingServerNet.StreamProcessor.Utilities
{
    /// <summary>
    /// Provides utilities for finding executable files in the system.
    /// </summary>
    public static class ExecutableFinder
    {
        /// <summary>
        /// Searches for an executable in all directories listed in the PATH environment variable and current directory.
        /// </summary>
        /// <param name="executableName">The name of the executable to find.</param>
        /// <returns>The full path to the executable if found, null otherwise.</returns>
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

        /// <summary>
        /// Searches for an executable in a specific directory.
        /// </summary>
        /// <param name="directoryPath">The directory path to search in.</param>
        /// <param name="executableName">The name of the executable to find.</param>
        /// <returns>FileInfo of the executable if found, null otherwise.</returns>
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
