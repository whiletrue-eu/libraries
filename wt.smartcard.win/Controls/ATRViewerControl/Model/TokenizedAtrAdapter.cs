using System.Collections.Generic;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class TokenizedAtrAdapter : ObservableObject
    {
        private readonly TokenizedAtr tokenizedAtr;
        private static readonly ReadOnlyPropertyAdapter<TokenizedAtrAdapter, AtrPreambleTokenAdapter> preambleAdapter;
        private static readonly EnumerablePropertyAdapter<TokenizedAtrAdapter, AtrInterfaceByteGroupToken, AtrInterfaceByteGroupTokenAdapter> interfaceByteGroupsAdapter;
        private static readonly ReadOnlyPropertyAdapter<TokenizedAtrAdapter, AtrHistoricalCharactersTokenAdapter> historicalCharactersAdapter;
        private static readonly ReadOnlyPropertyAdapter<TokenizedAtrAdapter, AtrChecksumTokenAdapter> checksumAdapter;
        private static readonly ReadOnlyPropertyAdapter<TokenizedAtrAdapter, AtrExtraBytesAdapter> extraBytesAdapter;

        static TokenizedAtrAdapter()
        {
            IPropertyAdapterFactory<TokenizedAtrAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<TokenizedAtrAdapter>();
            TokenizedAtrAdapter.preambleAdapter = PropertyFactory.Create(
                nameof(TokenizedAtrAdapter.Preamble),
                instance => new AtrPreambleTokenAdapter(instance.tokenizedAtr.Preamble)
                );
            TokenizedAtrAdapter.interfaceByteGroupsAdapter = PropertyFactory.Create(
                nameof(TokenizedAtrAdapter.InterfaceBytesGroups),
                instance => instance. tokenizedAtr.InterfaceByteGroups,
                (instance, group) => new AtrInterfaceByteGroupTokenAdapter(group)
                );
            TokenizedAtrAdapter.historicalCharactersAdapter = PropertyFactory.Create(
                nameof(TokenizedAtrAdapter.HistoricalCharacters),
                instance =>  new AtrHistoricalCharactersTokenAdapter(instance.tokenizedAtr.HistoricalCharacters)
                );
            TokenizedAtrAdapter.checksumAdapter = PropertyFactory.Create(
                nameof(TokenizedAtrAdapter.Checksum),
                instance => new AtrChecksumTokenAdapter(instance.tokenizedAtr.AtrChecksum)
                );
            TokenizedAtrAdapter.extraBytesAdapter = PropertyFactory.Create(
                nameof(TokenizedAtrAdapter.ExtraBytes),
                instance => new AtrExtraBytesAdapter(instance.tokenizedAtr.ExtraBytes)
                );
        }

        public TokenizedAtrAdapter(TokenizedAtr tokenizedAtr)
        {
            this.tokenizedAtr = tokenizedAtr;
        }

        public AtrExtraBytesAdapter ExtraBytes => TokenizedAtrAdapter.extraBytesAdapter.GetValue(this);

        public AtrChecksumTokenAdapter Checksum => TokenizedAtrAdapter.checksumAdapter.GetValue(this);

        public AtrHistoricalCharactersTokenAdapter HistoricalCharacters => TokenizedAtrAdapter.historicalCharactersAdapter.GetValue(this);

        public IEnumerable<AtrInterfaceByteGroupTokenAdapter> InterfaceBytesGroups => TokenizedAtrAdapter.interfaceByteGroupsAdapter.GetCollection(this);

        public AtrPreambleTokenAdapter Preamble => TokenizedAtrAdapter.preambleAdapter.GetValue(this);
    }
}