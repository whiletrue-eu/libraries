using System;
using System.Collections.Generic;
using System.Globalization;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
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
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, EnumerationAdapter<SpuUse>> spuUseAdapter;
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
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, EnumerationAdapter<VppProgrammingCurrent>> vppProgrammingCurrentAdapter;
        private static readonly ReadOnlyPropertyAdapter<GlobalInterfaceBytesAdapter, int?> etuAdapter;

        static GlobalInterfaceBytesAdapter()
        {
            IPropertyAdapterFactory<GlobalInterfaceBytesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<GlobalInterfaceBytesAdapter>();
            
            GlobalInterfaceBytesAdapter.protocolParameterBytes = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.ProtocolParameterBytes),
                instance => instance.globalInterfaceBytes.ParameterBytes,
                (instance, parameterByte) => ProtocolParameterByteAdapterBase.GetObject(parameterByte)
                );

            GlobalInterfaceBytesAdapter.diAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.Di),
                instance => EnumerationAdapter<Di>.GetInstanceFor(instance.globalInterfaceBytes.DiValue),
                (instance, value) => instance.globalInterfaceBytes.SetDiFiFmax(value, instance.globalInterfaceBytes.FiFmaxValue)
                );
            GlobalInterfaceBytesAdapter.fiFmaxAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.FiFmax),
                instance => EnumerationAdapter<FiFmax>.GetInstanceFor(instance.globalInterfaceBytes.FiFmaxValue),
                (instance, value) => instance.globalInterfaceBytes.SetDiFiFmax(instance.globalInterfaceBytes.DiValue, value)
                );
            GlobalInterfaceBytesAdapter.etuAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.Etu),
                instance => GlobalInterfaceBytesAdapter.CalculateEtu(instance.globalInterfaceBytes.FiFmaxValue, instance.globalInterfaceBytes.DiValue)
                );
            GlobalInterfaceBytesAdapter.diFiIsDefaultAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.DiFiIsDefault),
                instance => instance.globalInterfaceBytes.Di.HasValue == false && instance.globalInterfaceBytes.FiFmax.HasValue == false 
                );
            GlobalInterfaceBytesAdapter.extraGuardTimeAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.ExtraGuardTime),
                instance => instance.globalInterfaceBytes.ExtraGuardTimeValue,
                    (instance,value)=>instance.globalInterfaceBytes.ExtraGuardTime = value
                );
            GlobalInterfaceBytesAdapter.extraGuardTimeIsDefaultAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.ExtraGuardTimeIsDefault),
                instance => instance.globalInterfaceBytes.ExtraGuardTime.HasValue == false
                );

            GlobalInterfaceBytesAdapter.specificModeSupportedAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpecificModeSupported),
                instance => instance.globalInterfaceBytes.CanChangeNegotiableSpecificMode.HasValue ||
                      instance.globalInterfaceBytes.SpecificModeImplicitFiDi.HasValue ||
                      instance.globalInterfaceBytes.SpecificModeProtocol.HasValue
                );
            GlobalInterfaceBytesAdapter.canChangeNegotiableSpecificModeAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.CanChangeNegotiableSpecificMode),
                instance => instance.globalInterfaceBytes.CanChangeNegotiableSpecificMode.HasValue
                          ? instance.globalInterfaceBytes.CanChangeNegotiableSpecificMode.Value
                          : false
                );
            GlobalInterfaceBytesAdapter.specificModeImplicitFiDiAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpecificModeImplicitFiDi),
                instance => instance.globalInterfaceBytes.SpecificModeImplicitFiDi.HasValue
                          ? instance.globalInterfaceBytes.SpecificModeImplicitFiDi.Value
                          : false
                );
            GlobalInterfaceBytesAdapter.specificModeProtocolAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpecificModeProtocol),
                instance => instance.globalInterfaceBytes.SpecificModeProtocol.HasValue
                          ? EnumerationAdapter<ProtocolType>.GetInstanceFor(instance.globalInterfaceBytes.SpecificModeProtocol.Value)
                          : null
                );
            GlobalInterfaceBytesAdapter.clockStopSupportAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.ClockStopSupport),
                instance => EnumerationAdapter<ClockStopSupport>.GetInstanceFor(instance.globalInterfaceBytes.ClockStopSupportValue)
                );
            GlobalInterfaceBytesAdapter.clockStopSupportAndOperatingConditionsIsDefaultAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.ClockStopSupportAndOperatingConditionsIsDefault),
                instance => instance.globalInterfaceBytes.ClockStopSupport.HasValue == false || instance.globalInterfaceBytes.OperatingConditions.HasValue == false
                );
            GlobalInterfaceBytesAdapter.operatingConditionsAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.OperatingConditions),
                instance => EnumerationAdapter<OperatingConditions>.GetInstanceFor(instance.globalInterfaceBytes.OperatingConditionsValue)
                );
            GlobalInterfaceBytesAdapter.spuUseAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpuUse),
                instance => EnumerationAdapter<SpuUse>.GetInstanceFor(instance.globalInterfaceBytes.SpuUseValue)
                );
            GlobalInterfaceBytesAdapter.spuUseIsDefaultAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpuUseIsDefault),
                instance => instance.globalInterfaceBytes.SpuUse.HasValue == false
                );
            GlobalInterfaceBytesAdapter.spuUseIsNotUsedAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpuUseIsNotUsed),
                instance => instance.globalInterfaceBytes.SpuUse==Classes.ATR.SpuUse.NotUsed
                );
            GlobalInterfaceBytesAdapter.spuUseIsStandardAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpuUseIsStandard),
                instance => instance.globalInterfaceBytes.SpuUse == Classes.ATR.SpuUse.Standard 
                );
            GlobalInterfaceBytesAdapter.spuUseIsProprietaryAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpuUseIsProprietary),
                instance => instance.globalInterfaceBytes.SpuUse == Classes.ATR.SpuUse.Proprietary
                );
            GlobalInterfaceBytesAdapter.spuIsInUseAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpuIsInUse),
                instance => instance.globalInterfaceBytes.SpuUseValue != Classes.ATR.SpuUse.NotUsed
                );
            GlobalInterfaceBytesAdapter.spuTypeAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpuType),
                instance => instance.globalInterfaceBytes.SpuType != null ? instance.globalInterfaceBytes.SpuType.Value.ToHexString() : null
                );
            GlobalInterfaceBytesAdapter.spuTypeEtsiCodingAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.SpuTypeEtsiCoding),
                instance => instance.globalInterfaceBytes.SpuType != null && instance.globalInterfaceBytes.SpuType.EtsiCoding != null ? new SpuTypeEtsiCodingAdapter(instance.globalInterfaceBytes, instance.globalInterfaceBytes.SpuType.EtsiCoding) : null
                );

            GlobalInterfaceBytesAdapter.isVppConnectedAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.IsVppConnected),
                instance => instance.globalInterfaceBytes.IsVppConnected
                );
            GlobalInterfaceBytesAdapter.isVppConnectedisDefaultAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.IsVppConnectedIsDefault),
                instance => instance.globalInterfaceBytes.IsVppConnected.HasValue == false
                );
            GlobalInterfaceBytesAdapter.vppProgrammingVoltageAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.VppProgrammingVoltage),
                instance => instance.globalInterfaceBytes.VppProgrammingVoltage!=null?instance.globalInterfaceBytes.VppProgrammingVoltage.Value.ToString("F1"):null
                );
            GlobalInterfaceBytesAdapter.vppProgrammingCurrentAdapter = PropertyFactory.Create(
                nameof(GlobalInterfaceBytesAdapter.VppProgrammingCurrent),
                instance => EnumerationAdapter<VppProgrammingCurrent>.GetInstanceFor(instance.globalInterfaceBytes.VppProgrammingCurrent)
                );            
        }

        public int? Etu => GlobalInterfaceBytesAdapter.etuAdapter.GetValue(this);

        private static int? CalculateEtu(FiFmax fi, Di di)
        {
            int Fi;
            switch(fi)
            {
                case Classes.ATR.FiFmax.Fi372FMax4:
                case Classes.ATR.FiFmax.Fi372FMax5:
                    Fi = 372;
                    break;
                case Classes.ATR.FiFmax.Fi558FMax6:
                    Fi = 558;
                    break;
                case Classes.ATR.FiFmax.Fi744FMax8:
                    Fi = 744;
                    break;
                case Classes.ATR.FiFmax.Fi1116FMax12:
                    Fi = 1116;
                    break;
                case Classes.ATR.FiFmax.Fi1488FMax16:
                    Fi = 1488;
                    break;
                case Classes.ATR.FiFmax.Fi1860FMax20:
                    Fi = 1860;
                    break;
                case Classes.ATR.FiFmax.Fi512FMax5:
                    Fi = 512;
                    break;
                case Classes.ATR.FiFmax.Fi768FMax7P5:
                    Fi = 768;
                    break;
                case Classes.ATR.FiFmax.Fi1024FMax10:
                    Fi = 1024;
                    break;
                case Classes.ATR.FiFmax.Fi1536FMax15:
                    Fi = 1536;
                    break;
                case Classes.ATR.FiFmax.Fi2048FMax20:
                    Fi = 2048;
                    break;
                default:
                    return null;
            }
            int Di;
            switch (di)
            {
                case Classes.ATR.Di.Di1:
                    Di = 1;
                    break;
                case Classes.ATR.Di.Di2:
                    Di = 2;
                    break;
                case Classes.ATR.Di.Di4:
                    Di = 4;
                    break;
                case Classes.ATR.Di.Di8:
                    Di = 8;
                    break;
                case Classes.ATR.Di.Di16:
                    Di = 16;
                    break;
                case Classes.ATR.Di.Di32:
                    Di = 32;
                    break;
                case Classes.ATR.Di.Di64:
                    Di = 64;
                    break;
                case Classes.ATR.Di.Di12:
                    Di = 12;
                    break;
                case Classes.ATR.Di.Di20:
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

        public EnumerationAdapter<VppProgrammingCurrent> VppProgrammingCurrent
        {
            get { return GlobalInterfaceBytesAdapter.vppProgrammingCurrentAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetVpp(value,this.globalInterfaceBytes.VppProgrammingVoltage??0x05);
            }
        }

        public IEnumerable<EnumerationAdapter<VppProgrammingCurrent>> VppProgrammingCurrentValues => EnumerationAdapter<VppProgrammingCurrent>.Items;

        public string VppProgrammingVoltage
        {
            get { return GlobalInterfaceBytesAdapter.vppProgrammingVoltageAdapter.GetValue(this); }
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

                this.globalInterfaceBytes.SetVpp(this.globalInterfaceBytes.VppProgrammingCurrent ?? Classes.ATR.VppProgrammingCurrent.Current25, Value);
            }
        }

        public bool IsVppConnectedIsDefault
        {
            get { return GlobalInterfaceBytesAdapter.isVppConnectedisDefaultAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.globalInterfaceBytes.SetVppToDefault();
                }
                else
                {
                    //Ignore: will set through 'indiicated' property in UI
                }
            }
        }

        public bool? IsVppConnected
        {
            get { return GlobalInterfaceBytesAdapter.isVppConnectedAdapter.GetValue(this); }
            set
            {
                if (value.HasValue && value.Value)
                {
                    this.globalInterfaceBytes.SetVpp(this.globalInterfaceBytes.VppProgrammingCurrent ?? Classes.ATR.VppProgrammingCurrent.Current25, this.globalInterfaceBytes.VppProgrammingVoltage ?? 0x05);
                }
                else
                {
                    this.globalInterfaceBytes.SetVppToNotConnected();
                }
            }
        }

        public bool SpuIsInUse => GlobalInterfaceBytesAdapter.spuIsInUseAdapter.GetValue(this);

        public string SpuType
        {
            get { return GlobalInterfaceBytesAdapter.spuTypeAdapter.GetValue(this); }
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

        public SpuTypeEtsiCodingAdapter SpuTypeEtsiCoding => GlobalInterfaceBytesAdapter.spuTypeEtsiCodingAdapter.GetValue(this);

        public bool SpuUseIsDefault
        {
            get { return GlobalInterfaceBytesAdapter.spuUseIsDefaultAdapter.GetValue(this); }
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
            get { return GlobalInterfaceBytesAdapter.spuUseIsNotUsedAdapter.GetValue(this); }
            set
            {
                if (value && this.globalInterfaceBytes.SpuUse != Classes.ATR.SpuUse.NotUsed)
                {
                    this.globalInterfaceBytes.SetSpu(Classes.ATR.SpuUse.NotUsed, 0x00);
                }
                else
                {
                    //Ignore - for all other options, there is a bool property as well
                }
            }
        }

        public bool SpuUseIsStandard
        {
            get { return GlobalInterfaceBytesAdapter.spuUseIsStandardAdapter.GetValue(this); }
            set
            {
                if (value && this.globalInterfaceBytes.SpuUse != Classes.ATR.SpuUse.Standard)
                {
                    this.globalInterfaceBytes.SetSpu(Classes.ATR.SpuUse.Standard, 0x7F);
                }
                else
                {
                    //Ignore - for all other options, there is a bool property as well
                }
            }
        }

        public bool SpuUseIsProprietary
        {
            get { return GlobalInterfaceBytesAdapter.spuUseIsProprietaryAdapter.GetValue(this); }
            set
            {
                if (value && this.globalInterfaceBytes.SpuUse != Classes.ATR.SpuUse.Proprietary)
                {
                    this.globalInterfaceBytes.SetSpu(Classes.ATR.SpuUse.Proprietary, 0x00);
                }
                else
                {
                    //Ignore - for all other options, there is a bool property as well
                }
            }
        }


        public EnumerationAdapter<SpuUse> SpuUse => GlobalInterfaceBytesAdapter.spuUseAdapter.GetValue(this);

        public bool ClockStopSupportAndOperatingConditionsIsDefault
        {
            get { return GlobalInterfaceBytesAdapter.clockStopSupportAndOperatingConditionsIsDefaultAdapter.GetValue(this); }
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
            get { return GlobalInterfaceBytesAdapter.clockStopSupportAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetClockStopAndOperatingConditions(value, this.globalInterfaceBytes.OperatingConditionsValue);
            }
        }


        public IEnumerable<EnumerationAdapter<ClockStopSupport>> ClockStopSupportValues => EnumerationAdapter<ClockStopSupport>.Items;

        public EnumerationAdapter<OperatingConditions> OperatingConditions
        {
            get { return GlobalInterfaceBytesAdapter.operatingConditionsAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetClockStopAndOperatingConditions(this.globalInterfaceBytes.ClockStopSupportValue, value);
            }
        }

        public IEnumerable<EnumerationAdapter<OperatingConditions>> OperatingConditionsValues => EnumerationAdapter<OperatingConditions>.Items;

        public EnumerationAdapter<ProtocolType> SpecificModeProtocol
        {
            get { return GlobalInterfaceBytesAdapter.specificModeProtocolAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpecificModeSupport(
                    value,
                    this.globalInterfaceBytes.SpecificModeImplicitFiDi??false,
                    this.globalInterfaceBytes.CanChangeNegotiableSpecificMode??false
                    );
            }
        }

        public IEnumerable<EnumerationAdapter<ProtocolType>> SpecificModeProtocolValues => EnumerationAdapter<ProtocolType>.Items;

        public bool SpecificModeImplicitFiDi
        {
            get { return GlobalInterfaceBytesAdapter.specificModeImplicitFiDiAdapter.GetValue(this); }
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
            get { return GlobalInterfaceBytesAdapter.canChangeNegotiableSpecificModeAdapter.GetValue(this); }
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
            get { return GlobalInterfaceBytesAdapter.specificModeSupportedAdapter.GetValue(this); }
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
            get { return GlobalInterfaceBytesAdapter.diFiIsDefaultAdapter.GetValue(this); }
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
            get { return GlobalInterfaceBytesAdapter.extraGuardTimeIsDefaultAdapter.GetValue(this); }
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
            get { return GlobalInterfaceBytesAdapter.extraGuardTimeAdapter.GetValue(this); }
            set { GlobalInterfaceBytesAdapter.extraGuardTimeAdapter.SetValue(this,value); }
        }


        public EnumerationAdapter<Di> Di
        {
            get { return GlobalInterfaceBytesAdapter.diAdapter.GetValue(this); }
            set {  GlobalInterfaceBytesAdapter.diAdapter.SetValue(this,value); }
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
            get { return GlobalInterfaceBytesAdapter.fiFmaxAdapter.GetValue(this); }
            set { GlobalInterfaceBytesAdapter.fiFmaxAdapter.SetValue(this, value); }
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

        public IEnumerable<ProtocolParameterByteAdapterBase> ProtocolParameterBytes => GlobalInterfaceBytesAdapter.protocolParameterBytes.GetCollection(this);
    }
}