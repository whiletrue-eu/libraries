using WhileTrue.Classes.Components;
using WhileTrue.Common.Components.CommonDialogs;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.Common.Modules.CommonDialogs
{
    public class WpfCommonDialogsModule : IModule
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<WpfCommonDialogProvider>();   
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }
    }
}
