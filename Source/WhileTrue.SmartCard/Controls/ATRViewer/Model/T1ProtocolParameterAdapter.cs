using System.Collections.Generic;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
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

            informationFieldSizeAdapter = PropertyFactory.Create(
                @this => @this.InformationFieldSize,
                @this => @this.protocolParameters.IFSCValue,
                (@this,value) => @this.protocolParameters.IFSC=value
                );
            informationFieldSizeIsDefaultAdapter = PropertyFactory.Create(
                @this => @this.InformationFieldSizeIsDefault,
                @this => @this.protocolParameters.IFSC.HasValue == false
                );
            characterWaitingTimeAdapter = PropertyFactory.Create(
                @this => @this.CharacterWaitingTime,
                @this => @this.protocolParameters.CWIValue
                );
            blockWaitingTimeAdapter = PropertyFactory.Create(
                @this => @this.BlockWaitingTime,
                @this => @this.protocolParameters.BWIValue
                );
            blockAndCharacterWaitingTimeIsDefaultAdapter = PropertyFactory.Create(
                @this => @this.BlockAndCharacterWaitingTimeIsDefault,
                @this => @this.protocolParameters.BWI.HasValue == false && @this.protocolParameters.CWI.HasValue==false
                );
            redundancyCodeAdapter = PropertyFactory.Create(
                @this => @this.RedundancyCode,
                @this => EnumerationAdapter < RedundancyCodeType >.GetInstanceFor(@this.protocolParameters.RedundancyCodeValue),
                (@this,value)=>@this.protocolParameters.RedundancyCode = value
                );
            redundancyCodeIsDefaultAdapter = PropertyFactory.Create(
                @this => @this.RedundancyCodeIsDefault,
                @this => @this.protocolParameters.RedundancyCode.HasValue == false
                );   
        }
        public T1ProtocolParameterAdapter(T1ProtocolParameters protocolParameters)
            : base(protocolParameters)
        {
            this.protocolParameters = protocolParameters;
 
        }

        public bool RedundancyCodeIsDefault
        {
            get { return redundancyCodeIsDefaultAdapter.GetValue(this); }
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
            get { return redundancyCodeAdapter.GetValue(this); }
            set { redundancyCodeAdapter.SetValue(this, value); }
        }

        public IEnumerable<EnumerationAdapter<RedundancyCodeType>> RedundancyCodeValues
        {
            get
            {
                return EnumerationAdapter<RedundancyCodeType>.Items;
            }
        }

        public bool BlockAndCharacterWaitingTimeIsDefault
        {
            get { return blockAndCharacterWaitingTimeIsDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.protocolParameters.SetCWIandBWIToDefault();
                }
                else
                {
                    this.protocolParameters.SetCWIandBWI(this.protocolParameters.CWIValue,this.protocolParameters.BWIValue);
                }
            }
        }

        public byte BlockWaitingTime
        {
            get { return blockWaitingTimeAdapter.GetValue(this); }
            set
            {
                this.protocolParameters.SetCWIandBWI(this.protocolParameters.CWIValue, value);
            }
        }

        public byte CharacterWaitingTime
        {
            get { return characterWaitingTimeAdapter.GetValue(this); }
            set
            {
                this.protocolParameters.SetCWIandBWI(value, this.protocolParameters.BWIValue);
            }
        }

        public bool InformationFieldSizeIsDefault
        {
            get { return informationFieldSizeIsDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.protocolParameters.IFSC = null;
                }
                else
                {
                    this.protocolParameters.IFSC = this.protocolParameters.IFSCValue;
                }
            }
        }

        public byte InformationFieldSize
        {
            get { return informationFieldSizeAdapter.GetValue(this); }
            set
            {
                this.protocolParameters.IFSC = value;
            }
        }
    }
}