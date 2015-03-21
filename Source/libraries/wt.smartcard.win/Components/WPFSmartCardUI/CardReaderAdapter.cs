using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.WPFSmartCardUI
{
    internal class CardReaderAdapter:ObservableObject
    {
        private readonly ReadOnlyPropertyAdapter<string> nameAdapter;
        private readonly ReadOnlyPropertyAdapter<string> cardNameAdapter;
        private readonly ReadOnlyPropertyAdapter<ISmartCard> smartCardAdapter;

        public CardReaderAdapter(ICardReader cardReader)
        {
            this.CardReader = cardReader;

            this.nameAdapter = this.CreatePropertyAdapter(
                nameof(this.Name),
                ()=>cardReader.Name
                );

            this.cardNameAdapter = this.CreatePropertyAdapter(
                nameof(this.CardName),
                ()=>cardReader.SmartCard!=null? $"ATR: {cardReader.SmartCard.Atr.ToHexString()}" :"[no card inserted]"
                );

            this.smartCardAdapter = this.CreatePropertyAdapter(
                nameof(this.SmartCard),
                () => cardReader.SmartCard
                );
        }

        public string Name => this.nameAdapter.GetValue();

        public string CardName => this.cardNameAdapter.GetValue();

        internal ICardReader CardReader { get; }

        internal ISmartCard SmartCard => this.smartCardAdapter.GetValue();
    }
}