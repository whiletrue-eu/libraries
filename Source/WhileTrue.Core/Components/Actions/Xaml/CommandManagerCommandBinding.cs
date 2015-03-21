using System;
using System.Windows.Input;
using Mz.Facades.Actions;

namespace Mz.Components.Actions.Xaml
{
    public abstract class CommandManagerCommandBinding
    {
        private static ICommandManager commandManager;

        private readonly string actionID;
        private ICommand command;

        protected CommandManagerCommandBinding(string actionID)
        {
            this.actionID = actionID;
        }

        public static ICommandManager CommandManager
        {
            set
            {
                if (commandManager != null)
                {
                    throw new InvalidOperationException("Command Manager can only be set once!");
                }


                commandManager = value;
            }
            get { return commandManager; }
        }

        public ICommand Command
        {
            get
            {
                if (this.command == null)
                {
                    this.command = this.GetAction();
                }
                return this.command;
            }
        }


        public string ActionID
        {
            get { return this.actionID; }
        }

        protected abstract void BindToAction(ICommand command);

        private ICommand GetAction()
        {
            if (commandManager == null)
            {
                throw new InvalidOperationException("Command Manager is not set. Use static property 'commandManager' on class 'CommandManagerCommandBinding' to set the reference.");
            }

            ICommand Command = commandManager[this.actionID];
            this.BindToAction(Command);
            return Command;
        }
    }
}