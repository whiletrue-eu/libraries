using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class InterpretedAtrAdapter : ObservableObject
    {
        private readonly Atr atr;
        private static readonly ReadOnlyPropertyAdapter<InterpretedAtrAdapter, GlobalInterfaceBytesAdapter> globalInterfaceBytesAdapter;
        private static readonly EnumerablePropertyAdapter<InterpretedAtrAdapter, ProtocolParametersBase, ProtocolParameterAdapterBase> protocolParametersAdapter;
        private static readonly ReadOnlyPropertyAdapter<InterpretedAtrAdapter, AtrHistoricalBytesAdapterBase> historicalCharactersAdapter;
        private static readonly ReadOnlyPropertyAdapter<InterpretedAtrAdapter, IEnumerable<EnumerationAdapter<ProtocolType>>> possibleTypesToIndicateAdditionallyAdapter;
        private readonly DelegateCommand<EnumerationAdapter<ProtocolType>> indicateProtocolCommand;

        static InterpretedAtrAdapter()
        {
            IPropertyAdapterFactory<InterpretedAtrAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<InterpretedAtrAdapter>();

            InterpretedAtrAdapter.globalInterfaceBytesAdapter = PropertyFactory.Create(
                nameof(InterpretedAtrAdapter.GlobalInterfaceBytes),
                instance => new GlobalInterfaceBytesAdapter(instance.atr.GlobalInterfaceBytes)
                );
            InterpretedAtrAdapter.protocolParametersAdapter = PropertyFactory.Create(
                nameof(InterpretedAtrAdapter.ProtocolParameters),
                instance => instance.atr.ProtocolParameters,
                (instance, parameter) => ProtocolParameterAdapterBase.GetObject(parameter)
                );
            InterpretedAtrAdapter.historicalCharactersAdapter = PropertyFactory.Create(
                nameof(InterpretedAtrAdapter.HistoricalCharacters),
                instance => AtrHistoricalBytesAdapterBase.GetObject(instance.atr.HistoricalCharacters, instance)
                );
            InterpretedAtrAdapter.possibleTypesToIndicateAdditionallyAdapter = PropertyFactory.Create(
                nameof(InterpretedAtrAdapter.PossibleTypesToIndicateAdditionally),
                instance=>EnumerationAdapter<ProtocolType>.Items.Where(
                    type => type != ProtocolType.RfuF &&
                            instance.atr.ProtocolParameters.Any(_ => _.ProtocolType == type)==false
                    )
                );
        }
        public InterpretedAtrAdapter(Atr atr)
        {
            this.atr = atr;
            this.indicateProtocolCommand = new DelegateCommand<EnumerationAdapter<ProtocolType>>(this.IndicateProtocol);
        }

        private void IndicateProtocol(EnumerationAdapter<ProtocolType> type)
        {
            this.atr.IndicateProtocol(type.Value);
        }

        public IEnumerable<ProtocolParameterAdapterBase> ProtocolParameters => InterpretedAtrAdapter.protocolParametersAdapter.GetCollection(this);

        public GlobalInterfaceBytesAdapter GlobalInterfaceBytes => InterpretedAtrAdapter.globalInterfaceBytesAdapter.GetValue(this);

        public AtrHistoricalBytesAdapterBase HistoricalCharacters => InterpretedAtrAdapter.historicalCharactersAdapter.GetValue(this);

        public IEnumerable<EnumerationAdapter<ProtocolType>> PossibleTypesToIndicateAdditionally => InterpretedAtrAdapter.possibleTypesToIndicateAdditionallyAdapter.GetValue(this);

        public ICommand IndicateProtocolCommand => this.indicateProtocolCommand;

        public void SetHistoricalCharacterType(HistoricalCharacterTypes type)
        {
            this.atr.SetHistoricalCharactersType(type);
        }
    }
}