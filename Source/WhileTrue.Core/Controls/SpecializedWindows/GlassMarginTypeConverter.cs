using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace WhileTrue.Controls
{
    public class GlassMarginTypeConverter : TypeConverter
    {
        private readonly TypeConverter thicknessConverter;

        public GlassMarginTypeConverter()
        {
            this.thicknessConverter = TypeDescriptor.GetConverter(typeof (Thickness));
        }

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
                Thickness Margin = (Thickness) this.thicknessConverter.ConvertFrom(context, culture, value);
                return new GlassMargin(Margin.Left, Margin.Top, Margin.Right, Margin.Bottom);
            }
        }
    }
}