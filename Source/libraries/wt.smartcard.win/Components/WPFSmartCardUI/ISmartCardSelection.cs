using WhileTrue.Classes.Components;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.WPFSmartCardUI
{
    [ComponentInterface]
    public interface ISmartCardSelection
    {
        ICardReader ShowModal(ISmartCardService smartCardService, bool acceptEmptyCardReader, string details);
    }
}