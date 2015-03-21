using System.Collections.Generic;

namespace WhileTrue.DragNDrop.Facades.ImageLibraryModel
{
    internal interface IGroupCollection : IEnumerable<IGroup>
    {
        IGroup Add(string name);
        void Remove(IGroup group);
    }
}