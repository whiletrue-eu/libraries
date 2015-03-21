using System;
using WhileTrue.Classes.Components;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Facades.SmartCardUI;

namespace WhileTrue.Components.SmartCardUI
{
    ///<summary>
    /// Provides Smart Card UI
    ///</summary>
    [Component]
    public class SmartCardUIProvider : ISmartCardUIProvider
    {
        private readonly ISmartCardSelectionProvider selectionProvider;

        public SmartCardUIProvider(ISmartCardSelectionProvider selectionProvider)
        {
            this.selectionProvider = selectionProvider;
        }

        public IVariableResolver GetVariableResolverUI()
        {
            throw new NotImplementedException();
        }

        public ISmartCard SelectSmartCard(ISmartCardService smartCardService, string details)
        {
            return this.selectionProvider.SelectSmartCard(smartCardService, details);
        }

        public ICardReader SelectCardReader(ISmartCardService smartCardService, string details)
        {
            return this.selectionProvider.SelectCardReader(smartCardService, details);

        }
    }
}