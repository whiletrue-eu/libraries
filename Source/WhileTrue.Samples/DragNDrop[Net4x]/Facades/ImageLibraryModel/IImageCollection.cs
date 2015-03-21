using System.Collections.Generic;

namespace WhileTrue.DragNDrop.Facades.ImageLibraryModel
{
    internal interface IImageCollection : IEnumerable<IImage>
    {
        IImage Add(string path);
        void Remove(IImage image);
    }
}