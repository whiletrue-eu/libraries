using System;

namespace WhileTrue.Classes.Installer
{
    /// <summary>
    ///     Base class for prerequisige installation handlers
    /// </summary>
    [Serializable]
    public abstract class PrerequisiteBase
    {
        [NonSerialized] private readonly Func<bool> alreadyInstalled;

        [NonSerialized] private bool? isAlreadyInstalled;

        /// <summary />
        public PrerequisiteBase(string name, bool requiresAdmin, Func<bool> alreadyInstalled, string downloadId)
        {
            this.alreadyInstalled = alreadyInstalled;
            DownloadId = downloadId;
            Name = name;
            RequiresAdmin = requiresAdmin;
        }

        internal double DownloadProgress { get; set; }

        /// <summary>
        ///     Name of the prerequisite
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Indicates whether administrative rights are needed to install this prerequisite.
        /// </summary>
        /// <remarks>
        ///     If admin rights are needed, installation will be done within a child process with admin rights. For this, the
        ///     prerequisite class must be serializable
        /// </remarks>
        public bool RequiresAdmin { get; }

        /// <summary>
        ///     Indicates whether the prerequisite is already present on the system
        /// </summary>
        public bool IsAlreadyInstalled => (isAlreadyInstalled ?? (isAlreadyInstalled = GetIsAlreadyInstalled())).Value;

        /// <summary>
        ///     Indicates that the prerequisite was installed
        /// </summary>
        internal bool WasInstalled { get; private set; }

        /// <summary>
        ///     Download id used to download the prerequisite
        /// </summary>
        public string DownloadId { get; }

        private bool GetIsAlreadyInstalled()
        {
            try
            {
                return alreadyInstalled();
            }
            catch
            {
                return false;
            }
        }

        internal void SetInstalled()
        {
            WasInstalled = true;
        }

        /// <summary>
        ///     Performs installation of the rperequisite
        /// </summary>
        public abstract void DoInstall();
    }
}