using System;
using System.Threading;
using System.Windows.Media;
using WhileTrue.Classes.Components;
using WhileTrue.Controls;

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