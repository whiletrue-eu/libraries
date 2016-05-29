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
            componentRepository.AddComponent<PcscSmartCardServiceModule>();
            componentRepository.AddComponent<WpfSmartCardUiModule>();
            componentRepository.AddComponent<ApduSenderModule>();
#if DEBUG
            componentRepository.AddComponent<WhileTrue.Modules.ModelInspector.ModelInspectorModule>();
#endif
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }

        public int Run(ComponentContainer componentContainer)
        {
            IApduSender ApduSender = componentContainer.ResolveInstance<IApduSender>();
            ApduSender.Open();
            return 0;
        }
    }
}
