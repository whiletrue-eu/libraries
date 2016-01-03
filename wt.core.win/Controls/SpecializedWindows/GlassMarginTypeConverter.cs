using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Converter for glass margin. support thinckness (1,2,4 values for fix borders), 
    /// 'sheet' to have a complete glass window and a reference to a control to align the glass border to that control
    /// </summary>
    public class GlassMarginTypeConverter : TypeConverter
    {
        private readonly TypeConverter thicknessConverter;

        /// <summary/>
        public GlassMarginTypeConverter()
        {
            this.thicknessConverter = TypeDescriptor.GetConverter(typeof (Thickness));
        }

        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="sourceType">A <see cref="T:System.Type"/> that represents the type you want to convert from. </param>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (typeof (FrameworkElement).IsAssignableFrom(sourceType))
            {
                return true;
            }
            else
            {
                //Inherit conversion behavior from 'Thickness' type
                return this.thicknessConverter.CanConvertFrom(context, sourceType);
            }
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the converted value.
        /// </returns>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture. </param><param name="value">The <see cref="T:System.Object"/> to convert. </param><exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is FrameworkElement)
            {
                return new DataBoundGlassMargin((FrameworkElement) value);
            }
            else if (value is string && "sheet".Equals((string) value, StringComparison.OrdinalIgnoreCase))
            {
                return GlassMargin.Sheet;
            }
            else
            {
                Thickness Margin = (Thickness) this.thicknessConverter.ConvertFrom(context, culture, value).DbC_AssureNotNull();
                return new GlassMargin(Margin.Left, Margin.Top, Margin.Right, Margin.Bottom);
            }
        }
    }
}