using System;
using System.Windows.Data;
using WhileTrue.Classes.DragNDrop.DragDropUIHandler;

namespace WhileTrue.Classes.Wpf
{
    public class NegateBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool) && targetType != typeof(bool?))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }
            if (null == value)
            {
                return null;
            }
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool) && targetType != typeof(bool?))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }
            if (null == value)
            {
                return null;
            }
            return !(bool)value;
        }
    }
}