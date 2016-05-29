using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Converts a byte array to an hexadecimal representation, using the parameter string to separate bytes
    /// </summary>
    /// <remarks>
    /// namespace: wt = http://schemas.whiletrue.eu/xaml<br/>
    /// <br/>
    /// Usage:
    /// <code>
    /// &lt;ResourceDictionary>
    ///   &lt;wt:HexAsciiConverter x:Key="hexAsciiConverter"/>
    /// &lt;/ResourceDictionary>
    /// 
    /// Visibility="{Binding Path=...,Converter={StaticResource hexAsciiConverter}, ConverterParameter=' '}"
    /// </code>
    /// </remarks>
    public class HexAsciiConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string) && targetType != typeof(IEnumerable<byte>))
            {
                throw new InvalidOperationException("Converter only supports strings as targetType");
            }

            if (value != null)
            {
                string Separator = parameter.ToString();
                return ((IEnumerable<byte>) value).ToArray().ToHexString(Separator);
            }
            else
            {
                return null;
            }
        }

        /// <summary/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}