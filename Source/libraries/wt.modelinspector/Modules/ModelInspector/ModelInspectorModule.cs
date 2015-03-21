using WhileTrue.Classes.Components;
using WhileTrue.Facades.ApplicationLoader;
using WhileTrue.Modules.ModelInspectorWindow.Model;

namespace WhileTrue.Modules.ModelInspector
{
    [Component]
    public class ModelInspectorModule : IModule
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<ModelInspector>();
            componentRepository.AddComponent<ModelInspectorModel>();
            
            componentRepository.AddComponent<ModelInspectorWindow.ModelInspectorWindow>();
            componentRepository.AddComponent<ModelInspectorWindow.ModelInspectorWindowViewProvider>();
            componentRepository.AddComponent<ModelInspectorWindowModel>();
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }
    }
}