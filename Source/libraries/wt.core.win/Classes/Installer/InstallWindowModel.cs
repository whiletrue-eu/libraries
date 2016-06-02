using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
        private readonly Action<string, Action<string>> downloadFunc;
        private StatusBase status;
        private object statusLock= new object();

        /// <summary/>
        public InstallWindowModel(PrerequisiteBase[] prerequisites, Action<string,Action<string>> downloadFunc )
        {
            this.prerequisites = prerequisites;
            this.downloadFunc = downloadFunc;

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
                         bool MustWaitForDownload = PackagesToInstall < PackagesToDownload;
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
                         NamedPipeServerStream PipeServer=null;
                         Process AdminInstall=null;
                         StreamReader Reader=null;

                         if (IsAdminRequired)
                         {
                             string PipeName = Guid.NewGuid().ToString("X");
                             PipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte);

                             ProcessStartInfo AdminInstallStartInfo = new ProcessStartInfo(Environment.GetCommandLineArgs()[0], PipeName)
                                                                      {
                                                                          WorkingDirectory = Environment.CurrentDirectory,
                                                                          Verb = "runas",
                                                                          CreateNoWindow = true,
                                                                          WindowStyle = ProcessWindowStyle.Hidden
                                                                      };
                             AdminInstall = Process.Start(AdminInstallStartInfo);
                             PipeServer.WaitForConnection();
                             Reader = new StreamReader(PipeServer);
                         }

                         //Start download for all packages in parallel
                         foreach (PrerequisiteBase Prerequisite in this.prerequisites.Where(_=>_.DownloadId!=null))
                         {
                             this.downloadFunc(
                                 Prerequisite.DownloadId,
                                 id =>
                                 {
                                     //TODO: Handle download errors
                                     lock (this.statusLock)
                                     {
                                         if (this.status is InstallationErrorStatus)
                                         {
                                             //Ignore if there is already an error reported. Can happen in race conditions
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
                         while (this.prerequisites.Any(_=>_.WasInstalled==false))
                         {
                             PrerequisiteBase Prerequisite=null;
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
                                         this.status = new InstallationStatus(
                                             new InstallationStatus.InstallationWaitingForDownloadStatus(),
                                             ((InstallationStatus) this.status).Download
                                             );
                                     }
                                     InstallQueueAdded.WaitOne();
                                 }
                             } while (Prerequisite == null);


                             lock (this.statusLock)
                             {
                                 this.status = new InstallationStatus(
                                     new InstallationStatus.InstallingStatus(PackagesToInstall, PackagesInstalled, Prerequisite.Name),
                                     ((InstallationStatus)this.status).Download
                                     );
                             }

                             //if needed, run process with admin rights to execute installations
                             if (Prerequisite.RequiresAdmin)
                             {
                                 IFormatter Serializer = new BinaryFormatter();
                                 Serializer.Serialize(PipeServer, Prerequisite);
                                 PipeServer.WaitForPipeDrain();

                                 string Result = Reader.ReadLine();
                                 if (string.IsNullOrEmpty(Result))
                                 {
                                     Prerequisite.SetInstalled();
                                 }
                                 else
                                 {
                                     lock (this.statusLock)
                                     {
                                         this.status = new InstallationErrorStatus(Result);
                                     }
                                     //TODO: Stop downloads
                                     break;
                                 }
                             }
                             else
                             {
                                 try
                                 {
                                     Prerequisite.DoInstall();
                                     Prerequisite.SetInstalled();
                                 }
                                 catch (Exception Error)
                                 {
                                     lock (this.statusLock)
                                     {
                                         this.status = new InstallationErrorStatus(Error.Message);
                                     }
                                     //TODO: Stop downloads
                                     break;
                                 }
                             }
                             PackagesInstalled++;
                             lock (this.statusLock)
                             {
                                 this.status = new InstallationStatus(
                                     new InstallationStatus.InstallingStatus(PackagesToInstall, PackagesInstalled, Prerequisite.Name),
                                     ((InstallationStatus)this.status).Download
                                     );
                             }
                         }

                         //Installation finished or error occured. Close client program if it was started
                         if (AdminInstall != null)
                         {
                             PipeServer.Close();
                             AdminInstall.WaitForExit();
                             if (AdminInstall.ExitCode != 0)
                             {
                                 lock (this.statusLock)
                                 {
                                     this.status = new InstallationErrorStatus(null);
                                 }
                             }
                         }

                         lock (this.statusLock)
                         {
                             if (this.status is InstallationStatus)
                             {
                                 this.status = new InstallationSuccessStatus();
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