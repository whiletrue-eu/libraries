using System.Collections.Generic;
using WhileTrue.Classes.Components;
using WhileTrue.Modules.ModelInspectorWindow.Model;

namespace WhileTrue.Modules.ModelInspectorWindow
{
    [ComponentInterface]
    internal interface IModelInspectorWindowModel
    {
        IEnumerable<ModelGroupAdapter> Groups { get; }
    }
}