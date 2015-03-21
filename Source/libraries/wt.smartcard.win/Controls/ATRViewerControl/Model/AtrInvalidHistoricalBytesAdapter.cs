using System.Linq;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
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

            AtrInvalidHistoricalBytesAdapter.parseErrorAdapter = PropertyFactory.Create(
                nameof(AtrInvalidHistoricalBytesAdapter.ParseError),
                instance => instance.historicalCharacters.ParseError.Error
                );

            AtrInvalidHistoricalBytesAdapter.preErrorDataAdapter = PropertyFactory.Create(
                nameof(AtrInvalidHistoricalBytesAdapter.PreErrorData),
                instance => instance.historicalCharacters.Bytes.Take(instance.historicalCharacters.ParseError.Index).ToHexString(" ")
                );
            AtrInvalidHistoricalBytesAdapter.errorDataAdapter = PropertyFactory.Create(
                nameof(AtrInvalidHistoricalBytesAdapter.ErrorData),
                instance => instance.historicalCharacters.Bytes.Skip(instance.historicalCharacters.ParseError.Index).Take(1).ToHexString(" ")
                );
            AtrInvalidHistoricalBytesAdapter.postErrorDataAdapter = PropertyFactory.Create(
                nameof(AtrInvalidHistoricalBytesAdapter.PostErrorData),
                instance => instance.historicalCharacters.Bytes.Skip(instance.historicalCharacters.ParseError.Index+1).ToHexString(" ")
                );
        }

        public string PostErrorData => AtrInvalidHistoricalBytesAdapter.postErrorDataAdapter.GetValue(this);

        public string ErrorData => AtrInvalidHistoricalBytesAdapter.errorDataAdapter.GetValue(this);

        public string PreErrorData => AtrInvalidHistoricalBytesAdapter.preErrorDataAdapter.GetValue(this);

        public string ParseError => AtrInvalidHistoricalBytesAdapter.parseErrorAdapter.GetValue(this);

        public AtrInvalidHistoricalBytesAdapter(AtrInvalidHistoricalCharacters historicalCharacters, InterpretedAtrAdapter interpretedAtr)
            : base(historicalCharacters, interpretedAtr)
        {
            this.historicalCharacters = historicalCharacters;
        }
    }
}