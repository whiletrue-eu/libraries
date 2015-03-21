using System.Collections.Generic;
using System.Windows.Input;

namespace WhileTrue.Classes.Commanding
{
    /// <summary>
    /// Factory to create RoutedCommand instances based on an ID string
    /// </summary>
    public static class RoutedCommandFactory
    {
        private static readonly Dictionary<string,RoutedCommand> routedCommands = new Dictionary<string, RoutedCommand>();

        /// <summary>
        /// Creates the instance. For the same ID string, the same instance will be returned
        /// </summary>
        public static RoutedCommand GetRoutedCommand(string commandID)
        {
            if(RoutedCommandFactory.routedCommands.ContainsKey(commandID))
            {
                return RoutedCommandFactory.routedCommands[commandID];
            }
            else
            {
                RoutedCommand Command = new RoutedCommand();
                RoutedCommandFactory.routedCommands.Add(commandID, Command);
                return Command;
            }
        }
    }
}