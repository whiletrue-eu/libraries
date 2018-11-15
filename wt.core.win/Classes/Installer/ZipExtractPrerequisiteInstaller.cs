using System;
using System.IO;

namespace WhileTrue.Classes.Installer
{
    /// <summary>
    ///     Prerequisite installer that extracts a ZIP file into a target folder for prerequisite installation
    /// </summary>
    [Serializable]
    public class ZipExtractPrerequisiteInstaller : PrerequisiteBase
    {
        /// <summary />
        public ZipExtractPrerequisiteInstaller(string name, bool requiresAdmin, string downloadId,
            Func<bool> alreadyInstalled, string zipFile, string destinationPath) : base(name, requiresAdmin,
            alreadyInstalled, downloadId)
        {
            ZipFile = zipFile;
            DestinationPath = destinationPath;
        }

        public string ZipFile { get; }
        public string DestinationPath { get; }

        /// <summary>
        ///     Performs installation of the rperequisite
        /// </summary>
        public override void DoInstall()
        {
            System.IO.Compression.ZipFile.ExtractToDirectory(ZipFile, DestinationPath);
            if (Directory.Exists(DestinationPath) == false) throw new DirectoryNotFoundException();
        }
    }
}