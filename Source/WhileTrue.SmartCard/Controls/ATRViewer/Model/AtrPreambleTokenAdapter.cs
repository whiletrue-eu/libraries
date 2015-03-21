using System;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Framework;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Controls.ATRView
{
    public class AtrPreambleTokenAdapter : AtrTokenAdapterBase
    {
        private static readonly ReadOnlyPropertyAdapter<AtrPreambleTokenAdapter, string> codingConventionAdapter;
        private static readonly ReadOnlyPropertyAdapter<AtrPreambleTokenAdapter, int> numberOfHistoricalCharactersAdapter;
        private static readonly ReadOnlyPropertyAdapter<AtrPreambleTokenAdapter, string> nextBytesTypeAdapter;

        static AtrPreambleTokenAdapter()
        {
            IPropertyAdapterFactory<AtrPreambleTokenAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrPreambleTokenAdapter>();

            codingConventionAdapter = PropertyFactory.Create(
                @this => @this.CodingConvention,
                @this => ToString(@this.preamble.CodingConvention)
                );
            numberOfHistoricalCharactersAdapter = PropertyFactory.Create(
                @this => @this.NumberOfHistoricalCharacters,
                @this => @this.preamble.NumberOfHistoricalCharacters
                );
            nextBytesTypeAdapter = PropertyFactory.Create(
                @this => @this.NextBytes,
                @this => @this.preamble.NextInterfaceBytesIndicator != null ? ToString(@this.preamble.NextInterfaceBytesIndicator.TaExists, @this.preamble.NextInterfaceBytesIndicator.TbExists, @this.preamble.NextInterfaceBytesIndicator.TcExists, @this.preamble.NextInterfaceBytesIndicator.TdExists) : "not set"
                );
        }

        private readonly AtrPreambleToken preamble;

        public AtrPreambleTokenAdapter(AtrPreambleToken preamble)
            : base(preamble)
        {
            this.preamble = preamble;
        }

        public string NextBytes
        {
            get { return nextBytesTypeAdapter.GetValue(this); }
        }

        public int NumberOfHistoricalCharacters
        {
            get { return numberOfHistoricalCharactersAdapter.GetValue(this); }
        }

        public string CodingConvention
        {
            get { return codingConventionAdapter.GetValue(this); }
        }

        private static string ToString(CodingConvention convention)
        {
            switch(convention)
            {
                case WhileTrue.Types.SmartCard.CodingConvention.Direct:
                    return "Direct";
                case WhileTrue.Types.SmartCard.CodingConvention.Inverse:
                    return "Inverse";
                default:
                    throw new ArgumentOutOfRangeException("convention");
            }
        }
    }
}