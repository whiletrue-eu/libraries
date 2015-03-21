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

        public string read(string path)
        {
            return File.ReadAllText(FileHelper.EnsureAbsolutePath(path, this.baseDir));
        }

        public string write(string path, string content)
        {
            string FilePath = FileHelper.EnsureAbsolutePath(path, this.baseDir);
            string DirectoryPath = Path.GetDirectoryName(FilePath);
            if (Directory.Exists(DirectoryPath)==false)
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            File.WriteAllText(FilePath, content);
            return "";
        }

        public IXPathNavigable readXml(string path)
        {
            return new XPathDocument( new StringReader(this.read(path)));
        }

        public string writeXml(string path, IXPathNavigable content)
        {
            return this.write(path, content.CreateNavigator().OuterXml);
        }
    }
}