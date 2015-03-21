using WhileTrue.Classes.Components;
using WhileTrue.Facades.ApplicationLoader;
using WhileTrue.Modules.SmartCard;
using WhileTrue.Modules.SmartCardUI;
using WhileTrue.SmartCard.Facades.APDUSender;
using WhileTrue.SmartCard.Modules.APDUSender;

namespace WhileTrue.SmartCard
{
    [Component]
    public class SmartCardSample : IMainModule
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<PCSCSmartCardServiceModule>();
            componentRepository.AddComponent<WPFSmartCardUIModule>();
            componentRepository.AddComponent<APDUSenderModule>();
#if DEBUG
            componentRepository.AddComponent<WhileTrue.Modules.ModelInspector.ModelInspectorModule>();
#endif
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }

        public int Run(ComponentContainer componentContainer)
        {
            IAPDUSender APDUSender = componentContainer.ResolveInstance<IAPDUSender>();
            APDUSender.Open();
            return 0;
        }
    }
}
