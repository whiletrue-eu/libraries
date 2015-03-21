using System.Collections.Generic;

namespace WhileTrue.Modules.ModelInspector
{
    internal interface IModelEnumerableNode : IModelNodeBase
    {
        IEnumerable<IEnumerationItemNode> Items { get; }
    }
}