using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls
{
    public class ContentAvailabilityConverter : TypeConverter, IValueConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is bool)
            {
                return Convert((bool) value);
            }
            else if (value is byte)
            {
                return Convert((byte) value != 0);
            }
            else if (value is sbyte)
            {
                return Convert((sbyte) value != 0);
            }
            else if (value is short)
            {
                return Convert((short) value != 0);
            }
            else if (value is ushort)
            {
                return Convert((ushort) value != 0);
            }
            else if (value is int)
            {
                return Convert((int)value != 0);
            }
            else if (value is uint)
            {
                return Convert((uint)value != 0);
            }
            else if (value is long)
            {
                return Convert((long)value != 0);
            }
            else if (value is ulong)
            {
                return Convert((ulong)value != 0);
            }
            else if (value is float)
            {
                return Convert((float)value != 0);
            }
            else if (value is double)
            {
                return Convert((double)value != 0);
            }
            else if (value is string)
            {
                return Convert(string.IsNullOrEmpty((string)value) == false);
            }
            else
            {
                return Convert(value != null);
            }
        }

        private static ContentAvailability Convert(bool value)
        {
            return value?ContentAvailability.Available:ContentAvailability.Unavailable;
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Conversion.ChangeType(value, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}