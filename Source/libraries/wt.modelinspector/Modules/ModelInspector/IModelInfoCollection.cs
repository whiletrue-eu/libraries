using System.Collections.Generic;

namespace WhileTrue.Modules.ModelInspector
{
    internal interface IModelInfoCollection : IEnumerable<IModelInfo>
    {
        void Add(IModelInfo model);
        int Count { get; }
        void Insert(int index, IModelInfo model);
        void Move(IModelInfo model, int newIndex);
        void Remove(IModelInfo value);
    }
}