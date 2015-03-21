using System;
using System.Collections;
using System.Collections.Specialized;
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}