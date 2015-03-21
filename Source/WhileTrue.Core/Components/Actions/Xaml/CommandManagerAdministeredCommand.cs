using System;
using System.Windows.Input;

namespace Mz.Components.Actions.Xaml
{
    public abstract class CommandManagerAdministeredCommand : CommandManagerCommandBinding, ICommand
    {
        protected CommandManagerAdministeredCommand(string commandID)
            : base(commandID)
        {
        }

        #region ICommand Members

        public virtual bool CanExecute(object parameter)
        {
            return this.Command.CanExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public virtual void Execute(object parameter)
        {
            this.Command.Execute(parameter);
        }

        #endregion

        protected override void BindToAction(ICommand command)
        {
            command.CanExecuteChanged += this.action_CanExecuteChanged;
        }

        protected void InvokeCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
            {
                this.CanExecuteChanged(this, new EventArgs());
            }
        }

        private void action_CanExecuteChanged(object sender, EventArgs e)
        {
            this.InvokeCanExecuteChanged();
        }
    }
}