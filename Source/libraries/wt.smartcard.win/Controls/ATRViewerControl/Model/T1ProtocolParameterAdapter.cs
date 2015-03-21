using System.Collections.Generic;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class T1ProtocolParameterAdapter : ProtocolParameterAdapterBase
    {
        private readonly T1ProtocolParameters protocolParameters;
        private static readonly PropertyAdapter<T1ProtocolParameterAdapter, byte> informationFieldSizeAdapter;
        private static readonly ReadOnlyPropertyAdapter<T1ProtocolParameterAdapter, bool> informationFieldSizeIsDefaultAdapter;
        private static readonly ReadOnlyPropertyAdapter<T1ProtocolParameterAdapter, byte> characterWaitingTimeAdapter;
        private static readonly ReadOnlyPropertyAdapter<T1ProtocolParameterAdapter, byte> blockWaitingTimeAdapter;
        private static readonly ReadOnlyPropertyAdapter<T1ProtocolParameterAdapter, bool> blockAndCharacterWaitingTimeIsDefaultAdapter;
        private static readonly PropertyAdapter<T1ProtocolParameterAdapter, EnumerationAdapter<RedundancyCodeType>> redundancyCodeAdapter;
        private static readonly ReadOnlyPropertyAdapter<T1ProtocolParameterAdapter, bool> redundancyCodeIsDefaultAdapter;

        static T1ProtocolParameterAdapter()
        {
            IPropertyAdapterFactory<T1ProtocolParameterAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<T1ProtocolParameterAdapter>();

            T1ProtocolParameterAdapter.informationFieldSizeAdapter = PropertyFactory.Create(
                nameof(T1ProtocolParameterAdapter.InformationFieldSize),
                instance => instance.protocolParameters.IfscValue,
                (instance,value) => instance.protocolParameters.Ifsc=value
                );
            T1ProtocolParameterAdapter.informationFieldSizeIsDefaultAdapter = PropertyFactory.Create(
                nameof(T1ProtocolParameterAdapter.InformationFieldSizeIsDefault),
                instance => instance.protocolParameters.Ifsc.HasValue == false
                );
            T1ProtocolParameterAdapter.characterWaitingTimeAdapter = PropertyFactory.Create(
                nameof(T1ProtocolParameterAdapter.CharacterWaitingTime),
                instance => instance.protocolParameters.CwiValue
                );
            T1ProtocolParameterAdapter.blockWaitingTimeAdapter = PropertyFactory.Create(
                nameof(T1ProtocolParameterAdapter.BlockWaitingTime),
                instance => instance.protocolParameters.BwiValue
                );
            T1ProtocolParameterAdapter.blockAndCharacterWaitingTimeIsDefaultAdapter = PropertyFactory.Create(
                nameof(T1ProtocolParameterAdapter.BlockAndCharacterWaitingTimeIsDefault),
                instance => instance.protocolParameters.Bwi.HasValue == false && instance.protocolParameters.Cwi.HasValue==false
                );
            T1ProtocolParameterAdapter.redundancyCodeAdapter = PropertyFactory.Create(
                nameof(T1ProtocolParameterAdapter.RedundancyCode),
                instance => EnumerationAdapter < RedundancyCodeType >.GetInstanceFor(instance.protocolParameters.RedundancyCodeValue),
                (instance,value)=>instance.protocolParameters.RedundancyCode = value
                );
            T1ProtocolParameterAdapter.redundancyCodeIsDefaultAdapter = PropertyFactory.Create(
                nameof(T1ProtocolParameterAdapter.RedundancyCodeIsDefault),
                instance => instance.protocolParameters.RedundancyCode.HasValue == false
                );   
        }
        public T1ProtocolParameterAdapter(T1ProtocolParameters protocolParameters)
            : base(protocolParameters)
        {
            this.protocolParameters = protocolParameters;
 
        }

        public bool RedundancyCodeIsDefault
        {
            get { return T1ProtocolParameterAdapter.redundancyCodeIsDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.protocolParameters.RedundancyCode = null;
                }
                else
                {
                    this.protocolParameters.RedundancyCode = this.protocolParameters.RedundancyCodeValue;
                }
            }
        }

        public EnumerationAdapter<RedundancyCodeType> RedundancyCode
        {
            get { return T1ProtocolParameterAdapter.redundancyCodeAdapter.GetValue(this); }
            set { T1ProtocolParameterAdapter.redundancyCodeAdapter.SetValue(this, value); }
        }

        public IEnumerable<EnumerationAdapter<RedundancyCodeType>> RedundancyCodeValues => EnumerationAdapter<RedundancyCodeType>.Items;

        public bool BlockAndCharacterWaitingTimeIsDefault
        {
            get { return T1ProtocolParameterAdapter.blockAndCharacterWaitingTimeIsDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.protocolParameters.SetCwIandBwiToDefault();
                }
                else
                {
                    this.protocolParameters.SetCwIandBwi(this.protocolParameters.CwiValue,this.protocolParameters.BwiValue);
                }
            }
        }

        public byte BlockWaitingTime
        {
            get { return T1ProtocolParameterAdapter.blockWaitingTimeAdapter.GetValue(this); }
            set
            {
                this.protocolParameters.SetCwIandBwi(this.protocolParameters.CwiValue, value);
            }
        }

        public byte CharacterWaitingTime
        {
            get { return T1ProtocolParameterAdapter.characterWaitingTimeAdapter.GetValue(this); }
            set
            {
                this.protocolParameters.SetCwIandBwi(value, this.protocolParameters.BwiValue);
            }
        }

        public bool InformationFieldSizeIsDefault
        {
            get { return T1ProtocolParameterAdapter.informationFieldSizeIsDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.protocolParameters.Ifsc = null;
                }
                else
                {
                    this.protocolParameters.Ifsc = this.protocolParameters.IfscValue;
                }
            }
        }

        public byte InformationFieldSize
        {
            get { return T1ProtocolParameterAdapter.informationFieldSizeAdapter.GetValue(this); }
            set
            {
                this.protocolParameters.Ifsc = value;
            }
        }
    }
}