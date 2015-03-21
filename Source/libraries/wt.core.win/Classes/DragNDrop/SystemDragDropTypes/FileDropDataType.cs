using System.ComponentModel;
using System.IO;

namespace WhileTrue.Classes.DragNDrop.SystemDragDropTypes
{
    /// <summary>
    /// Wraps file path list drag'n'drop data such as used by WIndows Explorer
    /// </summary>
    [TypeConverter(typeof(FileDropDataTypeConverter))]
    public class FileDropDataType
    {
        /// <summary/>
        public FileDropDataType(FileInfo[] files)
        {
            this.Files = files;
        }

        /// <summary>
        /// Reurns the list of file paths wrapped in this instance
        /// </summary>
        public FileInfo[] Files { get; }
    }
}