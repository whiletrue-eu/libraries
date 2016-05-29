using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// This class provides a thread-safe wrapper for ICOmmand implementations
    /// </summary>
    /// <remarks>
    /// The CanExecuteChanged command is dispatched into the thread of the GUI
    /// </remarks>
    public class CrossThreadCommandWrapper : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object Command = value;
            if (Command == null)
            {
                return null;
            }
            else if (Command is ICommand)
            {
                return CommandWrapper.GetCommandWrapperInstance((ICommand)Command);
            }
            else
            {
                throw new InvalidOperationException("Collection to wrap is either not enumerable or does not support INotifyCollectionChanged");
            }
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}