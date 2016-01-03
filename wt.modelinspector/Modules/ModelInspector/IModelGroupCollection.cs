using System.Collections.Generic;

namespace WhileTrue.Modules.ModelInspector
{
    internal interface IModelGroupCollection : IEnumerable<IModelGroup>
    {
        void Add(IModelGroup modelGroup);
        int Count { get; }
        void Insert(int index, IModelGroup modelGroup);
        void Move(IModelGroup modelGroup, int newIndex);
        void Remove(IModelGroup modelGroup);
    }
}