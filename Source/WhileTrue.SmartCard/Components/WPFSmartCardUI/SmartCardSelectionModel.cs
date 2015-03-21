// ReSharper disable MemberCanBePrivate.Global
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Models;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.SmartCardUI
{
    [Component]
    internal class SmartCardSelectionModel:ObservableObject,ISmartCardSelectionModel
    {
        private string title;
        private string subtitle;
        private readonly EnumerablePropertyAdapter<ICardReader, CardReaderAdapter> cardReadersAdapter;
        private CardReaderAdapter selectedCardReader;
        private bool acceptEmptyCardReader;
        private readonly ReadOnlyPropertyAdapter<bool> allowCloseAdapter;
        private readonly ReadOnlyPropertyAdapter<bool> smartCardMustBeInsertedAdapter;
        private readonly ZoomModel zoomModel;
        private ISmartCardService smartCardService;

        public SmartCardSelectionModel()
        {
            this.cardReadersAdapter = this.CreatePropertyAdapter(
                ()=>CardReaders,
                () => this.SmartCardService!=null?this.SmartCardService.CardReaders:(IEnumerable<ICardReader>)new ICardReader[0],
                EventBindingMode.Weak,ValueRetrievalMode.Lazy,
                reader=>new CardReaderAdapter(reader)
                );
            ((INotifyCollectionChanged) this.CardReaders).CollectionChanged += this.CardReadersChanged;

            this.SelectedCardReader = this.CardReaders.FirstOrDefault();

            this.allowCloseAdapter = this.CreatePropertyAdapter(
                ()=>AllowClose,
                () => this.SelectedCardReader != null && (this.AcceptEmptyCardReader || this.SelectedCardReader.SmartCard != null),
                EventBindingMode.Weak,ValueRetrievalMode.Lazy
                );
            this.smartCardMustBeInsertedAdapter = this.CreatePropertyAdapter(
                () => SmartCardMustBeInserted,
                () => this.AcceptEmptyCardReader == false && this.CardReaders.Count() > 0 && this.SelectedCardReader.SmartCard == null,
                EventBindingMode.Weak, ValueRetrievalMode.Lazy
                );
            this.zoomModel = new ZoomModel{MinimumZoomFactor = -3,MaximumZoomFactor = 3};
        }

        private void CardReadersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if( e.Action == NotifyCollectionChangedAction.Add )
            {
                this.SelectedCardReader = (CardReaderAdapter) e.NewItems[0];
            }
        }

        public bool AcceptEmptyCardReader
        {
            set
            {
                this.SetAndInvoke(() => AcceptEmptyCardReader, ref this.acceptEmptyCardReader, value);
                this.SetAndInvoke(()=>Title, ref this.title, value?"Select Card Reader":"Select Smart Card");
            }
            get
            {
                return this.acceptEmptyCardReader;
            }
        }

        public string Details
        {
            set { this.SetAndInvoke(()=>Subtitle,ref this.subtitle, value); }
        }

        public string Title
        {
            get { return this.title; }
        }

        public string Subtitle
        {
            get { return this.subtitle; }
        }

        public bool AllowClose
        {
            get
            {
                return this.allowCloseAdapter.GetValue();
            }
        }

        public bool SmartCardMustBeInserted
        {
            get
            {
                return this.smartCardMustBeInsertedAdapter.GetValue();
            }
        }

        public IEnumerable<CardReaderAdapter> CardReaders
        {
            get { return this.cardReadersAdapter.GetCollection(); }
        }

        public CardReaderAdapter SelectedCardReader
        {
            get { return this.selectedCardReader; }
            set { this.SetAndInvoke(()=>SelectedCardReader, ref this.selectedCardReader, value); }
        }

        public ISmartCardService SmartCardService
        {
            set { this.SetAndInvoke(() => SmartCardService, ref this.smartCardService, value); }
            get { return this.smartCardService; }
        }

        public ZoomModel ZoomModel
        {
            get { return this.zoomModel; }
        }
    }
}