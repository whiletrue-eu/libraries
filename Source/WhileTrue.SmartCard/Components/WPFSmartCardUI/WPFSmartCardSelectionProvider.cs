using WhileTrue.Classes.Components;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.SmartCardUI
{
    ///<summary/>
    [Component]
    public class WPFSmartCardSelectionProvider : ISmartCardSelectionProvider
    {
        private readonly ComponentRepository componentRepository;

        ///<summary/>
        public WPFSmartCardSelectionProvider(ComponentRepository componentRepository)
        {
            this.componentRepository = componentRepository;
        }

        public ISmartCard SelectSmartCard(ISmartCardService smartCardService, string details)
        {
            using(ComponentContainer ComponentContainer = new ComponentContainer(this.componentRepository))
            {
                ISmartCardSelection Selection = ComponentContainer.ResolveInstance<ISmartCardSelection>();
                ICardReader Reader = Selection.ShowModal(smartCardService, false, details);
                return Reader.SmartCard;
            }
        }

        public ICardReader SelectCardReader(ISmartCardService smartCardService, string details)
        {
            using (ComponentContainer ComponentContainer = new ComponentContainer(this.componentRepository))
            {
                ISmartCardSelection Selection = ComponentContainer.ResolveInstance<ISmartCardSelection>();
                return Selection.ShowModal(smartCardService, true, details);
            }
        }
    }
}