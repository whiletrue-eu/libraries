using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    public class FileDropDataTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return sourceType == typeof (IDataObject);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is IDataObject)
            {
                IDataObject DataObject = (IDataObject) value;
                return new FileDropDataType(
                    (from Path in ((string[]) DataObject.GetData(DataFormats.FileDrop))
                     select new FileInfo(Path)
                    ).ToArray());
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return destinationType == typeof(IDataObject);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == typeof(IDataObject) && value is FileDropDataType)
            {
                return new DataObject(
                    DataFormats.FileDrop,
                    (from FileInfo in ((FileDropDataType) value).Files
                     select FileInfo.FullName
                    ).ToArray());
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}