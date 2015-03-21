using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

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
    public class IconConverter : IValueConverter 
    { 
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}