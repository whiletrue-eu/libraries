namespace WhileTrue.Classes.Installer
{
    public partial class InstallWindowModel
    {
        /// <summary>
        /// State while the installation is performed. The installation consists of (optional) download and installation of missing packages
        /// </summary>
        public class InstallationStatus : StatusBase
        {
            /// <summary>
            /// Base class for download status
            /// </summary>
            public class DownloadStatusBase 
            {
            }

            /// <summary>
            /// Download state while download is performed
            /// </summary>
            public class DownloadingStatus : DownloadStatusBase
            {

                /// <summary/>
                public DownloadingStatus(int numberOfPackagesToDownload, int acquiredPackages, string acquisitionStatus)
                {
                    this.NumberOfPackagesToDownload = numberOfPackagesToDownload;
                    this.AcquiredPackages = acquiredPackages;
                    this.AcquisitionStatus = acquisitionStatus;
                }

                /// <summary>
                /// Number of packages that need to be downloaded
                /// </summary>
                public int NumberOfPackagesToDownload { get; }

                /// <summary>
                /// Number of packages that are already downloaded
                /// </summary>
                public int AcquiredPackages { get; }

                /// <summary>
                /// Information about the current download(s)
                /// </summary>
                public string AcquisitionStatus { get; }
            }

            /// <summary>
            /// Download state when all downloads are fnished
            /// </summary>
            public class DownloadFinishedStatus : DownloadStatusBase
            {
            }

            /// <summary>
            /// Base class for installation status
            /// </summary>
            public class InstallationStatusBase
            {
            }

            /// <summary>
            /// Installation status if installation is waiting for download of the first package to end
            /// </summary>
            public class InstallationWaitingForDownloadStatus : InstallationStatusBase
            {
            }

            /// <summary>
            /// INstallation status while installing prerequisites
            /// </summary>
            public class InstallingStatus : InstallationStatusBase
            {
                /// <summary/>
                public InstallingStatus(int numberOfPackagesToInstall, int installedPackages, string installationStatus)
                {
                    this.NumberOfPackagesToInstall = numberOfPackagesToInstall;
                    this.InstalledPackages = installedPackages;
                    this.InstallationStatus = installationStatus;
                }

                /// <summary>
                /// Number of package to be installed
                /// </summary>
                public int NumberOfPackagesToInstall { get; }

                /// <summary>
                /// Number of packages already installed
                /// </summary>
                public int InstalledPackages { get; }

                /// <summary>
                /// Information about the currently installed package
                /// </summary>
                public string InstallationStatus { get; }

            }

            /// <summary/>
            public InstallationStatus(InstallationStatusBase installation, DownloadStatusBase download)
            {
                this.Installation = installation;
                this.Download = download;
            }

            /// <summary>
            /// Status for the Installation part
            /// </summary>
            public InstallationStatusBase Installation { get; }

            /// <summary>
            /// Status for the download part
            /// </summary>
            public DownloadStatusBase Download { get; }
        }
    }
}