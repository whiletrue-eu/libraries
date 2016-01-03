using WhileTrue.Classes.Components;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Facades.SmartCardUI
{
    ///<summary>
    /// Provides UI parts that are commonly used with smart card applications
    ///</summary>
    [ComponentInterface]
    public interface ISmartCardUiProvider
    {
        /// <summary>
        /// Returns an implementation to handle variable resolving
        /// </summary>
        IVariableResolver GetVariableResolverUi();
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
