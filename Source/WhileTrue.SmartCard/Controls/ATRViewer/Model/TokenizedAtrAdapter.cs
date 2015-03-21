using System.Collections.Generic;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
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
            preambleAdapter = PropertyFactory.Create(
                @this => @this.Preamble,
                @this => new AtrPreambleTokenAdapter(@this.tokenizedAtr.Preamble)
                );
            interfaceByteGroupsAdapter = PropertyFactory.Create(
                @this => @this.InterfaceBytesGroups,
                @this => @this. tokenizedAtr.InterfaceByteGroups,
                (@this, group) => new AtrInterfaceByteGroupTokenAdapter(group)
                );
            historicalCharactersAdapter = PropertyFactory.Create(
                @this => @this.HistoricalCharacters,
                @this =>  new AtrHistoricalCharactersTokenAdapter(@this.tokenizedAtr.HistoricalCharacters)
                );
            checksumAdapter = PropertyFactory.Create(
                @this => @this.Checksum,
                @this => new AtrChecksumTokenAdapter(@this.tokenizedAtr.AtrChecksum)
                );
            extraBytesAdapter = PropertyFactory.Create(
                @this => @this.ExtraBytes,
                @this => new AtrExtraBytesAdapter(@this.tokenizedAtr.ExtraBytes)
                );
        }

        public TokenizedAtrAdapter(TokenizedAtr tokenizedAtr)
        {
            this.tokenizedAtr = tokenizedAtr;
        }

        public AtrExtraBytesAdapter ExtraBytes
        {
            get { return extraBytesAdapter.GetValue(this); }
        }

        public AtrChecksumTokenAdapter Checksum
        {
            get { return checksumAdapter.GetValue(this); }
        }

        public AtrHistoricalCharactersTokenAdapter HistoricalCharacters
        {
            get { return historicalCharactersAdapter.GetValue(this); }
        }

        public IEnumerable<AtrInterfaceByteGroupTokenAdapter> InterfaceBytesGroups
        {
            get { return interfaceByteGroupsAdapter.GetCollection(this); }
        }

        public AtrPreambleTokenAdapter Preamble
        {
            get { return preambleAdapter.GetValue(this); }
        }
    }
}