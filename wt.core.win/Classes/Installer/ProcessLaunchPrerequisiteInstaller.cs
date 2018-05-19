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
        public string ProcessName { get; }
        public string Arguments { get; }

        /// <summary/>
        public ProcessLaunchPrerequisiteInstaller(string name, bool requiresAdmin, string downloadId, Func<bool> alreadyInstalled, string processName, string arguments) : base(name, requiresAdmin, alreadyInstalled, downloadId)
        {
            this.ProcessName = processName;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Performs installation of the rperequisite
        /// </summary>
        public override void DoInstall()
        {
            ProcessStartInfo InstallStartInfo = new ProcessStartInfo(this.ProcessName, this.Arguments)
            {
                WorkingDirectory = Environment.CurrentDirectory,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process Install = Process.Start(InstallStartInfo);
            Install.WaitForExit();
            if (Install.ExitCode != 0)
            {
                throw new InstallationProcessException(Install.ExitCode);
            }
        }
    }
}