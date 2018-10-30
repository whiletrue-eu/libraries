using System;

namespace WhileTrue.Classes.Installer
{
    /// <summary>
    ///     Thrown if process for an installation does not end with exit code '0'
    /// </summary>
    public class InstallationProcessException : Exception
    {
        /// <summary />
        public InstallationProcessException(int exitCode)
            : base($"Installer process exited with code '{exitCode}'")
        {
            ExitCode = exitCode;
        }

        /// <summary>
        ///     Exit code of the installation process
        /// </summary>
        public int ExitCode { get; }
    }
}