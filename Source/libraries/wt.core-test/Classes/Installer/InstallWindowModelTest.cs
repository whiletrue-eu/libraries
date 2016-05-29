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

            public override bool DoInstall()
            {
                this.installStartedEvent.Set();
                this.installCompleteEvent.WaitOne();
                this.DoInstallCalled = true;
                return this.installSuccess;
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

        [Test]
        public void When_Install_needed_without_download_installation_shall_begin_immediately_after_start()
        {
            TestPrerequisite[] TestPrerequisites = { new TestPrerequisite("Name1",false, false, null) };
            InstallWindowModel Model = new InstallWindowModel(TestPrerequisites, (downloadId, action) => this.DownloadFunc(downloadId, action, TestPrerequisites.First(_ => _.DownloadId == downloadId)));

            Assert.That(Model.Status, Is.TypeOf<InstallWindowModel.PreperationStatus>());
            Assert.That(((InstallWindowModel.PreperationStatus)Model.Status).IsAdminRequired, Is.False);
            Assert.That(((InstallWindowModel.PreperationStatus)Model.Status).MissingSoftware.Length, Is.EqualTo(1));
            Assert.That(((InstallWindowModel.PreperationStatus)Model.Status).MissingSoftware[0], Is.EqualTo("Name1"));

            ((InstallWindowModel.PreperationStatus)Model.Status).SetUpSystemCommand.Execute(null);

            WaitForStateChange(Model);

            Assert.That(Model.Status, Is.TypeOf<InstallWindowModel.InstallationStatus>());
            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Download, Is.Null);
            Assert.That(((InstallWindowModel.InstallationStatus)Model.Status).Installation, Is.TypeOf<InstallWindowModel.InstallationStatus.InstallingStatus>());
            Assert.That(((InstallWindowModel.InstallationStatus.InstallingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Installation).NumberOfPackagesToInstall, Is.EqualTo(1));
            Assert.That(((InstallWindowModel.InstallationStatus.InstallingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Installation).InstalledPackages, Is.EqualTo(0));
            Assert.That(((InstallWindowModel.InstallationStatus.InstallingStatus)((InstallWindowModel.InstallationStatus)Model.Status).Installation).InstallationStatus, Is.EqualTo("Name1"));

            TestPrerequisites[0].MarkInstalled(true);

            Assert.That(Model.Status, Is.TypeOf<InstallWindowModel.InstallationSuccessStatus>());
        }

        private void WaitForStateChange(InstallWindowModel model, Type type)
        {
            TimeSpan Timeout= Debugger.IsAttached ? TimeSpan.FromDays(1) : TimeSpan.FromSeconds(10);

            while (model.Status.GetType() != type && Timeout. > 0)
            {
                Thread.Sleep(100);
                Timeout = Timeout - TimeSpan.FromMilliseconds(100);
            }
        }


        [Test]
        public void Software_already_installed_Shall_not_be_taken_into_account()
        {
            TestPrerequisite[] TestPrerequisites = { new TestPrerequisite("Name1",false, true, "Down1"), new TestPrerequisite("Name2",false, false, "Down2") };
            InstallWindowModel Model = new InstallWindowModel(TestPrerequisites, (downloadId, action) => this.DownloadFunc(downloadId, action, TestPrerequisites.First(_ => _.DownloadId == downloadId)));

            Assert.That(Model.Status, Is.TypeOf<InstallWindowModel.PreperationStatus>());
            Assert.That(((InstallWindowModel.PreperationStatus)Model.Status).IsAdminRequired, Is.False);
            Assert.That(((InstallWindowModel.PreperationStatus)Model.Status).MissingSoftware.Length, Is.EqualTo(1));
            Assert.That(((InstallWindowModel.PreperationStatus)Model.Status).MissingSoftware[0], Is.EqualTo("Name2"));
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
    }
}
