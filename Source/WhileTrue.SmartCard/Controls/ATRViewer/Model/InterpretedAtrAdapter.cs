using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class InterpretedAtrAdapter : ObservableObject
    {
        private readonly Atr atr;
        private static readonly ReadOnlyPropertyAdapter<InterpretedAtrAdapter, GlobalInterfaceBytesAdapter> globalInterfaceBytesAdapter;
        private static readonly EnumerablePropertyAdapter<InterpretedAtrAdapter, ProtocolParametersBase, ProtocolParameterAdapterBase> protocolParametersAdapter;
        private static readonly ReadOnlyPropertyAdapter<InterpretedAtrAdapter, AtrHistoricalBytesAdapterBase> historicalCharactersAdapter;
        private static readonly ReadOnlyPropertyAdapter<InterpretedAtrAdapter, IEnumerable<EnumerationAdapter<ProtocolType>>> possibleTypesToIndicateAdditionallyAdapter;
        private readonly DelegateCommand<EnumerationAdapter<ProtocolType>> indicateProtocolCommand;
        private static readonly ReadOnlyPropertyAdapter<InterpretedAtrAdapter, IEnumerable<EnumerationAdapter<HistoricalCharacterTypes>>> historicalCharacterTypesAdapter;

        static InterpretedAtrAdapter()
        {
            IPropertyAdapterFactory<InterpretedAtrAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<InterpretedAtrAdapter>();

            globalInterfaceBytesAdapter = PropertyFactory.Create(
                @this => @this.GlobalInterfaceBytes,
                @this => new GlobalInterfaceBytesAdapter(@this.atr.GlobalInterfaceBytes)
                );
            protocolParametersAdapter = PropertyFactory.Create(
                @this => @this.ProtocolParameters,
                @this => @this.atr.ProtocolParameters,
                (@this, parameter) => ProtocolParameterAdapterBase.GetObject(parameter)
                );
            historicalCharactersAdapter = PropertyFactory.Create(
                @this => @this.HistoricalCharacters,
                @this => AtrHistoricalBytesAdapterBase.GetObject(@this.atr.HistoricalCharacters, @this)
                );
            possibleTypesToIndicateAdditionallyAdapter = PropertyFactory.Create(
                @this=>@this.PossibleTypesToIndicateAdditionally,
                @this=>EnumerationAdapter<ProtocolType>.Items.Where(
                    type => type != ProtocolType.RFU_F &&
                            @this.atr.ProtocolParameters.Any(_ => _.ProtocolType == type)==false
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

        public IEnumerable<ProtocolParameterAdapterBase> ProtocolParameters
        {
            get { return protocolParametersAdapter.GetCollection(this); }
        }

        public GlobalInterfaceBytesAdapter GlobalInterfaceBytes
        {
            get { return globalInterfaceBytesAdapter.GetValue(this); }
        }

        public AtrHistoricalBytesAdapterBase HistoricalCharacters
        {
            get { return historicalCharactersAdapter.GetValue(this); }
        }

        public IEnumerable<EnumerationAdapter<ProtocolType>> PossibleTypesToIndicateAdditionally
        {
            get { return possibleTypesToIndicateAdditionallyAdapter.GetValue(this); } 
        }

        public ICommand IndicateProtocolCommand
        {
            get { return this.indicateProtocolCommand; }
        }

        public void SetHistoricalCharacterType(HistoricalCharacterTypes type)
        {
            this.atr.SetHistoricalCharactersType(type);
        }
    }
}