using System;
using System.Windows;
using System.Windows.Input;
using NUnit.Framework;

namespace WhileTrue.Controls._Unittest_Debug_
{
    partial class ProgressTestWindow : ICommand
    {
        public ProgressTestWindow()
        {
            this.InitializeComponent();
        }

        private void StartProgress(object sender, RoutedEventArgs e)
        {
            this.Progress.Progress = new Progress() { Status = "Hello World", CurrentProgress = 0.5, CancelCommand = this};
        }

        public void Execute(object parameter)
        {
            this.Progress.Progress = null;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

    [TestFixture]
    public class ProgressTest
    {
        [Test, Ignore("Manual")]
        public void Launch()
        {
            ProgressTestWindow ProgressTestWindow = new ProgressTestWindow();
            ProgressTestWindow.ShowDialog();
        }
    }
}