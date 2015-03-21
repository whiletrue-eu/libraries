using System;
using WhileTrue.Classes.Components;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.WPFSmartCardUI
{
    [Component]
    internal class SmartCardSelection : ISmartCardSelection, IDisposable
    {
        private readonly ISmartCardSelectionModel model;
        private readonly ISmartCardSelectionView view;

        public SmartCardSelection(ISmartCardSelectionModel model, ISmartCardSelectionView view)
        {
            this.model = model;
            this.view = view;
            this.view.Model = model;
        }

        public ICardReader ShowModal(ISmartCardService smartCardService, bool acceptEmptyCardReader, string details)
        {
            this.model.SmartCardService = smartCardService;
            this.model.AcceptEmptyCardReader = acceptEmptyCardReader;
            this.model.Details = details;
            this.view.ShowModal();
            return this.model.SelectedCardReader.CardReader;
        }

        public void Dispose()
        {
            
        }
    }
}