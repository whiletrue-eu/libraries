using System;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.SmartCardUI
{
    internal class CardReaderAdapter:ObservableObject
    {
        private readonly ICardReader cardReader;
        private readonly ReadOnlyPropertyAdapter<string> nameAdapter;
        private readonly ReadOnlyPropertyAdapter<string> cardNameAdapter;
        private readonly ReadOnlyPropertyAdapter<ISmartCard> smartCardAdapter;

        public CardReaderAdapter(ICardReader cardReader)
        {
            this.cardReader = cardReader;

            this.nameAdapter = this.CreatePropertyAdapter(
                ()=>Name,
                ()=>cardReader.Name,
                EventBindingMode.Weak, ValueRetrievalMode.Lazy
                );

            this.cardNameAdapter = this.CreatePropertyAdapter(
                ()=>CardName,
                ()=>cardReader.SmartCard!=null?string.Format("ATR: {0}", cardReader.SmartCard.ATR.ToHexString()):"[no card inserted]",
                EventBindingMode.Weak, ValueRetrievalMode.Lazy
                );

            this.smartCardAdapter = this.CreatePropertyAdapter(
                () => SmartCard,
                () => cardReader.SmartCard,
                EventBindingMode.Weak, ValueRetrievalMode.Lazy
                );
        }

        public string Name
        {
            get { return this.nameAdapter.GetValue();  }
        }

        public string CardName
        {
            get { return this.cardNameAdapter.GetValue();  }
        }

        internal ICardReader CardReader
        {
            get { return this.cardReader; }
        }

        internal ISmartCard SmartCard
        {
            get { return this.smartCardAdapter.GetValue(); }
        }
    }
}