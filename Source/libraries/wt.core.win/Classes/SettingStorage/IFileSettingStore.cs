using System.IO;

namespace WhileTrue.Classes.SettingStorage
{
    /// <summary>
    /// Provides isolated storage of files
    /// </summary>
    public interface IFileSettingStore
    {
        /// <summary/>
        FileStream CreateFile(string fileName, FileMode mode, FileAccess access, FileShare share);
        /// <summary/>
        FileInfo[] GetFileNames(string searchPattern);
        /// <summary/>
        void DeleteFile(string fileName);
    }
}