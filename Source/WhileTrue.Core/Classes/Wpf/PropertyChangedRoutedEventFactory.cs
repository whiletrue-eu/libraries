using System;
using System.Collections.Generic;
using System.Windows;

namespace WhileTrue.Classes.Commanding
{
    /// <summary>
    /// Factory for RoutedEvent instances based on ID strings
    /// </summary>
    public static class PropertyChangedRoutedEventFactory
    {
        private static readonly Dictionary<string, RoutedEvent> routedCommands = new Dictionary<string, RoutedEvent>();

        /// <summary>
        /// Creates a RoutedCommand from the ID string. The same string will return the same instance
        /// </summary>
        public static RoutedEvent GetRoutedEvent(string commandID)
        {
            if (PropertyChangedRoutedEventFactory.routedCommands.ContainsKey(commandID))
            {
                return PropertyChangedRoutedEventFactory.routedCommands[commandID];
            }
            else
            {
                RoutedEvent Command = EventManager.RegisterRoutedEvent(commandID, RoutingStrategy.Bubble, typeof(EventHandler), typeof(PropertyChangedRoutedEventFactory));
                PropertyChangedRoutedEventFactory.routedCommands.Add(commandID, Command);
                return Command;
            }
        }
    }
}