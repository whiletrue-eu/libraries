using System;
using System.Diagnostics;

namespace WhileTrue.Classes.Installer
{
    /// <summary>
    ///     Prerequisite installer that launches a process (e.g. setup.exe) to install
    /// </summary>
    [Serializable]
    public class ProcessLaunchPrerequisiteInstaller : PrerequisiteBase
    {
        /// <summary />
        public ProcessLaunchPrerequisiteInstaller(string name, bool requiresAdmin, string downloadId,
            Func<bool> alreadyInstalled, string processName, string arguments) : base(name, requiresAdmin,
            alreadyInstalled, downloadId)
        {
            ProcessName = processName;
            Arguments = arguments;
        }

        public string ProcessName { get; }
        public string Arguments { get; }

        /// <summary>
        ///     Performs installation of the rperequisite
        /// </summary>
        public override void DoInstall()
        {
            var InstallStartInfo = new ProcessStartInfo(ProcessName, Arguments)
            {
                WorkingDirectory = Environment.CurrentDirectory,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var Install = Process.Start(InstallStartInfo);
            Install.WaitForExit();
            if (Install.ExitCode != 0) throw new InstallationProcessException(Install.ExitCode);
        }
    }
}