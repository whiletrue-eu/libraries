using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Controls.ATRView
{
    public class GlobalInterfaceBytesAdapter : ObservableObject
    {
        private readonly GlobalInterfaceBytes globalInterfaceBytes;
        private static readonly EnumerablePropertyAdapter<GlobalInterfaceBytesAdapter, ParameterByte, ProtocolParameterByteAdapterBase> protocolParameterBytes;
        private static readonly PropertyAdapter<GlobalInterfaceBytesAdapter, EnumerationAdapter<Di>> diAdapter;
        private static readonly PropertyAdapter<GlobalInterfaceBytesAdapter, EnumerationAdapter<FiFmax>> fiFmaxAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> diFiIsDefaultAdapter;
        private static readonly PropertyAdapter<GlobalInterfaceBytesAdapter, byte> extraGuardTimeAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> extraGuardTimeIsDefaultAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> specificModeSupportedAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> canChangeNegotiableSpecificModeAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> specificModeImplicitFiDiAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, EnumerationAdapter<ProtocolType>> specificModeProtocolAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, EnumerationAdapter<ClockStopSupport>> clockStopSupportAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> clockStopSupportAndOperatingConditionsIsDefaultAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, EnumerationAdapter<SPUUse>> spuUseAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> spuUseIsDefaultAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> spuUseIsNotUsedAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> spuUseIsStandardAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> spuUseIsProprietaryAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, string> spuTypeAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, SpuTypeEtsiCodingAdapter> spuTypeEtsiCodingAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, EnumerationAdapter<OperatingConditions>> operatingConditionsAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> spuIsInUseAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool?> isVppConnectedAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, bool> isVppConnectedisDefaultAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, string> vppProgrammingVoltageAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, EnumerationAdapter<VPPProgrammingCurrent>> vppProgrammingCurrentAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, int?> etuAdapter;

        static GlobalInterfaceBytesAdapter()
        {
            IPropertyAdapterFactory<GlobalInterfaceBytesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<GlobalInterfaceBytesAdapter>();
            
            protocolParameterBytes = PropertyFactory.Create(
                @this => @this.ProtocolParameterBytes,
                @this => @this.globalInterfaceBytes.ParameterBytes,
                (@this, parameterByte) => ProtocolParameterByteAdapterBase.GetObject(parameterByte)
                );

            diAdapter = PropertyFactory.Create(
                @this => @this.Di,
                @this => EnumerationAdapter<Di>.GetInstanceFor(@this.globalInterfaceBytes.DiValue),
                (@this, value) => @this.globalInterfaceBytes.SetDiFiFmax(value, @this.globalInterfaceBytes.FiFmaxValue)
                );
            fiFmaxAdapter = PropertyFactory.Create(
                @this => @this.FiFmax,
                @this => EnumerationAdapter<FiFmax>.GetInstanceFor(@this.globalInterfaceBytes.FiFmaxValue),
                (@this, value) => @this.globalInterfaceBytes.SetDiFiFmax(@this.globalInterfaceBytes.DiValue, value)
                );
            etuAdapter = PropertyFactory.Create(
                @this => @this.Etu,
                @this => CalculateEtu(@this.globalInterfaceBytes.FiFmaxValue, @this.globalInterfaceBytes.DiValue)
                );
            diFiIsDefaultAdapter = PropertyFactory.Create(
                @this => @this.DiFiIsDefault,
                @this => @this.globalInterfaceBytes.Di.HasValue == false && @this.globalInterfaceBytes.FiFmax.HasValue == false 
                );
            extraGuardTimeAdapter = PropertyFactory.Create(
                @this => @this.ExtraGuardTime,
                @this => @this.globalInterfaceBytes.ExtraGuardTimeValue,
                    (@this,value)=>@this.globalInterfaceBytes.ExtraGuardTime = value
                );
            extraGuardTimeIsDefaultAdapter = PropertyFactory.Create(
                @this => @this.ExtraGuardTimeIsDefault,
                @this => @this.globalInterfaceBytes.ExtraGuardTime.HasValue == false
                );

            specificModeSupportedAdapter = PropertyFactory.Create(
                @this => @this.SpecificModeSupported,
                @this => @this.globalInterfaceBytes.CanChangeNegotiableSpecificMode.HasValue ||
                      @this.globalInterfaceBytes.SpecificModeImplicitFiDi.HasValue ||
                      @this.globalInterfaceBytes.SpecificModeProtocol.HasValue
                );
            canChangeNegotiableSpecificModeAdapter = PropertyFactory.Create(
                @this => @this.CanChangeNegotiableSpecificMode,
                @this => @this.globalInterfaceBytes.CanChangeNegotiableSpecificMode.HasValue
                          ? @this.globalInterfaceBytes.CanChangeNegotiableSpecificMode.Value
                          : false
                );
            specificModeImplicitFiDiAdapter = PropertyFactory.Create(
                @this => @this.SpecificModeImplicitFiDi,
                @this => @this.globalInterfaceBytes.SpecificModeImplicitFiDi.HasValue
                          ? @this.globalInterfaceBytes.SpecificModeImplicitFiDi.Value
                          : false
                );
            specificModeProtocolAdapter = PropertyFactory.Create(
                @this => @this.SpecificModeProtocol,
                @this => @this.globalInterfaceBytes.SpecificModeProtocol.HasValue
                          ? EnumerationAdapter<ProtocolType>.GetInstanceFor(@this.globalInterfaceBytes.SpecificModeProtocol.Value)
                          : null
                );
            clockStopSupportAdapter = PropertyFactory.Create(
                @this => @this.ClockStopSupport,
                @this => EnumerationAdapter<ClockStopSupport>.GetInstanceFor(@this.globalInterfaceBytes.ClockStopSupportValue)
                );
            clockStopSupportAndOperatingConditionsIsDefaultAdapter = PropertyFactory.Create(
                @this => @this.ClockStopSupportAndOperatingConditionsIsDefault,
                @this => @this.globalInterfaceBytes.ClockStopSupport.HasValue == false || @this.globalInterfaceBytes.OperatingConditions.HasValue == false
                );
            operatingConditionsAdapter = PropertyFactory.Create(
                @this => @this.OperatingConditions,
                @this => EnumerationAdapter<OperatingConditions>.GetInstanceFor(@this.globalInterfaceBytes.OperatingConditionsValue)
                );
            spuUseAdapter = PropertyFactory.Create(
                @this => @this.SpuUse,
                @this => EnumerationAdapter<SPUUse>.GetInstanceFor(@this.globalInterfaceBytes.SpuUseValue)
                );
            spuUseIsDefaultAdapter = PropertyFactory.Create(
                @this => @this.SpuUseIsDefault,
                @this => @this.globalInterfaceBytes.SpuUse.HasValue == false
                );
            spuUseIsNotUsedAdapter = PropertyFactory.Create(
                @this => @this.SpuUseIsNotUsed,
                @this => @this.globalInterfaceBytes.SpuUse==SPUUse.NotUsed
                );
            spuUseIsStandardAdapter = PropertyFactory.Create(
                @this => @this.SpuUseIsStandard,
                @this => @this.globalInterfaceBytes.SpuUse == SPUUse.Standard 
                );
            spuUseIsProprietaryAdapter = PropertyFactory.Create(
                @this => @this.SpuUseIsProprietary,
                @this => @this.globalInterfaceBytes.SpuUse == SPUUse.Proprietary
                );
            spuIsInUseAdapter = PropertyFactory.Create(
                @this => @this.SpuIsInUse,
                @this => @this.globalInterfaceBytes.SpuUseValue != SPUUse.NotUsed
                );
            spuTypeAdapter = PropertyFactory.Create(
                @this => @this.SpuType,
                @this => @this.globalInterfaceBytes.SpuType != null ? @this.globalInterfaceBytes.SpuType.Value.ToHexString() : null
                );
            spuTypeEtsiCodingAdapter = PropertyFactory.Create(
                @this => @this.SpuTypeEtsiCoding,
                @this => @this.globalInterfaceBytes.SpuType != null && @this.globalInterfaceBytes.SpuType.EtsiCoding != null ? new SpuTypeEtsiCodingAdapter(@this.globalInterfaceBytes, @this.globalInterfaceBytes.SpuType.EtsiCoding) : null
                );

            isVppConnectedAdapter = PropertyFactory.Create(
                @this => @this.IsVppConnected,
                @this => @this.globalInterfaceBytes.IsVPPConnected
                );
            isVppConnectedisDefaultAdapter = PropertyFactory.Create(
                @this => @this.IsVppConnectedIsDefault,
                @this => @this.globalInterfaceBytes.IsVPPConnected.HasValue == false
                );
            vppProgrammingVoltageAdapter = PropertyFactory.Create(
                @this => @this.VppProgrammingVoltage,
                @this => @this.globalInterfaceBytes.VPPProgrammingVoltage!=null?@this.globalInterfaceBytes.VPPProgrammingVoltage.Value.ToString("F1"):null
                );
            vppProgrammingCurrentAdapter = PropertyFactory.Create(
                @this => @this.VppProgrammingCurrent,
                @this => EnumerationAdapter<VPPProgrammingCurrent>.GetInstanceFor(@this.globalInterfaceBytes.VPPProgrammingCurrent)
                );            
        }

        public int? Etu
        {
            get { return etuAdapter.GetValue(this); }
        }

        private static int? CalculateEtu(FiFmax fi, Di di)
        {
            int Fi;
            switch(fi)
            {
                case Types.SmartCard.FiFmax.Fi372_fMax4:
                case Types.SmartCard.FiFmax.Fi372_fMax5:
                    Fi = 372;
                    break;
                case Types.SmartCard.FiFmax.Fi558_fMax6:
                    Fi = 558;
                    break;
                case Types.SmartCard.FiFmax.Fi744_fMax8:
                    Fi = 744;
                    break;
                case Types.SmartCard.FiFmax.Fi1116_fMax12:
                    Fi = 1116;
                    break;
                case Types.SmartCard.FiFmax.Fi1488_fMax16:
                    Fi = 1488;
                    break;
                case Types.SmartCard.FiFmax.Fi1860_fMax20:
                    Fi = 1860;
                    break;
                case Types.SmartCard.FiFmax.Fi512_fMax5:
                    Fi = 512;
                    break;
                case Types.SmartCard.FiFmax.Fi768_fMax7p5:
                    Fi = 768;
                    break;
                case Types.SmartCard.FiFmax.Fi1024_fMax10:
                    Fi = 1024;
                    break;
                case Types.SmartCard.FiFmax.Fi1536_fMax15:
                    Fi = 1536;
                    break;
                case Types.SmartCard.FiFmax.Fi2048_fMax20:
                    Fi = 2048;
                    break;
                default:
                    return null;
            }
            int Di;
            switch (di)
            {
                case Types.SmartCard.Di.Di1:
                    Di = 1;
                    break;
                case Types.SmartCard.Di.Di2:
                    Di = 2;
                    break;
                case Types.SmartCard.Di.Di4:
                    Di = 4;
                    break;
                case Types.SmartCard.Di.Di8:
                    Di = 8;
                    break;
                case Types.SmartCard.Di.Di16:
                    Di = 16;
                    break;
                case Types.SmartCard.Di.Di32:
                    Di = 32;
                    break;
                case Types.SmartCard.Di.Di64:
                    Di = 64;
                    break;
                case Types.SmartCard.Di.Di12:
                    Di = 12;
                    break;
                case Types.SmartCard.Di.Di20:
                    Di = 20;
                    break;
                default:
                    return null;
            }

            return Fi/Di;
        }

        public GlobalInterfaceBytesAdapter(GlobalInterfaceBytes globalInterfaceBytes)
        {
            this.globalInterfaceBytes = globalInterfaceBytes;
        }

        public EnumerationAdapter<VPPProgrammingCurrent> VppProgrammingCurrent
        {
            get { return vppProgrammingCurrentAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetVPP(value,this.globalInterfaceBytes.VPPProgrammingVoltage??0x05);
            }
        }

        public IEnumerable<EnumerationAdapter<VPPProgrammingCurrent>> VppProgrammingCurrentValues
        {
            get { return EnumerationAdapter<VPPProgrammingCurrent>.Items; }
        }

        public string VppProgrammingVoltage
        {
            get { return vppProgrammingVoltageAdapter.GetValue(this); }
            set
            {
                //value is always in format ###.#! if it is only ###, the point was deleted, so we can add it before the last digit to get a consistent editor
                if (value == "")
                {
                    value = "0";
                }
                if (value.EndsWith(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator))
                {
                    value = value + "0";
                }
                //Filter out double decimal points
                if (value.Contains(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator))
                {
                    value = value.Substring(0, value.IndexOf(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator)) +
                            NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator +
                            value.Substring(value.LastIndexOf(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator) + 1);
                }
                double Value = double.Parse(value);

                this.globalInterfaceBytes.SetVPP(this.globalInterfaceBytes.VPPProgrammingCurrent ?? VPPProgrammingCurrent.Current25, Value);
            }
        }

        public bool IsVppConnectedIsDefault
        {
            get { return isVppConnectedisDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.globalInterfaceBytes.SetVPPToDefault();
                }
                else
                {
                    //Ignore: will set through 'indiicated' property in UI
                }
            }
        }

        public bool? IsVppConnected
        {
            get { return isVppConnectedAdapter.GetValue(this); }
            set
            {
                if (value.HasValue && value.Value)
                {
                    this.globalInterfaceBytes.SetVPP(this.globalInterfaceBytes.VPPProgrammingCurrent ?? VPPProgrammingCurrent.Current25, this.globalInterfaceBytes.VPPProgrammingVoltage ?? 0x05);
                }
                else
                {
                    this.globalInterfaceBytes.SetVPPToNotConnected();
                }
            }
        }

        public bool SpuIsInUse
        {
            get { return spuIsInUseAdapter.GetValue(this); }
        }

        public string SpuType
        {
            get { return spuTypeAdapter.GetValue(this); }
            set
            {
                if (value != null)
                {
                    byte Value;
                    if (byte.TryParse(value, NumberStyles.HexNumber, null, out Value) == false)
                    {
                        throw new ArgumentException("must be a hex value");
                    }

                    if ((Value & 0x80) == 0x80)
                    {
                        throw new ArgumentException("only 7 bit value is allowed");
                    }
                    
                    this.globalInterfaceBytes.SetSpu(this.globalInterfaceBytes.SpuUseValue, Value);
                }
                else
                {
                    this.globalInterfaceBytes.SetSpuToDefault();
                }
            }
        }

        public SpuTypeEtsiCodingAdapter SpuTypeEtsiCoding
        {
            get { return spuTypeEtsiCodingAdapter.GetValue(this); }
        }

        public bool SpuUseIsDefault
        {
            get { return spuUseIsDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.globalInterfaceBytes.SetSpuToDefault();
                }
                else
                {
                    //Ignore - for all other options, there is a bool property as well
                }
            }
        }

        public bool SpuUseIsNotUsed
        {
            get { return spuUseIsNotUsedAdapter.GetValue(this); }
            set
            {
                if (value && this.globalInterfaceBytes.SpuUse != SPUUse.NotUsed)
                {
                    this.globalInterfaceBytes.SetSpu(SPUUse.NotUsed, 0x00);
                }
                else
                {
                    //Ignore - for all other options, there is a bool property as well
                }
            }
        }

        public bool SpuUseIsStandard
        {
            get { return spuUseIsStandardAdapter.GetValue(this); }
            set
            {
                if (value && this.globalInterfaceBytes.SpuUse != SPUUse.Standard)
                {
                    this.globalInterfaceBytes.SetSpu(SPUUse.Standard, 0x7F);
                }
                else
                {
                    //Ignore - for all other options, there is a bool property as well
                }
            }
        }

        public bool SpuUseIsProprietary
        {
            get { return spuUseIsProprietaryAdapter.GetValue(this); }
            set
            {
                if (value && this.globalInterfaceBytes.SpuUse != SPUUse.Proprietary)
                {
                    this.globalInterfaceBytes.SetSpu(SPUUse.Proprietary, 0x00);
                }
                else
                {
                    //Ignore - for all other options, there is a bool property as well
                }
            }
        }


        public EnumerationAdapter<SPUUse> SpuUse
        {
            get { return spuUseAdapter.GetValue(this); }
        }

        public bool ClockStopSupportAndOperatingConditionsIsDefault
        {
            get { return clockStopSupportAndOperatingConditionsIsDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.globalInterfaceBytes.SetClockStopAndOperatingConditionsToDefault();
                }
                else
                {
                    this.globalInterfaceBytes.SetClockStopAndOperatingConditions(this.globalInterfaceBytes.ClockStopSupportValue, this.globalInterfaceBytes.OperatingConditionsValue);
                }
            }

        }

        public EnumerationAdapter<ClockStopSupport> ClockStopSupport
        {
            get { return clockStopSupportAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetClockStopAndOperatingConditions(value, this.globalInterfaceBytes.OperatingConditionsValue);
            }
        }


        public IEnumerable<EnumerationAdapter<ClockStopSupport>> ClockStopSupportValues
        {
            get
            {
                return EnumerationAdapter<ClockStopSupport>.Items;
            }
        }

        public EnumerationAdapter<OperatingConditions> OperatingConditions
        {
            get { return operatingConditionsAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetClockStopAndOperatingConditions(this.globalInterfaceBytes.ClockStopSupportValue, value);
            }
        }

        public IEnumerable<EnumerationAdapter<OperatingConditions>> OperatingConditionsValues
        {
            get
            {
                return EnumerationAdapter<OperatingConditions>.Items;
            }
        }

        public EnumerationAdapter<ProtocolType> SpecificModeProtocol
        {
            get { return specificModeProtocolAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpecificModeSupport(
                    value,
                    this.globalInterfaceBytes.SpecificModeImplicitFiDi??false,
                    this.globalInterfaceBytes.CanChangeNegotiableSpecificMode??false
                    );
            }
        }

        public IEnumerable<EnumerationAdapter<ProtocolType>> SpecificModeProtocolValues
        {
            get
            {
                return EnumerationAdapter<ProtocolType>.Items;
            }
        }

        public bool SpecificModeImplicitFiDi
        {
            get { return specificModeImplicitFiDiAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpecificModeSupport(
                    this.globalInterfaceBytes.SpecificModeProtocol??ProtocolType.T0,
                    value,
                    this.globalInterfaceBytes.CanChangeNegotiableSpecificMode ?? false
                    );
            }
        }

        public bool CanChangeNegotiableSpecificMode
        {
            get { return canChangeNegotiableSpecificModeAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpecificModeSupport(
                    this.globalInterfaceBytes.SpecificModeProtocol ?? ProtocolType.T0,
                    this.globalInterfaceBytes.SpecificModeImplicitFiDi ?? false,
                    value
                    );
            }
        }

        public bool SpecificModeSupported
        {
            get { return specificModeSupportedAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.globalInterfaceBytes.SetSpecificModeSupport(ProtocolType.T0, false, false);
                }
                else
                {
                    this.globalInterfaceBytes.SetSpecificModeUnsupported();
                }
            }
        }


        public bool DiFiIsDefault
        {
            get { return diFiIsDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.globalInterfaceBytes.SetDiFiFmax(null,null);
                }
                else
                {
                    this.globalInterfaceBytes.SetDiFiFmax(this.globalInterfaceBytes.DiValue,this.globalInterfaceBytes.FiFmaxValue);
                }
            }
        }
        public bool ExtraGuardTimeIsDefault
        {
            get { return extraGuardTimeIsDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.globalInterfaceBytes.ExtraGuardTime = null;
                }
                else
                {
                    this.globalInterfaceBytes.ExtraGuardTime = this.globalInterfaceBytes.ExtraGuardTimeValue;
                }

            }
        }

        public byte ExtraGuardTime
        {
            get { return extraGuardTimeAdapter.GetValue(this); }
            set { extraGuardTimeAdapter.SetValue(this,value); }
        }


        public EnumerationAdapter<Di> Di
        {
            get { return diAdapter.GetValue(this); }
            set {  diAdapter.SetValue(this,value); }
        }

        public IEnumerable<EnumerationAdapter<Di>> PossibleDiValues
        {
            get
            {
                foreach (Di Value in Enum.GetValues(typeof (Di)))
                {
                    yield return EnumerationAdapter<Di>.GetInstanceFor(Value);
                }
            }
        }

        public EnumerationAdapter<FiFmax> FiFmax
        {
            get { return fiFmaxAdapter.GetValue(this); }
            set { fiFmaxAdapter.SetValue(this, value); }
        }


        public IEnumerable<EnumerationAdapter<FiFmax>> PossibleFiFmaxValues
        {
            get
            {
                foreach (FiFmax Value in Enum.GetValues(typeof(FiFmax)))
                {
                    yield return EnumerationAdapter<FiFmax>.GetInstanceFor(Value);
                }
            }
        }

        public IEnumerable<ProtocolParameterByteAdapterBase> ProtocolParameterBytes
        {
            get { return protocolParameterBytes.GetCollection(this); }
        }
    }
}