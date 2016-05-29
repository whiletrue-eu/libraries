using System;

namespace WhileTrue.Classes.Framework
{
    /// <summary/>
    public class ValidationEventArgs : EventArgs
    {
        /// <summary/>
        public ValidationEventArgs(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Name of the property for which the validation results changed
        /// </summary>
        public string PropertyName { get; }
    }
}