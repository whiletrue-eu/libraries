#pragma warning disable 1591
// ReSharper disable UnusedMember.Global
using System;
using System.Windows.Markup;

namespace WhileTrue.Classes.Commanding
{
    public class RoutedCommandExtension : MarkupExtension
    {
        private string commandID;

        public RoutedCommandExtension()
        {
        }

        public RoutedCommandExtension(string commandID)
        {
            this.commandID = commandID;
        }

        [ConstructorArgument("commandID")]
        public string CommandID
        {
            get { return this.commandID; }
            set { this.commandID = value; }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return RoutedCommandFactory.GetRoutedCommand(this.commandID);
        }
    }
}