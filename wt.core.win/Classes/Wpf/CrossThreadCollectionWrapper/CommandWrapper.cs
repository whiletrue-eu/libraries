using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Threading;

namespace WhileTrue.Classes.Wpf
{
    internal class CommandWrapper : ICommand
    {
        private static readonly Dictionary<Dispatcher, Dictionary<ICommand, CommandWrapper>> commandWrappers =
            new Dictionary<Dispatcher, Dictionary<ICommand, CommandWrapper>>();


        private readonly Dispatcher dispatcher;
        private readonly ICommand originalCommand;

        private CommandWrapper(ICommand command, Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
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
                var CurrentDispatcher = Dispatcher.CurrentDispatcher;
                if (commandWrappers.ContainsKey(CurrentDispatcher))
                    return GetCommandWrapperInstance(commandWrappers[CurrentDispatcher], command,
                        CurrentDispatcher);

                var CollectionWrappers =
                    new Dictionary<ICommand, CommandWrapper>();
                commandWrappers.Add(CurrentDispatcher, CollectionWrappers);
                return GetCommandWrapperInstance(CollectionWrappers, command, CurrentDispatcher);
            }
        }

        private static CommandWrapper GetCommandWrapperInstance(IDictionary<ICommand, CommandWrapper> commandWrappers,
            ICommand command, Dispatcher dispatcher)
        {
            lock (commandWrappers)
            {
                if (commandWrappers.ContainsKey(command)) return commandWrappers[command];

                var Wrapper = new CommandWrapper(command, dispatcher);
                commandWrappers.Add(command, Wrapper);
                return Wrapper;
            }
        }

        private void OriginalCommandCanExecuteChanged(object sender, EventArgs e)
        {
            dispatcher.BeginInvoke((Action) delegate { CanExecuteChanged(this, e); });
        }
    }
}