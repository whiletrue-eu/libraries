// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Framework;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.WPFSmartCardUI
{
    [Component]
    internal class SmartCardSelectionModel : ObservableObject, ISmartCardSelectionModel
    {
        private string title;
        private string subtitle;
        private readonly EnumerablePropertyAdapter<ICardReader, CardReaderAdapter> cardReadersAdapter;
        private CardReaderAdapter selectedCardReader;
        private bool acceptEmptyCardReader;
        private readonly ReadOnlyPropertyAdapter<bool> allowCloseAdapter;
        private readonly ReadOnlyPropertyAdapter<bool> smartCardMustBeInsertedAdapter;
        private ISmartCardService smartCardService;

        public SmartCardSelectionModel()
        {
            this.cardReadersAdapter = this.CreatePropertyAdapter(
                nameof(this.CardReaders),
                () => this.SmartCardService != null ? this.SmartCardService.CardReaders : (IEnumerable<ICardReader>) new ICardReader[0],
                reader => new CardReaderAdapter(reader)
                );
            ((INotifyCollectionChanged) this.CardReaders).CollectionChanged += this.CardReadersChanged;

            this.SelectedCardReader = this.CardReaders.FirstOrDefault();

            this.allowCloseAdapter = this.CreatePropertyAdapter(
                nameof(this.AllowClose),
                () => this.SelectedCardReader != null && (this.AcceptEmptyCardReader || this.SelectedCardReader.SmartCard != null)
                );
            this.smartCardMustBeInsertedAdapter = this.CreatePropertyAdapter(
                nameof(this.SmartCardMustBeInserted),
                () => this.AcceptEmptyCardReader == false && this.CardReaders.Any() && this.SelectedCardReader.SmartCard == null
                );
        }

        private void CardReadersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.SelectedCardReader = (CardReaderAdapter) e.NewItems[0];
            }
        }

        public bool AcceptEmptyCardReader
        {
            set
            {
                this.SetAndInvoke(ref this.acceptEmptyCardReader, value);
                this.SetAndInvoke(nameof(this.Title), ref this.title, value ? "Select Card Reader" : "Select Smart Card");
            }
            get { return this.acceptEmptyCardReader; }
        }

        public string Details
        {
            set { this.SetAndInvoke(nameof(this.Subtitle), ref this.subtitle, value); }
        }

        public string Title => this.title;

        public string Subtitle => this.subtitle;

        public bool AllowClose => this.allowCloseAdapter.GetValue();

        public bool SmartCardMustBeInserted => this.smartCardMustBeInsertedAdapter.GetValue();

        public IEnumerable<CardReaderAdapter> CardReaders => this.cardReadersAdapter.GetCollection();

        public CardReaderAdapter SelectedCardReader
        {
            get { return this.selectedCardReader; }
            set { this.SetAndInvoke(ref this.selectedCardReader, value); }
        }

        public ISmartCardService SmartCardService
        {
            set { this.SetAndInvoke(ref this.smartCardService, value); }
            get { return this.smartCardService; }
        }

    }
}
