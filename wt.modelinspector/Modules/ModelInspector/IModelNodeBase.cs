using System;

namespace WhileTrue.Modules.ModelInspector
{
    internal interface IModelNodeBase:INodeBase
    {
        Type Type { get; }
        object Value { get; }
    }
}