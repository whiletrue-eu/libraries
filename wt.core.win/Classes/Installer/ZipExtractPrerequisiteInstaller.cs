using System;
using System.IO;
using System.IO.Compression;

namespace WhileTrue.Classes.Installer
{
    /// <summary>
    /// Prerequisite installer that extracts a ZIP file into a target folder for prerequisite installation
    /// </summary>
    [Serializable]
    public class ZipExtractPrerequisiteInstaller : PrerequisiteBase
    {
        public string ZipFile { get; }
        public string DestinationPath { get; }

        /// <summary/>
        public ZipExtractPrerequisiteInstaller(string name, bool requiresAdmin, string downloadId, Func<bool> alreadyInstalled, string zipFile, string destinationPath) : base(name, requiresAdmin, alreadyInstalled, downloadId)
        {
            this.ZipFile = zipFile;
            this.DestinationPath = destinationPath;
        }

        /// <summary>
        /// Performs installation of the rperequisite
        /// </summary>
        public override void DoInstall()
        {
            System.IO.Compression.ZipFile.ExtractToDirectory(this.ZipFile, this.DestinationPath);
            if (Directory.Exists(this.DestinationPath) == false)
            {
                throw new DirectoryNotFoundException();
            }
        }
    }
}