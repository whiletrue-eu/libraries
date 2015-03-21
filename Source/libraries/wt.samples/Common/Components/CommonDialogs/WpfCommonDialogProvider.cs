using System;
using System.Linq;
using System.Windows;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Wpf;
using WhileTrue.Common.Facades.CommonDialogs;

namespace WhileTrue.Common.Components.CommonDialogs
{
    [Component]
    public class WpfCommonDialogProvider : ICommonDialogProvider
    {
        public void ShowError(Exception exception)
        {
            if (exception is AggregateException)
            {
                AggregateException Exception = (AggregateException) exception;
                string[] Messages = (from InnerException in Exception.InnerExceptions select InnerException.Message).ToArray();

                MessageBox.Show(string.Join("\n\n---------------------------------\n", Messages));
            }
            else
            {
                WpfCommonDialogProvider.ShowError(exception.Message);
            }
        }

        private static void ShowError(string message)
        {
            Window ActiveWindow = WpfUtils.FindActiveWindow();

            if (ActiveWindow != null)
            {
                ActiveWindow.Invoke(window => MessageBox.Show(window, message, "Error"));
            }
            else
            {
                MessageBox.Show(message, "Error");
            }
        }
    }
}
