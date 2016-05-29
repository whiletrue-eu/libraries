using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace WhileTrue.Classes.Forms
{
    internal class CommandWrapper : ICommand
    {
        private static readonly Dictionary<ICommand, CommandWrapper> commandWrappers = new Dictionary<ICommand, CommandWrapper>();

        public static CommandWrapper GetCommandWrapperInstance(ICommand command)
        {
            lock (CommandWrapper.commandWrappers)
            {
                    return CommandWrapper.GetCommandWrapperInstance(CommandWrapper.commandWrappers, command);
            }
        }

        private static CommandWrapper GetCommandWrapperInstance(IDictionary<ICommand, CommandWrapper> commandWrappers, ICommand command)
        {
            lock (commandWrappers)
            {
                if (commandWrappers.ContainsKey(command))
                {
                    return commandWrappers[command];
                }
                else
                {
                    CommandWrapper Wrapper = new CommandWrapper(command);
                    commandWrappers.Add(command, Wrapper);
                    return Wrapper;
                }
            }
        }


        private readonly ICommand originalCommand;

        private CommandWrapper(ICommand command)
        {
            this.originalCommand = command;
            this.originalCommand.CanExecuteChanged += this.OriginalCommandCanExecuteChanged;
        }

        void OriginalCommandCanExecuteChanged(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(()=>this.CanExecuteChanged(this, e));
        }


        public void Execute(object parameter)
        {
            this.originalCommand.Execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return this.originalCommand.CanExecute(parameter);
        }

        public event EventHandler CanExecuteChanged=delegate {};
    }
}