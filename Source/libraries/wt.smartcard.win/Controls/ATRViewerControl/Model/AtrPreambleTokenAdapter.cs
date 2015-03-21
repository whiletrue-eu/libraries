using System;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class AtrPreambleTokenAdapter : AtrTokenAdapterBase
    {
        private static readonly ReadOnlyPropertyAdapter<AtrPreambleTokenAdapter, string> codingConventionAdapter;
        private static readonly ReadOnlyPropertyAdapter<AtrPreambleTokenAdapter, byte> numberOfHistoricalCharactersAdapter;
        private static readonly ReadOnlyPropertyAdapter<AtrPreambleTokenAdapter, string> nextBytesTypeAdapter;

        static AtrPreambleTokenAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<AtrPreambleTokenAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrPreambleTokenAdapter>();

            AtrPreambleTokenAdapter.codingConventionAdapter = PropertyFactory.Create(
                nameof(AtrPreambleTokenAdapter.CodingConvention),
                instance => AtrPreambleTokenAdapter.ToString(instance.preamble.CodingConvention)
                );
            AtrPreambleTokenAdapter.numberOfHistoricalCharactersAdapter = PropertyFactory.Create(
                nameof(AtrPreambleTokenAdapter.NumberOfHistoricalCharacters),
                instance => instance.preamble.NumberOfHistoricalCharacters
                );
            AtrPreambleTokenAdapter.nextBytesTypeAdapter = PropertyFactory.Create(
                nameof(AtrPreambleTokenAdapter.NextBytes),
                instance => instance.preamble.NextInterfaceBytesIndicator != null ? AtrTokenAdapterBase.ToString(instance.preamble.NextInterfaceBytesIndicator.TaExists, instance.preamble.NextInterfaceBytesIndicator.TbExists, instance.preamble.NextInterfaceBytesIndicator.TcExists, instance.preamble.NextInterfaceBytesIndicator.TdExists) : "not set"
                );
        }

        private readonly AtrPreambleToken preamble;

        public AtrPreambleTokenAdapter(AtrPreambleToken preamble)
            : base(preamble)
        {
            this.preamble = preamble;
        }

        public string NextBytes => AtrPreambleTokenAdapter.nextBytesTypeAdapter.GetValue(this);

        public byte NumberOfHistoricalCharacters => AtrPreambleTokenAdapter.numberOfHistoricalCharactersAdapter.GetValue(this);

        public string CodingConvention => AtrPreambleTokenAdapter.codingConventionAdapter.GetValue(this);

        private static string ToString(CodingConvention convention)
        {
            switch(convention)
            {
                case Classes.ATR.CodingConvention.Direct:
                    return "Direct";
                case Classes.ATR.CodingConvention.Inverse:
                    return "Inverse";
                default:
                    throw new ArgumentOutOfRangeException(nameof(convention));
            }
        }
    }
}