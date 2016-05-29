using WhileTrue.Classes.Components;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    [Component]
    internal class ApduSenderModule : IModule
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            ComponentRepository ApduSenderRepository = new ComponentRepository(componentRepository);
            ApduSenderRepository.AddComponent<ApduSenderPresenter>();
            ApduSenderRepository.AddComponent<ApduSenderModel>();
            ApduSenderRepository.AddComponent<ApduSenderView>();

            componentRepository.AddComponent<ApduSenderProxy>(ApduSenderRepository);
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }
    }
}
