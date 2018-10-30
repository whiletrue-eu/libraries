using System.IO;
using System.Xml.XPath;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.XTransformer
{
    internal class FileExtensionMethods
    {
        private readonly string baseDir;

        public FileExtensionMethods(string baseDir)
        {
            this.baseDir = baseDir;
        }

        public string Read(string path)
        {
            return File.ReadAllText(FileHelper.EnsureAbsolutePath(path, baseDir));
        }

        public string Write(string path, string content)
        {
            var FilePath = FileHelper.EnsureAbsolutePath(path, baseDir);
            var DirectoryPath = Path.GetDirectoryName(FilePath);
            if (Directory.Exists(DirectoryPath) == false) Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(FilePath, content);
            return "";
        }

        public IXPathNavigable ReadXml(string path)
        {
            return new XPathDocument(new StringReader(Read(path)));
        }

        public string WriteXml(string path, IXPathNavigable content)
        {
            return Write(path, content.CreateNavigator().OuterXml);
        }
    }
}