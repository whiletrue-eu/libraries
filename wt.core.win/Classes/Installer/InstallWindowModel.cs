using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Installer
{
    /// <summary>
    /// View Model for prerequisite installation wizard
    /// </summary>
    public partial class InstallWindowModel: ObservableObject
    {
        /// <summary>
        /// base class for state specific models
        /// </summary>
        public abstract class StatusBase
        {
        }


        private readonly PrerequisiteBase[] prerequisites;
        private readonly Action<string, Action<string, double>, Action<string>> downloadFunc;
        private readonly IAdminProcessConnector adminProcessConnector;
        private StatusBase status;
        private object statusLock= new object();

        /// <summary/>
        public InstallWindowModel(PrerequisiteBase[] prerequisites, Action<string, Action<string, double>, Action<string>> downloadFunc )
            :this(prerequisites,downloadFunc, new AdminProcessConnector())
        {
        }     
        
        /// <summary/>
        internal InstallWindowModel(PrerequisiteBase[] prerequisites, Action<string,Action<string,double>, Action<string>> downloadFunc, IAdminProcessConnector adminProcessConnector )
        {
            Contract.Requires(prerequisites.All(_=>_.IsAlreadyInstalled==false));

            this.prerequisites = prerequisites;
            this.downloadFunc = downloadFunc;
            this.adminProcessConnector = adminProcessConnector;

            this.status = new PreperationStatus(
                prerequisites.Select(_ => _.Name).ToArray(),
                prerequisites.Any(_ => _.RequiresAdmin),
                new DelegateCommand(this.SetUpSystem));
        }

        /// <summary>
        /// Current status
        /// </summary>
        public StatusBase Status
        {
            get { return this.status; }
            private set { this.SetAndInvoke(ref this.status, value); }
        }

        private void SetUpSystem()
        {
            new Task(() =>
                     {
                         int PackagesToInstall = this.prerequisites.Length;
                         int PackagesToDownload = this.prerequisites.Count(_ => _.DownloadId != null);
                         int PackagesDownloaded = 0;
                         int PackagesInstalled = 0;

                         //Set up initial status for download and imnstallation
                         bool IsAdminRequired = this.prerequisites.Any(_ => _.RequiresAdmin);
                         bool DownloadsNeeded = PackagesToDownload>0;
                         bool MustWaitForDownload = PackagesToInstall == PackagesToDownload;
                         this.Status = new InstallationStatus(
                             MustWaitForDownload
                                 ? (InstallationStatus.InstallationStatusBase) new InstallationStatus.InstallationWaitingForDownloadStatus()
                                 : new InstallationStatus.InstallingStatus(PackagesToInstall, 0, null),
                             DownloadsNeeded
                                 ? new InstallationStatus.DownloadingStatus(PackagesToDownload, 0, null)
                                 : default(InstallationStatus.DownloadStatusBase));

                         //Set up queue for installable packages. All packages that do not need to be downloaded are added right now as installation can directly start
                         Queue<PrerequisiteBase> InstallQueue = new Queue<PrerequisiteBase>();
                         AutoResetEvent InstallQueueAdded = new AutoResetEvent(false);
                         this.prerequisites.Where(_=>_.DownloadId==null).ForEach(_=>InstallQueue.Enqueue(_));


                         //If administrative rights are required,set up client process to delegate those installations to.
                         if (IsAdminRequired)
                         {
                             this.adminProcessConnector.LaunchProcess();
                         }

                         //Start download for all packages in parallel
                         foreach (PrerequisiteBase Prerequisite in this.prerequisites.Where(_=>_.DownloadId!=null))
                         {
                             this.downloadFunc(
                                 Prerequisite.DownloadId,
                                 (id, progress) =>
                                 {
                                     lock (this.statusLock)
                                     {
                                         this.prerequisites.First(_ => _.DownloadId == id).DownloadProgress = progress;
                                         double OverallProgress = this.prerequisites.Where(_ => _.DownloadId != null).Sum(_ => _.DownloadProgress);
                                         lock (this.statusLock)
                                         {
                                             if (this.Status is InstallationErrorStatus)
                                             {
                                                 //Ignore if there is already an error reported. Can happen if multiple downloads are pending and one has a failure
                                                 return;
                                             }
                                         }
                                         // Update download status
                                         this.Status = new InstallationStatus(((InstallationStatus)this.Status).Installation, new InstallationStatus.DownloadingStatus(PackagesToDownload, OverallProgress, null));
                                     }
                                 },
                                 id =>
                                 {
                                     //TODO: Handle download errors
                                     lock (this.statusLock)
                                     {
                                         if (this.Status is InstallationErrorStatus)
                                         {
                                             //Ignore if there is already an error reported. Can happen if multiple downloads are pending and one has a failure
                                             return;
                                         }
                                     }
                                     lock (InstallQueue)
                                     {
                                         InstallQueue.Enqueue(this.prerequisites.First(_ => _.DownloadId == id));
                                         InstallQueueAdded.Set();
                                         PackagesDownloaded++;
                                         lock (this.statusLock)
                                         {
                                             if (PackagesToDownload != PackagesDownloaded)
                                             {
                                                 // Update download status
                                                 this.Status = new InstallationStatus(((InstallationStatus) this.Status).Installation, new InstallationStatus.DownloadingStatus(PackagesToDownload, PackagesDownloaded, null));
                                             }
                                             else
                                             {
                                                 // All downloads finished
                                                 this.Status = new InstallationStatus(((InstallationStatus) this.Status).Installation, new InstallationStatus.DownloadFinishedStatus());
                                             }
                                         }
                                     }
                                 });
                         }


                         //repeat until all prerequisites are installed
                         while (this.prerequisites.Any(_ => _.WasInstalled == false))
                         {
                             PrerequisiteBase Prerequisite = null;
                             do
                             {
                                 lock (InstallQueue)
                                 {
                                     if (InstallQueue.Count > 0)
                                     {
                                         Prerequisite = InstallQueue.Dequeue();
                                     }
                                 }
                                 if (Prerequisite == null)
                                 {
                                     lock (this.statusLock)
                                     {
                                         this.Status = new InstallationStatus(
                                             new InstallationStatus.InstallationWaitingForDownloadStatus(),
                                             ((InstallationStatus) this.Status).Download
                                             );
                                     }
                                     InstallQueueAdded.WaitOne();
                                 }
                             } while (Prerequisite == null);


                             lock (this.statusLock)
                             {
                                 this.Status = new InstallationStatus(
                                     new InstallationStatus.InstallingStatus(PackagesToInstall, PackagesInstalled, Prerequisite.Name),
                                     ((InstallationStatus) this.Status).Download
                                     );
                             }

                             try
                             {
                                 //if needed, run process with admin rights to execute installations
                                 if (Prerequisite.RequiresAdmin)
                                 {
                                     this.adminProcessConnector.DoInstallRemote(Prerequisite);
                                 }
                                 else
                                 {
                                     Prerequisite.DoInstall();
                                 }
                                 Prerequisite.SetInstalled();
                             }
                             catch (Exception Error)
                             {
                                 lock (this.statusLock)
                                 {
                                     this.Status = new InstallationErrorStatus(Error.Message);
                                 }
                                 //TODO: Stop downloads
                                 break;
                             }

                             PackagesInstalled++;
                             lock (this.statusLock)
                             {
                                 this.Status = new InstallationStatus(
                                     new InstallationStatus.InstallingStatus(PackagesToInstall, PackagesInstalled, Prerequisite.Name),
                                     ((InstallationStatus)this.Status).Download
                                     );
                             }
                         }

                         //Installation finished or error occured. Close client program if it was started
                         this.adminProcessConnector.EndIfStartedAndWaitForExit();

                         lock (this.statusLock)
                         {
                             if (this.Status is InstallationStatus)
                             {
                                 this.Status = new InstallationSuccessStatus();
                             }
                             else
                             {
                                 //Error status, leave as is
                             }
                         }
                     }).Start();
        }
    }
}