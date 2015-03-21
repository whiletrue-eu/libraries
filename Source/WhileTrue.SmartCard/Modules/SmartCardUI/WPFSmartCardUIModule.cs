using System;
using WhileTrue.Classes.Components;
using WhileTrue.Components.SmartCardUI;
using WhileTrue.Components.WPFSmartCardUI;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.Modules.SmartCardUI
{
    [Component]
    public class WPFSmartCardUIModule : IModule
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<SmartCardUIProvider>();
            componentRepository.AddComponent<WPFSmartCardSelectionProvider>();
            componentRepository.AddComponent<SmartCardSelection>();
            componentRepository.AddComponent<SmartCardSelectionModel>();
            componentRepository.AddComponent<SmartCardSelectionView>();
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }
    }
}
