using System;
using System.Linq.Expressions;

namespace WhileTrue.Modules.ModelInspector
{
    internal interface IModelNodeBase
    {
        Type Type { get; }
        object Value { get; }
    }
}