using System.Windows;
using JetBrains.Annotations;
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.SystemDialogs
{
    /// <summary>
    /// Provides system dialogs to models
    /// </summary>
    [ComponentInterface]
    [PublicAPI]
    public interface ISystemDialogsProvider
    {
        /// <summary>
        /// Shows an Open File Dialog and returns the result, or <c>null</c> if the user cancelled the dialog 
        /// </summary>
        OpenFileDialogResult ShowOpenFileDialog(string title=null, string filter="All Files|*.*");
        /// <summary>
        /// Shows an Save File Dialog and returns the result, or <c>null</c> if the user cancelled the dialog 
        /// </summary>
        SaveFileDialogResult ShowSaveFileDialog(string title = null, string filter = "All Files|*.*", bool? addExtension=null);
        /// <summary>
        /// Opens an explorer window that shows the directory and selected file of the given path
        /// </summary>
        void OpenPathInExplorer(string path);
        /// <summary>
        /// Shows a message box
        /// </summary>
        void ShowMessage(string message, string title = null, MessageBoxImage icon = MessageBoxImage.Information);
    }
}
