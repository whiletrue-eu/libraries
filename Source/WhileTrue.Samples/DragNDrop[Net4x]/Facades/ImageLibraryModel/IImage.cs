using System.Collections.Generic;

namespace WhileTrue.DragNDrop.Facades.ImageLibraryModel
{
    internal interface IImage
    {
        string Path { get; }
        string Name { get; }
    }
}