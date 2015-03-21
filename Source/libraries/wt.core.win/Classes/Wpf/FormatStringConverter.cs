using System;
using System.Globalization;
using System.Windows.Data;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Converts a string (or the string representation of an object) into a string with the formatting given as parameter
    /// </summary>
    /// <remarks>
    /// namespace: wt = http://schemas.whiletrue.eu/xaml<br/>
    /// <br/>
    /// Usage:
    /// <code>
    /// &lt;ResourceDictionary>
    ///   &lt;wt:FormatStringConverter x:Key="formatStringConverter"/>
    /// &lt;/ResourceDictionary>
    /// 
    /// Visibility="{Binding Path=...,Converter={StaticResource formatStringConverter}, ConverterParameter='value: {0}'}"
    /// </code>
    /// </remarks>
    public class FormatStringConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof (string) && targetType != typeof (object))
            {
                throw new InvalidOperationException("Converter only supports strings as targetType");
            }
            string Formatting = parameter.ToString();

            return string.Format(Formatting, value);
        }

        /// <summary/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}