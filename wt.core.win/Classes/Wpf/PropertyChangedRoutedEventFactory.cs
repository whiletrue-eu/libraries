using System;
using System.Collections.Generic;
using System.Windows;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Factory for RoutedEvent instances based on ID strings
    /// </summary>
    internal static class PropertyChangedRoutedEventFactory
    {
        private static readonly Dictionary<string, RoutedEvent> routedCommands = new Dictionary<string, RoutedEvent>();

        /// <summary>
        /// Creates a RoutedCommand from the ID string. The same string will return the same instance
        /// </summary>
        public static RoutedEvent GetRoutedEvent(string commandId)
        {
            if (PropertyChangedRoutedEventFactory.routedCommands.ContainsKey(commandId))
            {
                return PropertyChangedRoutedEventFactory.routedCommands[commandId];
            }
            else
            {
                RoutedEvent Command = EventManager.RegisterRoutedEvent(commandId, RoutingStrategy.Bubble, typeof(EventHandler), typeof(PropertyChangedRoutedEventFactory));
                PropertyChangedRoutedEventFactory.routedCommands.Add(commandId, Command);
                return Command;
            }
        }
    }
}