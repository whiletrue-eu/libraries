using System;
using System.Windows;
using System.Windows.Input;
using NUnit.Framework;

namespace WhileTrue.Controls
{
    partial class ProgressTestWindow : ICommand
    {
        public ProgressTestWindow()
        {
            InitializeComponent();
        }

        public void Execute(object parameter)
        {
            Progress.Progress = null;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        private void StartProgress(object sender, RoutedEventArgs e)
        {
            Progress.Progress = new Progress {Status = "Hello World", CurrentProgress = 0.5, CancelCommand = this};
        }
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
    }

    [TestFixture]
    public class ProgressTest
    {
        [Test]
        [Ignore("Manual")]
        public void Launch()
        {
            var ProgressTestWindow = new ProgressTestWindow();
            ProgressTestWindow.ShowDialog();
        }
    }
}