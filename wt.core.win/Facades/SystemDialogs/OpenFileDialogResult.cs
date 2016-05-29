using JetBrains.Annotations;

namespace WhileTrue.Facades.SystemDialogs
{
    /// <summary>
    /// Result of <see cref="ISystemDialogsProvider.ShowOpenFileDialog"/>
    /// </summary>
    [PublicAPI]
    public class OpenFileDialogResult
    {
        /// <summary/>
        public OpenFileDialogResult(string fileName)
        {
            this.FileName = fileName;
        }

        /// <summary>
        /// Selected filename
        /// </summary>
        public string FileName { get; }
    }
}