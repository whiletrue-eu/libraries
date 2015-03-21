using WhileTrue.Classes.Components;
using WhileTrue.Components.SmartCardUI;
using WhileTrue.Components.WPFSmartCardUI;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.Modules.SmartCardUI
{
    [Component]
    public class WpfSmartCardUiModule : IModule
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<SmartCardUiProvider>();
            componentRepository.AddComponent<WpfSmartCardSelectionProvider>();
            componentRepository.AddComponent<SmartCardSelection>();
            componentRepository.AddComponent<SmartCardSelectionModel>();
            componentRepository.AddComponent<SmartCardSelectionView>();
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }
    }
}
