using WhileTrue.Classes.Components;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    [Component]
    internal class APDUSenderModule : IModule
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            ComponentRepository APDUSenderRepository = new ComponentRepository(componentRepository);
            APDUSenderRepository.AddComponent<APDUSenderPresenter>();
            APDUSenderRepository.AddComponent<APDUSenderModel>();
            APDUSenderRepository.AddComponent<APDUSenderView>();

            componentRepository.AddComponent<APDUSenderProxy>(APDUSenderRepository);
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }
    }
}
