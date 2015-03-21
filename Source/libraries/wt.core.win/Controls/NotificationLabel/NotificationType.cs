using System.ComponentModel;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
        ///<summary/>
        [TypeConverter(typeof(NotificationTypeTypeConverter))]
    public enum NotificationType
    {
        ///<summary/>
        Info = ValidationSeverity.Info,
        ///<summary/>
        Warning = ValidationSeverity.Warning,
        ///<summary/>
        Error = ValidationSeverity.Error,
    }
}