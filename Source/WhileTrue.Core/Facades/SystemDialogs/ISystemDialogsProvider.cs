using System.Windows;
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.SystemDialogs
{
    [ComponentInterface]
    public interface ISystemDialogsProvider
    {
        OpenFileDialogResult ShowOpenFileDialog(string title=null, string filter="All Files|*.*");
        SaveFileDialogResult ShowSaveFileDialog(string title = null, string filter = "All Files|*.*", bool? addExtension=null);
        void OpenPathInExplorer(string path);
        void ShowMessage(string message, string title = null, MessageBoxImage icon = MessageBoxImage.Information);
    }
}
