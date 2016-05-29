using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.SystemDialogs;

namespace WhileTrue.Components.SystemDialogs
{
    /// <summary>
    /// This component provides system dialogs to a componentized application, to decouple simple user interaction (message boxed, file open/save dialogs etc.) form
    /// the application logic.
    /// </summary>
    [Component]
    public class SystemDialogsProvider : ISystemDialogsProvider
    {
        /// <summary>
        /// Shows an Open File Dialog and returns the result, or <c>null</c> if the user cancelled the dialog 
        /// </summary>
        public OpenFileDialogResult ShowOpenFileDialog(string title, string filter)
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            if( title !=null )
            {
                Dialog.Title = title;
            }
            Dialog.Filter = filter;
            if( Dialog.ShowDialog() == true )
            {
                return new OpenFileDialogResult(Dialog.FileName);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Shows an Save File Dialog and returns the result, or <c>null</c> if the user cancelled the dialog 
        /// </summary>
        public SaveFileDialogResult ShowSaveFileDialog(string title, string filter, bool? addExtension)
        {
            SaveFileDialog Dialog = new SaveFileDialog();
            if (title != null)
            {
                Dialog.Title = title;
            }
            Dialog.Filter = filter;
            if( addExtension != null)
            {
                Dialog.AddExtension = (bool) addExtension;
            }
            if (Dialog.ShowDialog() == true)
            {
                return new SaveFileDialogResult(Dialog.FileName);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Opens an explorer window that shows the directory and selected file of the given path
        /// </summary>
        public void OpenPathInExplorer(string path)
        {
            DbC.AssureArgumentNotNull(path,"path");

            string ExplorerExe = Path.Combine(Environment.GetEnvironmentVariable("windir")??"", "Explorer.exe");

            while(File.Exists(path) == false && Directory.Exists(path) == false )
            {
                path = Path.GetDirectoryName(path);
            }

            if( Directory.Exists(path) )
            {
                //path is a directory
                path += "\\";
            }
            ProcessStartInfo ProcessInfo = new ProcessStartInfo(ExplorerExe);
            ProcessInfo.Arguments = $@"/select, ""{path}""";
            ProcessInfo.WindowStyle = ProcessWindowStyle.Normal;
            Process.Start(ProcessInfo);
        }

        /// <summary>
        /// Shows a message box
        /// </summary>
        public void ShowMessage(string message, string title = null, MessageBoxImage icon = MessageBoxImage.Information)
        {
            MessageBox.Show(message,title, MessageBoxButton.OK, icon);
        }
    }
}
