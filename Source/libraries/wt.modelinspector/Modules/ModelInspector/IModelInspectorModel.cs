using WhileTrue.Classes.Components;

namespace WhileTrue.Modules.ModelInspector
{
    [ComponentInterface]
    internal interface IModelInspectorModel
    {
        IModelGroupCollection Groups { get; }
    }
}