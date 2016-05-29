using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace WhileTrue.Classes.Installer
{
    [TestFixture]
    public class InstallWindowModelTest
    {
        public class TestPrerequisite : PrerequisiteBase
        {
            private readonly ManualResetEvent downloadCompleteEvent;
            private readonly ManualResetEvent downloadStartedEvent;
            private bool downloadSuccess;
            private readonly ManualResetEvent installCompleteEvent;
            private readonly ManualResetEvent installStartedEvent;
            private bool installSuccess;

            public TestPrerequisite(string name, bool requiresAdmin, bool alreadyInstalled, string downloadId) : base(name, requiresAdmin, ()=>alreadyInstalled, downloadId)
            {
                this.downloadCompleteEvent = new ManualResetEvent(false);
                this.installCompleteEvent = new ManualResetEvent(false);
                this.downloadStartedEvent = new ManualResetEvent(false);
                this.installStartedEvent = new ManualResetEvent(false);
            }

            public override void DoInstall()
            {
                this.installStartedEvent.Set();
                this.installCompleteEvent.WaitOne();
                this.DoInstallCalled = true;
                if (this.installSuccess == false)
                {
                    throw new Exception("FAIL!");
                }
            }

            public bool DoInstallCalled { get; private set; }

            public void MarkInstalled(bool success)
            {
                this.installSuccess = success;
                this.installCompleteEvent.Set();
            }

            public void WaitDownloadStarted()
            {
                this.downloadStartedEvent.WaitOne(Debugger.IsAttached?TimeSpan.FromDays(1) : TimeSpan.FromSeconds(10));
            }
            public void WaitInstallStarted()
            {
                this.installStartedEvent.WaitOne(Debugger.IsAttached ? TimeSpan.FromDays(1) : TimeSpan.FromSeconds(10));
            }


            public void MarkDownloaded(bool success)
            {
                this.downloadSuccess = success;
                this.downloadCompleteEvent.Set();
            }

            public void WaitDownloaded()
            {
                this.downloadStartedEvent.Set();
                this.downloadCompleteEvent.WaitOne();
            }
        }

        private class AdminProcess : IAdminProcessConnector
        {
            public bool AdminProcessLaunched { get; private set; }
            private readonly AutoResetEvent installStartedEvent = new AutoResetEvent(false);
            private bool installSuccess;
            private readonly AutoResetEvent installCompleteEvent = new AutoResetEvent(false);

            public void LaunchProcess()
            {
                this.AdminProcessLaunched = true;
            }

            public void EndIfStartedAndWaitForExit()
            {
            }



            public void WaitInstallStarted()
            {
                this.installStartedEvent.WaitOne(Debugger.IsAttached ? TimeSpan.FromDays(1) : TimeSpan.FromSeconds(10));
            }


            public void DoInstallRemote(PrerequisiteBase prerequisite)
            {
                this.installStartedEvent.Set();
                this.installCompleteEvent.WaitOne();
                if (this.installSuccess == false)
                {
                    throw new Exception("FAIL!");
                }
            }

            public void MarkInstalled(bool success)
            {
                this.installSuccess = success;
                this.installCompleteEvent.Set();
            }
        }

        private static void WaitForStateChange<T>(InstallWindowModel model)
        {
            TimeSpan Timeout = Debugger.IsAttached ? TimeSpan.FromDays(1) : TimeSpan.FromSeconds(10);

            if (typeof(InstallWindowModel.StatusBase).IsAssignableFrom(typeof(T)))
            {
                while (model.Status.GetType() != typeof(T) && Timeout.Ticks > 0)
                {
                    Thread.Sleep(100);
                    Timeout = Timeout - TimeSpan.FromMilliseconds(100);
                }
                Assert.That(model.Status, Is.TypeOf<T>());
            }
            else if (typeof(InstallWindowModel.InstallationStatus.InstallationStatusBase).IsAssignableFrom(typeof(T)))
            {
                while ( (model.Status.GetType() != typeof(InstallWindowModel.InstallationStatus) || ((InstallWindowModel.InstallationStatus)model.Status).Installation.GetType() != typeof(T) ) && Timeout.Ticks > 0)
                {
                    Thread.Sleep(100);
                    Timeout = Timeout - TimeSpan.FromMilliseconds(100);
                }
                Assert.That(((InstallWindowModel.InstallationStatus)model.Status).Installation, Is.TypeOf<T>());
            }
            else if (typeof(InstallWindowModel.InstallationStatus.DownloadStatusBase).IsAssignableFrom(typeof(T)))
            {
                while ( (model.Status.GetType() != typeof(InstallWindowModel.InstallationStatus) || ((InstallWindowModel.InstallationStatus)model.Status).Download.GetType() != typeof(T) ) && Timeout.Ticks > 0)
                {
                    Thread.Sleep(100);
                    Timeout = Timeout - TimeSpan.FromMilliseconds(100);
                }
                Assert.That(((InstallWindowModel.InstallationStatus)model.Status).Download, Is.TypeOf<T>());
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private void DownloadFunc(string downloadId, Action<string> downloadedCallback, TestPrerequisite prerequisite)
        {
            Assert.That(string.IsNullOrEmpty(downloadId), Is.False);
            Task.Run(() =>
            {
                prerequisite.WaitDownloaded();
                downloadedCallback(downloadId);
            });
        }

        [Test]
        public void When_Install_needed_without_download_installation_shall_begin_immediately_after_start()
        {
            TestPrerequisite[] TestPrerequisites = { new TestPrerequisite("Name1",false, false, null) };
            InstallWindowModel Model = new InstallWindowModel(TestPrerequisites, (downloadId, progressCallback, downloadedCallback) => this.DownloadFunc(downloadId, downloadedCallback, TestPrerequisites.First(_ => _.DownloadId == downloadId)));

            Assert.That(Model.Status, Is.TypeOf<InstallWindowModel.PreperationStatus>());
            Assert.That(((InstallWindowModel.PreperationStatus)Model.Status).IsAdminRequired, Is.False);
            Assert.That(((InstallWindowModel.PreperationStatus)Model.Status).MissingSoftware.Length, Is.EqualTo(1));
            Assert.That(((InstallWindowModel.PreperationStatus)Model.Status).MissingSoftware[0], Is.EqualTo("Name1"));

            ((InstallWindowModel.PreperationStatus)Model.Status).SetUpSystemCommand.Execute(null);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus>(Model);

            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Download, Is.Null);
            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Installation, Is.TypeOf<InstallWindowModel.InstallationStatus.InstallingStatus>());
            Assert.That(((InstallWindowModel.InstallationStatus.InstallingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Installation).NumberOfPackagesToInstall, Is.EqualTo(1));
            Assert.That(((InstallWindowModel.InstallationStatus.InstallingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Installation).InstalledPackages, Is.EqualTo(0));
            Assert.That(((InstallWindowModel.InstallationStatus.InstallingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Installation).InstallationStatus, Is.EqualTo("Name1"));

            TestPrerequisites[0].MarkInstalled(true);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationSuccessStatus>(Model);
        }

        [Test]
        public void When_download_is_needed_installation_shall_wait_but_first_downloaded_shall_be_installed_first()
        {
            TestPrerequisite[] TestPrerequisites = { new TestPrerequisite("Name1", false, false, "Down1"), new TestPrerequisite("Name2", false, false, "Down2") };

            InstallWindowModel Model = new InstallWindowModel(TestPrerequisites, (downloadId, progressCallback, downloadedCallback) => this.DownloadFunc(downloadId, downloadedCallback, TestPrerequisites.First(_ => _.DownloadId == downloadId)));
            Assert.That(Model.Status, Is.TypeOf<InstallWindowModel.PreperationStatus>());
            ((InstallWindowModel.PreperationStatus)Model.Status).SetUpSystemCommand.Execute(null);
            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus>(Model);

            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Installation, Is.TypeOf<InstallWindowModel.InstallationStatus.InstallationWaitingForDownloadStatus>());
            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Download, Is.TypeOf<InstallWindowModel.InstallationStatus.DownloadingStatus>());

            TestPrerequisites[1].MarkDownloaded(true);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus.InstallingStatus>(Model);

            Assert.That(((InstallWindowModel.InstallationStatus.InstallingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Installation).InstallationStatus, Is.EqualTo("Name2"));
            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Download, Is.TypeOf<InstallWindowModel.InstallationStatus.DownloadingStatus>());
            Assert.That(((InstallWindowModel.InstallationStatus.DownloadingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Download).AcquiredPackages, Is.EqualTo(1));

            TestPrerequisites[1].MarkInstalled(true);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus.InstallationWaitingForDownloadStatus>(Model);

            TestPrerequisites[0].MarkDownloaded(true);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus.InstallingStatus>(Model);

            Assert.That(((InstallWindowModel.InstallationStatus.InstallingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Installation).InstallationStatus, Is.EqualTo("Name1"));
            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Download, Is.TypeOf<InstallWindowModel.InstallationStatus.DownloadFinishedStatus>());

            TestPrerequisites[0].MarkInstalled(true);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationSuccessStatus>(Model);
        }

        [Test]
        public void When_download_is_needed_installation_shall_wait_but_if_download_is_finished_while_installation_happens_it_shall_continue_immediately()
        {
            TestPrerequisite[] TestPrerequisites = { new TestPrerequisite("Name1", false, false, "Down1"), new TestPrerequisite("Name2", false, false, "Down2") };

            InstallWindowModel Model = new InstallWindowModel(TestPrerequisites, (downloadId, progressCallback, downloadedCallback) => this.DownloadFunc(downloadId, downloadedCallback, TestPrerequisites.First(_ => _.DownloadId == downloadId)));
            Assert.That(Model.Status, Is.TypeOf<InstallWindowModel.PreperationStatus>());
            ((InstallWindowModel.PreperationStatus)Model.Status).SetUpSystemCommand.Execute(null);
            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus>(Model);
            ManualResetEvent StatusChangeEvent = new ManualResetEvent(false);
            Model.PropertyChanged += (s, e) =>
                                     {
                                         if (e.PropertyName == nameof(InstallWindowModel.Status))
                                         {
                                             StatusChangeEvent.Set();
                                         }
                                     };


            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Installation, Is.TypeOf<InstallWindowModel.InstallationStatus.InstallationWaitingForDownloadStatus>());
            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Download, Is.TypeOf<InstallWindowModel.InstallationStatus.DownloadingStatus>());

            TestPrerequisites[1].MarkDownloaded(true);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus.InstallingStatus>(Model);

            Assert.That(((InstallWindowModel.InstallationStatus.InstallingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Installation).InstallationStatus, Is.EqualTo("Name2"));
            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Download, Is.TypeOf<InstallWindowModel.InstallationStatus.DownloadingStatus>());

            TestPrerequisites[0].MarkDownloaded(true);
            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus.DownloadFinishedStatus>(Model);

            StatusChangeEvent.Reset();
            TestPrerequisites[1].MarkInstalled(true);
            StatusChangeEvent.WaitOne(TimeSpan.FromSeconds(10));

            Assert.That(((InstallWindowModel.InstallationStatus.InstallingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Installation).InstallationStatus, Is.EqualTo("Name1"));
            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Download, Is.TypeOf<InstallWindowModel.InstallationStatus.DownloadFinishedStatus>());

            TestPrerequisites[0].MarkInstalled(true);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationSuccessStatus>(Model);
        }

        [Test]
        public void When_administrative_rights_are_needded_those_prerequisites_shall_be_delegated_to_child_process_with_admin_rights()
        {
            TestPrerequisite[] TestPrerequisites = { new TestPrerequisite("Name1", true, false, null), new TestPrerequisite("Name2", false, false, null) };
            AdminProcess AdminProcess = new AdminProcess();
            InstallWindowModel Model = new InstallWindowModel(TestPrerequisites, (downloadId, progressCallback, downloadedCallback) => this.DownloadFunc(downloadId, downloadedCallback, TestPrerequisites.First(_ => _.DownloadId == downloadId)),AdminProcess);
            Assert.That(Model.Status, Is.TypeOf<InstallWindowModel.PreperationStatus>());
            Assert.That(((InstallWindowModel.PreperationStatus)Model.Status).IsAdminRequired, Is.True);
            ((InstallWindowModel.PreperationStatus)Model.Status).SetUpSystemCommand.Execute(null);
            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus>(Model);

            AdminProcess.WaitInstallStarted();
            AdminProcess.MarkInstalled(true);
            
            TestPrerequisites[1].MarkInstalled(true);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationSuccessStatus>(Model);
        }

        [Test]
        public void When_install_of_a_prerequisite_fails_the_state_shall_move_into_error_status()
        {
            TestPrerequisite[] TestPrerequisites = { new TestPrerequisite("Name1", false, false, null) };
            InstallWindowModel Model = new InstallWindowModel(TestPrerequisites, (downloadId, progressCallback, downloadedCallback) => this.DownloadFunc(downloadId, downloadedCallback, TestPrerequisites.First(_ => _.DownloadId == downloadId)));

            Assert.That(Model.Status, Is.TypeOf<InstallWindowModel.PreperationStatus>());
            ((InstallWindowModel.PreperationStatus)Model.Status).SetUpSystemCommand.Execute(null);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus>(Model);

            TestPrerequisites[0].MarkInstalled(false);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationErrorStatus>(Model);
            Assert.That(((InstallWindowModel.InstallationErrorStatus)Model.Status).Message, Is.EqualTo("FAIL!"));
        }

        [Test]
        public void When_install_of_a_prerequisite_fails_remote_the_state_shall_move_into_error_status()
        {
            TestPrerequisite[] TestPrerequisites = { new TestPrerequisite("Name1", true, false, null) };
            AdminProcess AdminProcess = new AdminProcess();
            InstallWindowModel Model = new InstallWindowModel(TestPrerequisites, (downloadId, progressCallback, downloadedCallback) => this.DownloadFunc(downloadId, downloadedCallback, TestPrerequisites.First(_ => _.DownloadId == downloadId)), AdminProcess);

            Assert.That(Model.Status, Is.TypeOf<InstallWindowModel.PreperationStatus>());
            ((InstallWindowModel.PreperationStatus)Model.Status).SetUpSystemCommand.Execute(null);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationStatus>(Model);

            AdminProcess.MarkInstalled(false);

            InstallWindowModelTest.WaitForStateChange<InstallWindowModel.InstallationErrorStatus>(Model);
            Assert.That(((InstallWindowModel.InstallationErrorStatus)Model.Status).Message, Is.EqualTo("FAIL!"));
        }
    }
}
