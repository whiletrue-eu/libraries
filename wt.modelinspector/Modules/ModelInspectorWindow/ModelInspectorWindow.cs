using WhileTrue.Classes.Components;

namespace WhileTrue.Modules.ModelInspectorWindow
{
    [Component]
    internal class ModelInspectorWindow :IModelInspectorWindow
    {
        private readonly IModelInspectorWindowView view;

        public ModelInspectorWindow(IModelInspectorWindowView view, IModelInspectorWindowModel model)
        {
            this.view = view;
            this.view.Model = model;
        }

        public void Show()
        {
            this.view.Show();
        }
    }
}