using System;
using System.IO.Compression;

namespace WhileTrue.Classes.Installer
{
    /// <summary>
    /// Prerequisite installer that extracts a ZIP file into a target folder for prerequisite installation
    /// </summary>
    [Serializable]
    public class ZipExtractPrerequisiteInstaller : PrerequisiteBase
    {
        private readonly string zipFile;
        private readonly string destinationPath;

        /// <summary/>
        public ZipExtractPrerequisiteInstaller(string name, bool requiresAdmin, string downloadId, Func<bool> alreadyInstalled, string zipFile, string destinationPath) : base(name, requiresAdmin, alreadyInstalled, downloadId)
        {
            this.zipFile = zipFile;
            this.destinationPath = destinationPath;
        }

        /// <summary>
        /// Performs installation of the rperequisite
        /// </summary>
        public override bool DoInstall()
        {
            ZipFile.ExtractToDirectory(this.zipFile, this.destinationPath);
            return true;
        }
    }
}