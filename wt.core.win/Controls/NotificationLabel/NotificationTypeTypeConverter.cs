using System;
using System.ComponentModel;
using System.Globalization;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
    internal class NotificationTypeTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if( typeof(ValidationSeverity) == sourceType || typeof(string) == sourceType )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return Enum.Parse(typeof (NotificationType), (string) value);
            }
            else if (value is ValidationSeverity)
            {
                return ((ValidationSeverity)value) == ValidationSeverity.ImplicitError ? NotificationType.Error : (NotificationType)value;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}