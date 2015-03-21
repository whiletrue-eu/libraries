﻿using WhileTrue.Classes.Components;

namespace WhileTrue.Components.SmartCardUI
{
    [ComponentInterface]
    internal interface ISmartCardSelectionView
    {
        void ShowModal();
        ISmartCardSelectionModel Model { set; }
    }
}