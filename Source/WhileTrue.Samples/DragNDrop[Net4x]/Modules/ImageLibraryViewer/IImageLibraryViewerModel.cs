using System.Collections.Generic;
using WhileTrue.Classes.Components;

namespace WhileTrue.DragNDrop.Modules.ImageLibraryViewer
{
    [ComponentInterface]
    internal interface IImageLibraryViewerModel
    {
        IEnumerable<ItemAdapterBase> Items { get; }
    }
}