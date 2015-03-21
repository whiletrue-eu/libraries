using System.Collections.Generic;
using System.Windows.Input;

namespace WhileTrue.Classes.Commanding
{
    ///<summary>
    /// extension methods to register ICOmmand implementations to WPF command binding collections
    ///</summary>
    public static class CommandBindingHelper
    {
        /// <summary>
        /// Registers the dictionary of commands at the command binding collection.
        /// </summary>
        /// <remarks>
        /// The key is taken as RoutedCommand identifier while the value is the ICommand implementation</remarks>
        public static void Register(this CommandBindingCollection commandBindings, IEnumerable<KeyValuePair<CommandKey, ICommand>> commands)
        {
            foreach (KeyValuePair<CommandKey, ICommand> Command in commands)
            {
                commandBindings.Add(new Binding(Command.Key.RoutedCommand, Command.Value));
            }
        }

        /// <summary>
        /// unregisters the dictionary of commands at the command binding collection.
        /// </summary>
        /// <remarks>
        /// All teh commands with the given keys are removed. The value is ignored.</remarks>
        public static void Unregister(this CommandBindingCollection commandBindings, IEnumerable<KeyValuePair<CommandKey, ICommand>> commands)
        {
            foreach (KeyValuePair<CommandKey, ICommand> Command in commands)
            {
                commandBindings.Remove(Command.Key.RoutedCommand);
            }
        }

        private static void Remove(this CommandBindingCollection commandBindings, RoutedCommand command)
        {
            foreach( CommandBinding Binding in commandBindings )
            {
                if( Binding.Command == command )
                {
                    commandBindings.Remove(Binding);
                    return;
                }
            }
        }

        private class Binding : CommandBinding
        {
            private readonly ICommand command;

            public Binding(RoutedCommand routedCommand, ICommand command)
            {
                this.command = command;
                this.Command = routedCommand;
                this.CanExecute += this.Binding_CanExecute;
                this.Executed += this.Binding_Executed;
            }

            void Binding_Executed(object sender, ExecutedRoutedEventArgs e)
            {
                this.command.Execute(e.Parameter);
                e.Handled = true;
            }

            void Binding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = this.command.CanExecute(e.Parameter);
                e.Handled = true;
            }
        }

    }
}