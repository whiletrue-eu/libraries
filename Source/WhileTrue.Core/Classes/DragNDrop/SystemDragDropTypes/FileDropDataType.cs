using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace WhileTrue.Classes.DragNDrop
{
    [TypeConverter(typeof(FileDropDataTypeConverter))]
    public class FileDropDataType
    {
        public FileDropDataType(FileInfo[] files)
        {
            this.files = files;
        }

        private readonly FileInfo[] files;

        public FileInfo[] Files
        {
            get { return this.files; }
        }
    }
}