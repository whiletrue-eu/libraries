using System.IO;

namespace WhileTrue.Classes.SettingStorage
{
    internal class FileSettingStore : IFileSettingStore
    {
        private readonly string path;

        internal FileSettingStore(string path, string name)
        {
            this.path = Path.Combine(path, name);

            Directory.CreateDirectory(this.path);
        }
        public FileStream CreateFile(string fileName, FileMode mode, FileAccess access, FileShare share)
        {

            return new FileStream(Path.Combine(this.path,fileName), mode, access, share);
        }

        public FileInfo[] GetFileNames(string searchPattern)
        {
            return new DirectoryInfo(this.path).GetFiles(searchPattern);
        }

        public void DeleteFile(string fileName)
        {
            Directory.Delete(Path.Combine(this.path, fileName));
        }  
    }
}