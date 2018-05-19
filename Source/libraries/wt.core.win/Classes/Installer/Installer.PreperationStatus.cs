using System.Windows.Input;

namespace WhileTrue.Classes.Installer
{
    public partial class InstallWindowModel
    {
        /// <summary>
        /// State before installation that informs the user that prerequisite setup is needed
        /// </summary>
        public class PreperationStatus : StatusBase
        {
            /// <summary/>
            public PreperationStatus(string[] missingSoftware, bool isAdminRequired, ICommand setUpSystemCommand)
            {
                this.MissingSoftware = missingSoftware;
                this.IsAdminRequired = isAdminRequired;
                this.SetUpSystemCommand = setUpSystemCommand;
            }

            /// <summary>
            /// List of prerequisites that need to be installed
            /// </summary>
            public string[] MissingSoftware { get; }
            /// <summary>
            /// true if administrative rights are required for (part of) the installation
            /// </summary>
            public bool IsAdminRequired { get; }
            /// <summary>
            /// Runs the prerequisite installation
            /// </summary>
            public ICommand SetUpSystemCommand { get; }
        }
    }
}