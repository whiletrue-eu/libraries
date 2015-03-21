using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace WhileTrue.Classes.Wpf
{
    internal class CommandWrapper : ICommand
    {
        private static readonly Dictionary<Dispatcher, Dictionary<ICommand, CommandWrapper>> commandWrappers = new Dictionary<Dispatcher, Dictionary<ICommand, CommandWrapper>>();
        private static readonly EventHandler requerySuggestedHandler;

        static CommandWrapper()
        {
            requerySuggestedHandler = RequerySuggested;
            CommandManager.RequerySuggested += requerySuggestedHandler;
        }

        private static void RequerySuggested(object sender, EventArgs eventArgs)
        {
            int CurrentThreadId = Thread.CurrentThread.ManagedThreadId;
            if (commandWrappers.Keys.Any(dispatcher => dispatcher.Thread.ManagedThreadId == CurrentThreadId) == false)
            {
                //call is not coming from any UI thread -> forward to all the UI threads
                foreach (Dispatcher Dispatcher in commandWrappers.Keys.ToArray())
                {

                    if( Dispatcher.HasShutdownFinished )
                    {
                        //Clean up if Dispatcher was shut down. can be done because collection is freezed in foreach with call to ToArray()
                        commandWrappers.Remove(Dispatcher);
                    }
                    else
                    {
                        Dispatcher.BeginInvoke((Action)CommandManager.InvalidateRequerySuggested);
                    }
                }
            }
        }

        public static CommandWrapper GetCommandWrapperInstance(ICommand command)
        {
            lock (commandWrappers)
            {
                Dispatcher CurrentDispatcher = Dispatcher.CurrentDispatcher;
                if (commandWrappers.ContainsKey(CurrentDispatcher))
                {
                    return GetCommandWrapperInstance(commandWrappers[CurrentDispatcher], command,
                                                        CurrentDispatcher);
                }
                else
                {
                    Dictionary<ICommand, CommandWrapper> CollectionWrappers =
                        new Dictionary<ICommand, CommandWrapper>();
                    commandWrappers.Add(CurrentDispatcher, CollectionWrappers);
                    return GetCommandWrapperInstance(CollectionWrappers, command, CurrentDispatcher);
                }
            }
        }

        private static CommandWrapper GetCommandWrapperInstance(IDictionary<ICommand, CommandWrapper> commandWrappers, ICommand command, Dispatcher dispatcher)
        {
            lock (commandWrappers)
            {
                if (commandWrappers.ContainsKey(command))
                {
                    return commandWrappers[command];
                }
                else
                {
                    CommandWrapper Wrapper = new CommandWrapper(command, dispatcher);
                    commandWrappers.Add(command, Wrapper);
                    return Wrapper;
                }
            }
        }


        private readonly Dispatcher dispatcher;
        private readonly ICommand originalCommand;

        private CommandWrapper(ICommand command, Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.originalCommand = command;
            this.originalCommand.CanExecuteChanged += this.OriginalCommandCanExecuteChanged;
        }

        void OriginalCommandCanExecuteChanged(object sender, EventArgs e)
        {
            this.dispatcher.BeginInvoke((Action) delegate { this.CanExecuteChanged(this, e); });
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