using WhileTrue.Classes.Components;

namespace WhileTrue.DragNDrop.Modules.ImageLibraryViewer
{
    [ComponentInterface]
    internal interface IImageLibraryViewerView
    {
        IImageLibraryViewerModel Model { set; }
        void Open();
    }
}