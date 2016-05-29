using System.Collections.Generic;

namespace WhileTrue.Modules.ModelInspector
{
    internal interface IModelComplexNode : IModelNodeBase
    {
        IEnumerable<IPropertyNode> Properties { get; }
    }
}