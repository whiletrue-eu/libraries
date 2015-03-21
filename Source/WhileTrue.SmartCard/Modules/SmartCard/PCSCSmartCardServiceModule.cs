using WhileTrue.Classes.Components;
using WhileTrue.Components.CardReaderSubsystem.PCSC;
using WhileTrue.Components.SmartCardService;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.Modules.SmartCard
{
    [Component]
    public class PCSCSmartCardServiceModule : IModule
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<SmartCardService>();
            componentRepository.AddComponent<PCSCSmartCardSubsystem>();
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }
    }
}
