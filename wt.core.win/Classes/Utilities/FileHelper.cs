using System.IO;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    /// Utility class for file operations
    /// </summary>
    public static class FileHelper
    {
        /// <summary/>
        public static string EnsureAbsolutePath(string path, string basePath)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(basePath, path);
        }
    }
}