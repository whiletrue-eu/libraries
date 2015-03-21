using System.Collections.Generic;
using System.Windows.Input;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Components;
using WhileTrue.Facades.Commanding;

namespace WhileTrue.Components.Commanding
{
    /// <summary>
    /// Impelementation of the ICommand Manager interface.
    /// </summary>
    /// <remarks>
    /// Provides Command Warpping and action console execution capabilities
    /// </remarks>
    [ComponentDeclaration("Command Manager")]
    public class CommandManager : ICommandManager /*, IConsoleCommandProvider*/
    {
        private readonly Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();
        private readonly ICommandWrapper[] commandWrapper;

        /// <summary>
        /// Creates the service component without action wrappers
        /// </summary>
        public CommandManager()
        {
            this.commandWrapper = new ICommandWrapper[0];
        }

        /// <summary>
        /// Creates the service component with the given action warppers which are called 
        /// for any action that is registered
        /// </summary>
        public CommandManager(ICommandWrapper[] commandWrapper)
        {
            this.commandWrapper = commandWrapper;
        }

        /// <summary>
        /// Creates the service component with the given action warppers which are called 
        /// for any action that is registered.
        /// Accepts a list of commands that are initially registered
        /// </summary>
        public CommandManager(ICommandWrapper[] commandWrapper, IDictionary<string,ICommand> commands)
        {
            this.commandWrapper = commandWrapper;
            foreach (KeyValuePair<string, ICommand> Command in commands)
            {
                this.RegisterCommand(Command.Key, Command.Value);
            }
        }

        #region ICommandManager Members

        /// <summary>
        /// Registeres the given command and wraps it with all given command wrappers
        /// </summary>
        public ICommand RegisterCommand(string id, ICommand command)
        {
            ICommand WrappedCommand = command;
            foreach (ICommandWrapper Wrapper in this.commandWrapper)
            {
                WrappedCommand = Wrapper.Wrap(WrappedCommand);
            }

            this.commands.Add(id, WrappedCommand);

            return WrappedCommand;
        }

        public ICommand RegisterCommand(ICommandIdentification command)
        {
            return this.RegisterCommand(command.ID, command);
        }

        public ICommand this[string commandID]
        {
            get { return this.commands[commandID]; }
        }

        #endregion

        /*
        #region IConsoleCommandProvider implementation

        public string CommandPrefix
	    {
            get 
            {
                return "execute";
            }
	    }

        public string CommandShortPrefix
	    {
	        get
	        {
	            return null;
	        }
	    }

	    public string Usage
	    {
            get
            {
                return string.Format("Usage: '{0} [action-ID]' or '{0} list'", CommandPrefix);
            }
	    }

        public string Execute(string command)
        {
            if (command == "list" )
            {
                List<string> Actions = new List<string>();
                foreach (string ActionID in this.commands.Keys)
                {
                    Actions.Add(ActionID);
                }

                return string.Format("Availiable commands:\n{0}", string.Join("\n", Actions.ToArray()));
            }
            else if (this.commands.ContainsKey(command))
            {
                if (this.commands[command].CanExecute)
                {
                    this.commands[command].Execute();
                    return string.Empty;
                }
                else
                {
                    return string.Format("Command '{0}' cannot be executed at the moment.", command);
                }
            }
            else
            {
                return string.Format("Command not found: '{0}'", command);
            }
        }


        public bool CanCastTo<ActionType>(object action) where ActionType : ICommand
        {
            if( action is ActionType )
            {
                return true;
            }
            else if( action is IActionWrap )
            {
                return ((IActionWrap)action).CanCastTo<ActionType>();
            }
            else
            {
                return false;
            }
        }

        public ActionType CastTo<ActionType>(object action) where ActionType : ICommand
        {
            if( action is ActionType )
            {
                return (ActionType)action;
            }
            else if( action is IActionWrap )
            {
                return ((IActionWrap)action).CastTo<ActionType>();
            }
            else
            {
                throw new InvalidCastException();
            }      
        }
        #endregion
        */
    }
}