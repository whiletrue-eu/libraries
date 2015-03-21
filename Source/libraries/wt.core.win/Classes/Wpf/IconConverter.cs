using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Wpf
{
    ///<summary>
    /// Returns one version of the icon image that is closest to the height that is given as converter parameter to get a
    /// higher quality image than just scaling through <see cref="Image.Stretch"/>.
    ///</summary>
    /// <remarks>
    /// if the parameter is not an int, or if the image converted is no icon,the image is simply returned and
    /// will be scaled as usual.
    /// </remarks>
    [PublicAPI]
    public class IconConverter : IValueConverter 
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
            BitmapFrame Value = value as BitmapFrame;
            double RequestedHeight;
            if (Value != null &&
                double.TryParse(parameter.ToString(), out RequestedHeight) &&
                Value.Decoder is IconBitmapDecoder)
            {
                BitmapDecoder Decoder = Value.Decoder;
                return (from Frame in Decoder.Frames
                        orderby Frame.Format.BitsPerPixel descending
                        orderby Math.Abs(Math.Log(Frame.Height/RequestedHeight))
                        select Frame)
                    .DefaultIfEmpty(Value)
                    .First();
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}