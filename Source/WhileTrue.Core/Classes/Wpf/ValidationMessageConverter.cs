using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Wpf
{
    ///<summary>
    /// Converts a collection of ValidationErrors (with string content) into a enumeration of ValidationMessages.
    ///</summary>
    /// <remarks>
    /// if the validation error messages contain one or more string-formatted validation messages, they are converted 
    /// back into validation messages with a corresponding severity.
    /// </remarks>
    public class ValidationMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if( value == null )
            {
                return null;
            }
            else if (value is ReadOnlyObservableCollection<ValidationError>)
            {
                ReadOnlyObservableCollection<ValidationError> Errors = (ReadOnlyObservableCollection<ValidationError>) value;
                if (Errors.Any(error => (error.ErrorContent is string) == false))
                {
                    return Errors;
                }
                else
                {
                    return new ValidationErrorCollection(Errors);
                }
            }
            else
            {
                return value;
            }
        }

        public class ValidationErrorCollection : ObservableCollection<ValidationMessage>
        {
            private readonly ReadOnlyObservableCollection<ValidationError> errors;

            public ValidationErrorCollection(ReadOnlyObservableCollection<ValidationError> errors)
            {
                this.errors = errors;
                ((INotifyCollectionChanged) errors).CollectionChanged += ItemsChanged;
                this.UpdateItems();
            }

            private void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                this.UpdateItems();
            }

            private void UpdateItems()
            {
                this.Clear();
                foreach (ValidationMessage Message in Convert(this.errors))
                {
                    this.Add(Message);
                }
            }

            private static IEnumerable<ValidationMessage> Convert(IEnumerable<ValidationError> value)
            {
                return
                    from Error in value
                    select (string) Error.ErrorContent
                    into ErrorMessage
                    from ValidationMessage in ValidationMessage.ParseMultiple(ErrorMessage)
                    orderby ValidationMessage.Severity descending
                    orderby ValidationMessage.Message
                    select ValidationMessage;
            }
        }

        ///<summary/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    ///<summary>
    /// Converts a collection of ValidationErrors (with string content) into the highest severity found in the messsages.
    ///</summary>
    /// <remarks>
    /// if the validation error messages contain one or more string-formatted validation messages, they are converted 
    /// back into validation messages with a corresponding severity.
    /// </remarks>
    public class ValidationMessageSeverityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is IEnumerable<ValidationError>)
            {
                IEnumerable<ValidationError> Errors = (IEnumerable<ValidationError>) value;
                if (Errors.Any(error => (error.ErrorContent is string) == false))
                {
                    return ValidationSeverity.Error;
                }
                else
                {
                    return Convert(Errors);
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