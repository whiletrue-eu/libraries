using WhileTrue.Classes.Components;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.WPFSmartCardUI
{
    [ComponentInterface]
    public interface ISmartCardSelectionProvider
    {
        /// <summary>
        /// Prompts the user to select a smart cardwithin a card reader
        /// </summary>
        ISmartCard SelectSmartCard(ISmartCardService smartCardService, string details);
        /// <summary>
        /// Prompts the user to select a card reader, even if no card is inserted
        /// </summary>
        ICardReader SelectCardReader(ISmartCardService smartCardService, string details);
    }
}