using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Installer
{
    /// <summary>
    ///     View Model for prerequisite installation wizard
    /// </summary>
    public partial class InstallWindowModel : ObservableObject
    {
        private readonly IAdminProcessConnector adminProcessConnector;
        private readonly Action<string, Action<string, double>, Action<string>> downloadFunc;


        private readonly PrerequisiteBase[] prerequisites;
        private StatusBase status;
        private object statusLock = new object();

        /// <summary />
        public InstallWindowModel(PrerequisiteBase[] prerequisites,
            Action<string, Action<string, double>, Action<string>> downloadFunc)
            : this(prerequisites, downloadFunc, new AdminProcessConnector())
        {
        }

        /// <summary />
        internal InstallWindowModel(PrerequisiteBase[] prerequisites,
            Action<string, Action<string, double>, Action<string>> downloadFunc,
            IAdminProcessConnector adminProcessConnector)
        {
            Contract.Requires(prerequisites.All(_ => _.IsAlreadyInstalled == false));

            this.prerequisites = prerequisites;
            this.downloadFunc = downloadFunc;
            this.adminProcessConnector = adminProcessConnector;

            status = new PreperationStatus(
                prerequisites.Select(_ => _.Name).ToArray(),
                prerequisites.Any(_ => _.RequiresAdmin),
                new DelegateCommand(SetUpSystem));
        }

        /// <summary>
        ///     Current status
        /// </summary>
        public StatusBase Status
        {
            get => status;
            private set => SetAndInvoke(ref status, value);
        }

        private void SetUpSystem()
        {
            new Task(() =>
            {
                var PackagesToInstall = prerequisites.Length;
                var PackagesToDownload = prerequisites.Count(_ => _.DownloadId != null);
                var PackagesDownloaded = 0;
                var PackagesInstalled = 0;

                //Set up initial status for download and imnstallation
                var IsAdminRequired = prerequisites.Any(_ => _.RequiresAdmin);
                var DownloadsNeeded = PackagesToDownload > 0;
                var MustWaitForDownload = PackagesToInstall == PackagesToDownload;
                Status = new InstallationStatus(
                    MustWaitForDownload
                        ? (InstallationStatus.InstallationStatusBase)
                        new InstallationStatus.InstallationWaitingForDownloadStatus()
                        : new InstallationStatus.InstallingStatus(PackagesToInstall, 0, null),
                    DownloadsNeeded
                        ? new InstallationStatus.DownloadingStatus(PackagesToDownload, 0, null)
                        : default(InstallationStatus.DownloadStatusBase));

                //Set up queue for installable packages. All packages that do not need to be downloaded are added right now as installation can directly start
                var InstallQueue = new Queue<PrerequisiteBase>();
                var InstallQueueAdded = new AutoResetEvent(false);
                prerequisites.Where(_ => _.DownloadId == null).ForEach(_ => InstallQueue.Enqueue(_));


                //If administrative rights are required,set up client process to delegate those installations to.
                if (IsAdminRequired) adminProcessConnector.LaunchProcess();

                //Start download for all packages in parallel
                foreach (var Prerequisite in prerequisites.Where(_ => _.DownloadId != null))
                    downloadFunc(
                        Prerequisite.DownloadId,
                        (id, progress) =>
                        {
                            lock (statusLock)
                            {
                                prerequisites.First(_ => _.DownloadId == id).DownloadProgress = progress;
                                var OverallProgress = prerequisites.Where(_ => _.DownloadId != null)
                                    .Sum(_ => _.DownloadProgress);
                                lock (statusLock)
                                {
                                    if (Status is InstallationErrorStatus) return;
                                }

                                // Update download status
                                Status = new InstallationStatus(((InstallationStatus) Status).Installation,
                                    new InstallationStatus.DownloadingStatus(PackagesToDownload, OverallProgress,
                                        null));
                            }
                        },
                        id =>
                        {
                            //TODO: Handle download errors
                            lock (statusLock)
                            {
                                if (Status is InstallationErrorStatus) return;
                            }

                            lock (InstallQueue)
                            {
                                InstallQueue.Enqueue(prerequisites.First(_ => _.DownloadId == id));
                                InstallQueueAdded.Set();
                                PackagesDownloaded++;
                                lock (statusLock)
                                {
                                    if (PackagesToDownload != PackagesDownloaded)
                                        Status = new InstallationStatus(((InstallationStatus) Status).Installation,
                                            new InstallationStatus.DownloadingStatus(PackagesToDownload,
                                                PackagesDownloaded, null));
                                    else
                                        Status = new InstallationStatus(((InstallationStatus) Status).Installation,
                                            new InstallationStatus.DownloadFinishedStatus());
                                }
                            }
                        });


                //repeat until all prerequisites are installed
                while (prerequisites.Any(_ => _.WasInstalled == false))
                {
                    PrerequisiteBase Prerequisite = null;
                    do
                    {
                        lock (InstallQueue)
                        {
                            if (InstallQueue.Count > 0) Prerequisite = InstallQueue.Dequeue();
                        }

                        if (Prerequisite == null)
                        {
                            lock (statusLock)
                            {
                                Status = new InstallationStatus(
                                    new InstallationStatus.InstallationWaitingForDownloadStatus(),
                                    ((InstallationStatus) Status).Download
                                );
                            }

                            InstallQueueAdded.WaitOne();
                        }
                    } while (Prerequisite == null);


                    lock (statusLock)
                    {
                        Status = new InstallationStatus(
                            new InstallationStatus.InstallingStatus(PackagesToInstall, PackagesInstalled,
                                Prerequisite.Name),
                            ((InstallationStatus) Status).Download
                        );
                    }

                    try
                    {
                        //if needed, run process with admin rights to execute installations
                        if (Prerequisite.RequiresAdmin)
                            adminProcessConnector.DoInstallRemote(Prerequisite);
                        else
                            Prerequisite.DoInstall();
                        Prerequisite.SetInstalled();
                    }
                    catch (Exception Error)
                    {
                        lock (statusLock)
                        {
                            Status = new InstallationErrorStatus(Error.Message);
                        }

                        //TODO: Stop downloads
                        break;
                    }

                    PackagesInstalled++;
                    lock (statusLock)
                    {
                        Status = new InstallationStatus(
                            new InstallationStatus.InstallingStatus(PackagesToInstall, PackagesInstalled,
                                Prerequisite.Name),
                            ((InstallationStatus) Status).Download
                        );
                    }
                }

                //Installation finished or error occured. Close client program if it was started
                adminProcessConnector.EndIfStartedAndWaitForExit();

                lock (statusLock)
                {
                    if (Status is InstallationStatus) Status = new InstallationSuccessStatus();
                }
            }).Start();
        }

        /// <summary>
        ///     base class for state specific models
        /// </summary>
        public abstract class StatusBase
        {
        }
    }
}