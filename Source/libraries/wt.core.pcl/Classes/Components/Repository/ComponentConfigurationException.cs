using System;

namespace WhileTrue.Classes.Components
{
    internal class ComponentConfigurationException : Exception
    {
        public ComponentConfigurationException(string message) : base(message)
        {
        }

        public ComponentConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}