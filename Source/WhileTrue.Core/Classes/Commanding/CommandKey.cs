// ReSharper disable MemberCanBePrivate.Global
using System.Windows.Input;
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Commanding
{
    /// <summary>
    /// used for command binding helper classes. Can wrap RoutedCommand or strings (which are converted using <see cref="RoutedCommandFactory"/>
    /// </summary>
    [NoCoverage]
    public class CommandKey
    {
        private readonly RoutedCommand command;

        /// <summary/>
        public CommandKey(string commandID)
        {
            this.command = RoutedCommandFactory.GetRoutedCommand(commandID);
        }

        /// <summary/>
        public CommandKey(RoutedCommand command)
        {
            this.command = command;
        }

        /// <summary/>
        public RoutedCommand RoutedCommand
        {
            get
            {
                return this.command;
            }
        }

        /// <summary/>
        public static implicit operator CommandKey(string commandID)
        {
            return new CommandKey(commandID);
        }

        /// <summary/>
        public static implicit operator CommandKey(RoutedCommand command)
        {
            return new CommandKey(command);
        }
    }
}