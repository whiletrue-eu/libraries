using System.Collections.Generic;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Modules.ModelInspector
{
    internal interface IContextNodeBase:INodeBase
    {
        string Name { get; }
        IModelNodeBase Value { get; }
    }

    internal interface INodeBase
    {
    }

    internal interface IPropertyNode:IContextNodeBase
    {
        bool SupportsValidation { get; }
        ValidationSeverity ValidationSeverity { get; }
        IEnumerable<ValidationMessage> ValidationResults { get; }
    }

    internal interface IEnumerationItemNode:IContextNodeBase
    {
    }
}