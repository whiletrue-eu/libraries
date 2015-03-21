using WhileTrue.Classes.Components;

namespace WhileTrue.Modules.ModelInspectorWindow
{
    [ComponentInterface]
    internal interface IModelInspectorWindowView
    {
        IModelInspectorWindowModel Model { set; }
        void Show();
    }
}