using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Wpf
{
    ///<summary>
    /// Converts a collection of ValidationErrors (with string content) into the highest severity found in the messsages.
    ///</summary>
    /// <remarks>
    /// if the validation error messages contain one or more string-formatted validation messages, they are converted 
    /// back into validation messages with a corresponding severity.
    /// </remarks>
    public class ValidationMessageSeverityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is IEnumerable<ValidationError>)
            {
                ValidationError[] Errors = ((IEnumerable<ValidationError>) value).ToArray();
                if (Errors.Any(error => (error.ErrorContent is string) == false))
                {
                    return ValidationSeverity.Error;
                }
                else
                {
                    return ValidationMessageSeverityConverter.Convert(Errors);
                }
            }
            else
            {
                return value;
            }
        }

        private static ValidationSeverity Convert(IEnumerable<ValidationError> value)
        {
            return
                (from Error in value
                    select (string) Error.ErrorContent
                    into ErrorMessage
                    from ValidationMessage in ValidationMessage.ParseMultiple(ErrorMessage)
                    select ValidationMessage.Severity)
                    .DefaultIfEmpty(ValidationSeverity.None)
                    .Max(severity => severity);
        }


        ///<summary/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}