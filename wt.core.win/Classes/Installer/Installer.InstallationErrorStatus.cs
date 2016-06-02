namespace WhileTrue.Classes.Installer
{
    public partial class InstallWindowModel
    {
        /// <summary>
        /// Status if the installation of a prerequisite failed
        /// </summary>
        public class InstallationErrorStatus : StatusBase
        {
            /// <summary/>
            public InstallationErrorStatus(string message)
            {
                this.Message = message;
            }

            /// <summary>
            /// Information about the failure
            /// </summary>
            public string Message { get; }
        }
    }
}