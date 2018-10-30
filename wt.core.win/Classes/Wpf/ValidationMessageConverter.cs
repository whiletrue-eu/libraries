﻿using System;
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
    /// <summary>
    ///     Converts a collection of ValidationErrors (with string content) into a enumeration of ValidationMessages.
    /// </summary>
    /// <remarks>
    ///     if the validation error messages contain one or more string-formatted validation messages, they are converted
    ///     back into validation messages with a corresponding severity.
    /// </remarks>
    public class ValidationMessageConverter : IValueConverter
    {
        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (value is IEnumerable<ValidationError>)
                return new ValidationErrorCollection((IEnumerable<ValidationError>) value);
            return value;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private class ValidationErrorCollection : ObservableCollection<ValidationMessage>
        {
            private readonly IEnumerable<ValidationError> errors;

            public ValidationErrorCollection(IEnumerable<ValidationError> errors)
            {
                this.errors = errors;
                if (errors is INotifyCollectionChanged)
                    ((INotifyCollectionChanged) errors).CollectionChanged += ItemsChanged;
                UpdateItems();
            }

            private void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                UpdateItems();
            }

            private void UpdateItems()
            {
                Clear();
                foreach (var Message in Convert(errors)) Add(Message);
            }

            private static IEnumerable<ValidationMessage> Convert(IEnumerable<ValidationError> value)
            {
                return
                    from Error in value
                    select Error.ErrorContent.ToString()
                    into ErrorMessage
                    from ValidationMessage in ValidationMessage.ParseMultiple(ErrorMessage)
                    orderby ValidationMessage.Severity descending
                    orderby ValidationMessage.Message
                    select ValidationMessage;
            }
        }
    }
}