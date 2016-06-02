using System;
using System.Diagnostics;

namespace WhileTrue.Classes.Installer
{
    /// <summary>
    /// Prerequisite installer that launches a process (e.g. setup.exe) to install
    /// </summary>
    [Serializable]
    public class ProcessLaunchPrerequisiteInstaller : PrerequisiteBase
    {
        private readonly string processName;
        private readonly string arguments;

        /// <summary/>
        public ProcessLaunchPrerequisiteInstaller(string name, bool requiresAdmin, string downloadId, Func<bool> alreadyInstalled, string processName, string arguments) : base(name, requiresAdmin, alreadyInstalled, downloadId)
        {
            this.processName = processName;
            this.arguments = arguments;
        }

        /// <summary>
        /// Performs installation of the rperequisite
        /// </summary>
        public override bool DoInstall()
        {
            ProcessStartInfo InstallStartInfo = new ProcessStartInfo(this.processName, this.arguments)
            {
                WorkingDirectory = Environment.CurrentDirectory,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process Install = Process.Start(InstallStartInfo);
            Install.WaitForExit();
            return true;
        }
    }
}