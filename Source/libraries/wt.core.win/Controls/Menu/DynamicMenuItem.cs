using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.Commanding;

namespace WhileTrue.Controls.Menu
{
    public class DynamicMenuItem : MenuItem, System.Windows.Input.ICommand
    {
        private ICommand command;
        public string CommandID { get; set; }

        private bool IsApplicable
        {
            get
            {
                if (this.command == null)
                {
                    return false;
                }
                if (this.command is ICommandAvailabilityInformation)
                {
                    return ((ICommandAvailabilityInformation)this.command).IsAvailable;
                }
                else
                {
                    //'normal' ICommand
                    return true;
                }
            }
        }

        #region ICommand Members

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            this.command.Execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return this.command.CanExecute(parameter);
        }

        #endregion

        public override void EndInit()
        {
            base.EndInit();

            ICommandManager CommandManager = (ICommandManager)this.FindResource(typeof(ICommandManager));
            this.command = CommandManager[this.CommandID];
            this.command.CanExecuteChanged += this.Action_CanExecuteChanged;
            if (this.command is ICommandAvailabilityInformation)
            {
                ((ICommandAvailabilityInformation)this.command).IsAvailableChanged += this.Action_IsAvailableChanged;
            }

            this.Command = this;

            this.UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            this.Visibility = this.IsApplicable ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Action_IsAvailableChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (VoidDelegate)delegate { this.UpdateVisibility(); });
        }

        private void Action_CanExecuteChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (VoidDelegate)delegate { this.InvokeCanExecuteChanged(); });
        }

        private void InvokeCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
            {
                this.CanExecuteChanged(this, new EventArgs());
            }
        }
    }
}