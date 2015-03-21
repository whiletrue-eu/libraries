using System.Linq;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class AtrInvalidHistoricalBytesAdapter : AtrHistoricalBytesAdapterBase
    {
        private readonly AtrInvalidHistoricalCharacters historicalCharacters;
        private static readonly ReadOnlyPropertyAdapter<AtrInvalidHistoricalBytesAdapter, string> parseErrorAdapter;
        private static readonly ReadOnlyPropertyAdapter<AtrInvalidHistoricalBytesAdapter, string> preErrorDataAdapter;
        private static readonly ReadOnlyPropertyAdapter<AtrInvalidHistoricalBytesAdapter, string> errorDataAdapter;
        private static readonly ReadOnlyPropertyAdapter<AtrInvalidHistoricalBytesAdapter, string> postErrorDataAdapter;

        static AtrInvalidHistoricalBytesAdapter()
        {
            IPropertyAdapterFactory<AtrInvalidHistoricalBytesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrInvalidHistoricalBytesAdapter>();

            parseErrorAdapter = PropertyFactory.Create(
                @this => @this.ParseError,
                @this => @this.historicalCharacters.ParseError.Error
                );

            preErrorDataAdapter = PropertyFactory.Create(
                @this => @this.PreErrorData,
                @this => @this.historicalCharacters.Bytes.Take(@this.historicalCharacters.ParseError.Index).ToHexString(" ")
                );
            errorDataAdapter = PropertyFactory.Create(
                @this => @this.ErrorData,
                @this => @this.historicalCharacters.Bytes.Skip(@this.historicalCharacters.ParseError.Index).Take(1).ToHexString(" ")
                );
            postErrorDataAdapter = PropertyFactory.Create(
                @this => @this.PostErrorData,
                @this => @this.historicalCharacters.Bytes.Skip(@this.historicalCharacters.ParseError.Index+1).ToHexString(" ")
                );
        }

        public string PostErrorData { get { return postErrorDataAdapter.GetValue(this); } }

        public string ErrorData { get { return errorDataAdapter.GetValue(this); } }

        public string PreErrorData { get { return preErrorDataAdapter.GetValue(this); } }

        public string ParseError { get { return parseErrorAdapter.GetValue(this); } }

        public AtrInvalidHistoricalBytesAdapter(AtrInvalidHistoricalCharacters historicalCharacters, InterpretedAtrAdapter interpretedAtr)
            : base(historicalCharacters, interpretedAtr)
        {
            this.historicalCharacters = historicalCharacters;
        }
    }
}