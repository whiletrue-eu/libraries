using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using WhileTrue.Classes.Utilities;
using Xamarin.Forms;

namespace WhileTrue.Controls
{
    /// <summary>
    ///     Converts from several types to 'content availablity', used for <see cref="ContentUnavailableControl" />.
    ///     Numeric values that equal zero, boolean false, empty or null string and null object references all yield
    ///     'unavailable'
    /// </summary>
    public class ContentAvailabilityConverter : TypeConverter, IValueConverter
    {
        /// <summary>
        ///     Returns whether this converter can convert an object of the given type to the type of this converter, using the
        ///     specified context.
        /// </summary>
        /// <returns>
        ///     true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        public override bool CanConvertFrom(Type sourceType)
        {
            return true;
        }

        /// <summary>
        ///     Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Object" /> that represents the converted value.
        /// </returns>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public override object ConvertFrom(CultureInfo culture, object value)
        {
            if (value is bool)
                return Convert((bool) value);
            if (value is byte)
                return Convert((byte) value != 0);
            if (value is sbyte)
                return Convert((sbyte) value != 0);
            if (value is short)
                return Convert((short) value != 0);
            if (value is ushort)
                return Convert((ushort) value != 0);
            if (value is int)
                return Convert((int) value != 0);
            if (value is uint)
                return Convert((uint) value != 0);
            if (value is long)
                return Convert((long) value != 0);
            if (value is ulong)
                return Convert((ulong) value != 0);
            if (value is float)
                return Convert((float) value != 0);
            if (value is double)
                return Convert((double) value != 0);
            if (value is string)
                return Convert(string.IsNullOrEmpty((string) value) == false);
            return Convert(value != null);
        }

        private static ContentAvailability Convert(bool value)
        {
            return value ? ContentAvailability.Available : ContentAvailability.Unavailable;
        }

        #region Implementation of IValueConverter

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
            return Conversion.ChangeType(value, targetType);
        }

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}