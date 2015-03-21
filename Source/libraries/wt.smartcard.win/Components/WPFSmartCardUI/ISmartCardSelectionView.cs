using WhileTrue.Classes.Components;

namespace WhileTrue.Components.WPFSmartCardUI
{
    [ComponentInterface]
    internal interface ISmartCardSelectionView
    {
        void ShowModal();
        ISmartCardSelectionModel Model { set; }
    }
}