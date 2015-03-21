using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Provides information about validation rules that ran on the object contents
    /// </summary>
    [PublicAPI]
    public interface IObjectValidation : INotifyDataErrorInfo
    {
        /// <summary>
        /// Fires when the validation results of the object change
        /// </summary>
        event EventHandler<ValidationEventArgs> ValidationChanged;
        /// <summary>
        /// Allows to preview errors by giving a theoretical value that is used instead of the current one from the property
        /// </summary>
        string PreviewErrors(string propertyName, object value);
        /// <summary>
        /// Gets all validation messages for the given property
        /// </summary>
        IEnumerable<ValidationMessage> GetValidationMessages(string propertyName);
        /// <summary>
        /// Gives an indictation whether error messages are present for the given property
        /// </summary>
        new bool HasErrors(string propertyName);
    }
}