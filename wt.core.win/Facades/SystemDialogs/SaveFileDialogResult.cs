using JetBrains.Annotations;

namespace WhileTrue.Facades.SystemDialogs
{
    /// <summary>
    /// Result of <see cref="ISystemDialogsProvider.ShowSaveFileDialog"/>
    /// </summary>
    [PublicAPI]
    public class SaveFileDialogResult
    {
        /// <summary/>
        public SaveFileDialogResult(string fileName)
        {
            this.FileName = fileName;
        }

        /// <summary>
        /// filename To save
        /// </summary>
        public string FileName { get; }
    }
}