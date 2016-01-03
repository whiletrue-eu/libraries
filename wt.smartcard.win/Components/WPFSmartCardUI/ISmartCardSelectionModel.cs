using WhileTrue.Classes.Components;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.WPFSmartCardUI
{
    [ComponentInterface]
    internal interface ISmartCardSelectionModel
    {
        bool AcceptEmptyCardReader { set; }
        string Details { set; }
        CardReaderAdapter SelectedCardReader { get; set; }
        ISmartCardService SmartCardService { set; }
    }
}