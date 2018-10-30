using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace WhileTrue.Classes.Forms
{
    internal class CommandWrapper : ICommand
    {
        private static readonly Dictionary<ICommand, CommandWrapper> commandWrappers =
            new Dictionary<ICommand, CommandWrapper>();


        private readonly ICommand originalCommand;

        private CommandWrapper(ICommand command)
        {
            originalCommand = command;
            originalCommand.CanExecuteChanged += OriginalCommandCanExecuteChanged;
        }


        public void Execute(object parameter)
        {
            originalCommand.Execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return originalCommand.CanExecute(parameter);
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public static CommandWrapper GetCommandWrapperInstance(ICommand command)
        {
            lock (commandWrappers)
            {
                return GetCommandWrapperInstance(commandWrappers, command);
            }
        }

        private static CommandWrapper GetCommandWrapperInstance(IDictionary<ICommand, CommandWrapper> commandWrappers,
            ICommand command)
        {
            lock (commandWrappers)
            {
                if (commandWrappers.ContainsKey(command)) return commandWrappers[command];

                var Wrapper = new CommandWrapper(command);
                commandWrappers.Add(command, Wrapper);
                return Wrapper;
            }
        }

        private void OriginalCommandCanExecuteChanged(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => CanExecuteChanged(this, e));
        }
    }
}