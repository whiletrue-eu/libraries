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
    [Component]
    public class SystemDialogsProvider : ISystemDialogsProvider
    {
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
            ProcessInfo.Arguments = string.Format(@"/select, ""{0}""", path);
            ProcessInfo.WindowStyle = ProcessWindowStyle.Normal;
            Process.Start(ProcessInfo);
        }

        public void ShowMessage(string message, string title = null, MessageBoxImage icon = MessageBoxImage.Information)
        {
            MessageBox.Show(message,title, MessageBoxButton.OK, icon);
        }
    }
}
