// using System.Collections.Generic;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class GlobalInterfaceBytes : AtrParametersBase
    {
        public GlobalInterfaceBytes(Atr owner)
            : base(owner, _ => _.Type == InterfaceByteGroupType.Global ||
                               _.Type == InterfaceByteGroupType.GlobalExtended ||
                               _.Number == 2, InterfaceByteGroupType.GlobalExtended) //Special handling for T?2 group, as it may contain global characters Ta2 and Tb2 and Tc2 is dedicated to T=0

        {
        }

        public override IEnumerable<ParameterByte> ParameterBytes
        {
            get
            {
                foreach (AtrInterfaceByteGroupToken InterfaceByteGroup in this.GetInterfaceGroups())
                {
                    yield return InterfaceByteGroup.Ta;
                    yield return InterfaceByteGroup.Tb;
                    if (InterfaceByteGroup.Number == 2)
                    {
                        // Tc2 is specific to T0
                        yield return new ParameterByte(ParameterByte.ValueIndicator.Irrelevant);
                    }
                    else
                    {
                        yield return InterfaceByteGroup.Tc;
                    }
                }
            }
        }

        public Di? Di
        {
            get
            {
                //    Interface byte TA1, Low Nibble
                //    Interface byte TA1, if present, is global. It encodes the maximum clock frequency fmax supported by the card, and the number of clock periods
                //    per ETU that it suggests to use after the ATR, expressed as the ratio Fi/Di of two integers.
                //
                //    The 4 low-order bits of TA1 encode Di as:
                //    4th to 1st bits | 0000 | 0001 | 0010 | 0011 | 0100 | 0101 | 0110 | 0111 | 1000 | 1001 | 1010 | 1011 | 1100 | 1101 | 1110 | 1111
                //    Di              | RFU  | 1    | 2    | 4    | 8    | 16   | 32   | 64*  | 12   | 20   | RFU  | RFU  | RFU  | RFU  | RFU  | RFU  
                //
                //    (*) This was RFU in ISO/IEC 7816-3:1997 and former. Some card readers or drivers may erroneously reject cards using this value (or other RFU).

                byte? InterfaceByte = this.GetInterfaceByte(0, InterfaceByteType.Ta);
                return InterfaceByte.HasValue ? (Di)InterfaceByte.Value.GetLoNibble() : (Di?) null;
            }
        }

        public void SetDiFiFmax(Di? di, FiFmax? fiFmax)
        {
            DbC.Assure((di.HasValue && fiFmax.HasValue) ||
                   (di.HasValue == false && fiFmax.HasValue == false),"di and fi/fmax must be either both defined or undefined");

            if (di.HasValue && fiFmax.HasValue)
            {
                this.SetInterfaceByte(0, InterfaceByteType.Ta, CodingUtils.NibbleToByte((byte)fiFmax, (byte)di));
            }
            else
            {
                this.SetInterfaceByte(0, InterfaceByteType.Ta, null);
            }
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.Di));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.DiValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.FiFmax));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.FiFmaxValue));
        }

        public Di DiValue => this.Di ?? ATR.Di.Di1;

        public FiFmax? FiFmax
        {
            get
            {
                //    Interface byte TA1, High Nibble
                //    The 4 high-order bits of TA1 encode fmax and Fi as:
                //    8th to 5th bits | 0000 | 0001 | 0010 | 0011 | 0100 | 0101 | 0110 | 0111 | 1000 | 1001 | 1010 | 1011 | 1100 | 1101 | 1110 | 1111
                //    Fi              | 372* | 372  | 558  | 744  | 1116 | 1488 | 1860 | RFU  | RFU  | 512  | 768  | 1024 | 1536 | 2048 | RFU  | RFU
                //    fmax (MHz)      | 4*   | 5    | 6    | 8    | 12   | 16   | 20   | -    | -    | 5    | 7.5  | 10   | 15   | 20   | -    | -
                //    (*) Historical note: in ISO/IEC 7816-3:1989, this was assigned to cards with internal clock, and thus no assigned Fi or f(max).
                //
                //    For example, TA1 = 'B5' = 10110101 encodes fmax = 10 MHz, Fi/Di = 1024/16 = 64; this is inviting the card reader to take (after the ATR) the 
                //    necessary steps to reduce the ETU to 64 clock cycles per ETU (from 372 during ATR) and increase the clock frequency up to 10 MHz (from perhaps 
                //    4 MHz during ATR).

                byte? InterfaceByte = this.GetInterfaceByte(0, InterfaceByteType.Ta);
                return InterfaceByte.HasValue ? (FiFmax)InterfaceByte.Value.GetHiNibble() : (FiFmax?) null;
            }
        }

        public FiFmax FiFmaxValue => this.FiFmax ?? ATR.FiFmax.Fi372FMax5;

        //if( TB1Exists )
        //{
        //    Interface byte TB1
        //        TB1, if present, is global. It used to indicate the programming voltage VPP and maximum programming current required by some cards on the dedicated
        //        contact C6 during programming of their EPROM memory. Modern Smart Cards internally generate the programming voltage for their EEPROM or Flash memory,
        //        and the usage of TB1 is deprecated since the 2006 edition of the standard. Nowadays, cards should not include it in the ATR, and readers shall
        //        ignore TB1 if present. Including TB1='00' (indicating that the card does not use VPP) remains common.
        //
        //        In the 1997 and earlier editions of the standard:
        //        - The low 5 bits of TB1 (5th to 1st) encode PI1; if TB2 is absent, PI1=0 indicates that the C6 contact (assigned to VPP) is not connected in the
        //          card; PI1 in range [5..25] encodes the value of VPP in Volt (the reader shall apply that voltage only on specific demand by the card, with a 
        //          tolerance of 2.5%, up to the maximum programming current; and otherwise leave the C6 contact used for VPP within 5% of the VCC voltage, up to 
        //          20 mA); if TB2 is present, it supersedes the indication given by TB1 in the PI1 field, regarding VPP connection or voltage.
        //        - The high bit of TB1 (8th bits) is reserved, shall be 0, and can be ignored by the reader.
        //        - The 6th and 5th bits of TB1 encode the maximum programming current (assuming neither TB1 nor TB2 indicate that VPP is not connected in the card)
        //
        //        7th and 6th bits            | 00    | 01    | 10   | 11
        //        Maximum programming current | 25 mA | 50 mA | RFU* | RFU
        //        (*) This was 100 mA in ISO/IEC 7816-3:1989.

        public bool? IsVppConnected
        {
            get
            {
                 byte? InterfaceByte = this.GetInterfaceByte(0, InterfaceByteType.Tb);
                return InterfaceByte.HasValue ? InterfaceByte.Value != 0x00 : (bool?)null;
            }
        }

        [UsedImplicitly]
        public bool IsVppConnectedValue => this.IsVppConnected ?? false;

        public double? VppProgrammingVoltage
        {
            get
            {
                byte? Pi2Byte = this.GetInterfaceByte(1, InterfaceByteType.Tb);
                if (Pi2Byte.HasValue)
                {
                    return Pi2Byte.Value*0.1d;
                }
                else
                {
                    byte? Pi1Byte = this.GetInterfaceByte(0, InterfaceByteType.Tb);
                    return Pi1Byte.HasValue && Pi1Byte.Value != 0x00 ? (Pi1Byte.Value & 0x1F) : (double?) null;
                }
            }
        }

        public void SetVppToDefault()
        {
            this.SetInterfaceByte(0, InterfaceByteType.Tb, null);
            this.SetInterfaceByte(1, InterfaceByteType.Tb, null);

            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.VppProgrammingVoltage));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.IsVppConnected));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.IsVppConnectedValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.VppProgrammingCurrent));
        }

        public void SetVppToNotConnected()
        {
            this.SetInterfaceByte(0, InterfaceByteType.Tb, 0x00);
            this.SetInterfaceByte(1, InterfaceByteType.Tb, null);

            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.VppProgrammingVoltage));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.IsVppConnected));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.IsVppConnectedValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.VppProgrammingCurrent));
        }

        public void SetVpp(VppProgrammingCurrent current, double voltage)
        {
            int Value = (int) (voltage*10d);
            if (((Value%10) == 0) ? Value > 310 : Value > 255)
            {
                throw new ArgumentException("value is too big. max value is 31 with no fractions, or 25.5 with one fractional digit");
            }
            if (Value == 0)
            {
                throw new ArgumentException("value is too small. Value must be larger than zero");
            }
            if (Value % 10 == 0)
            {
                //can be coded in tb1
                this.SetInterfaceByte(0, InterfaceByteType.Tb, (byte)(0x80 | (byte)current | (byte)(Value/10)));
                this.SetInterfaceByte(1, InterfaceByteType.Tb, null);
            }
            else
            {
                //must be coded in tb2
                this.SetInterfaceByte(0, InterfaceByteType.Tb, (byte)(0x80 | (byte)current));
                this.SetInterfaceByte(1, InterfaceByteType.Tb, (byte)Value);
            }

            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.VppProgrammingVoltage));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.IsVppConnected));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.IsVppConnectedValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.VppProgrammingCurrent));
        }

        public VppProgrammingCurrent? VppProgrammingCurrent
        {
            get
            {
                byte? Pi1Byte = this.GetInterfaceByte(0, InterfaceByteType.Tb);
                return Pi1Byte.HasValue && Pi1Byte.Value != 0x00 ? (VppProgrammingCurrent?)(Pi1Byte.Value & 0x60) : null;
            }
        }

        // TC1 encodes the extra guard time integer (N) from 0 to 255 over the eight bits. The default value is N = 0.
        // - If N = 0 to 254, then before being ready to receive the next character, the card requires the following delay
        //   from the leading edge of the previous character (transmitted by the card or the interface device).
        //                       N
        //    GT = 12 etu + R × ---
        //                       f
        //   * If T=15 is absent in the Answer-to-Reset, then R = F / D, i.e., the integers used for computing the etu.
        //   * If T=15 is present in the Answer-to-Reset, then R = Fi / Di, i.e., the integers defined above by TA1.
        //   No extra guard time is used to transmit characters from the card: GT = 12 etu.
        // - The use of N = 255 is protocol dependent: GT = 12 etu in PPS (see 9) and in T=0 (see 10). For the use of
        //   N = 255 in T=1, see 11.2
        public byte? ExtraGuardTime
        {
            get
            {
                //    Interface byte TC1
                //        TC1 encodes the extra guard time integer (N) from 0 to 255 over the eight bits. The default value is N = 0.
                return this.GetInterfaceByte(0, InterfaceByteType.Tc);
            }
            set
            {
                this.SetInterfaceByte(0, InterfaceByteType.Tc, value);
                this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ExtraGuardTime));
                this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ExtraGuardTimeValue));
            }
        }

        public byte ExtraGuardTimeValue => this.ExtraGuardTime ?? 0;

        // TA2 is the specific mode byte as shown in Figure 15. For the use of TA2, see 6.3.1 and 7.1.
        // - Bit 8 indicates the ability for changing the negotiable/specific mode:
        //   * capable to change if bit 8 is set to 0;
        //   * unable to change if bit 8 is set to 1.
        // - Bits 7 and 6 are reserved for future use (set to 0 when not used).
        // - Bit 5 indicates the definition of the parameters F and D.
        //   * If bit 5 is set to 0, then the integers Fi and Di defined above by TA1 shall apply.
        //   * If bit 5 is set to 1, then implicit values (not defined by the interface bytes) shall apply.
        // - Bits 4 to 1 encode a type T.
        // | Bit 8 | Bit 7 | Bit 6 | Bit 5 | Bit 4 | Bit 3 | Bit 2 | Bit 1 |
        // |msb    |       |       |       |   <---------- T ---------->lsb|
        public bool? CanChangeNegotiableSpecificMode
        {
            get
            {
                byte? Ta2 = this.GetInterfaceByte(1, InterfaceByteType.Ta);
                return Ta2.HasValue ? (Ta2 & 0x80) == 0x00 : (bool?) null;
            }
        }

        public bool? SpecificModeImplicitFiDi
        {
            get
            {
                byte? Ta2 = this.GetInterfaceByte(1, InterfaceByteType.Ta);
                return Ta2.HasValue ? (Ta2 & 0x10) == 0x10 : (bool?) null;
            }
        }

        public ProtocolType? SpecificModeProtocol
        {
            get
            {
                byte? Ta2 = this.GetInterfaceByte(1, InterfaceByteType.Ta);
                return Ta2.HasValue ? (ProtocolType) (Ta2 & 0x0F) : (ProtocolType?) null;
            }
        }


        public void SetSpecificModeUnsupported()
        {
            this.SetInterfaceByte(1,InterfaceByteType.Ta, null);
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpecificModeImplicitFiDi));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpecificModeProtocol));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.CanChangeNegotiableSpecificMode));
        }

        public void SetSpecificModeSupport(ProtocolType protocol, bool implicitFiDi, bool canChangeNegotiableSpecificMode)
        {
            this.SetInterfaceByte(1,InterfaceByteType.Ta, (byte)((canChangeNegotiableSpecificMode?0x00:0x80)|(implicitFiDi?0x10:0x00)|((byte)protocol)));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpecificModeImplicitFiDi));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpecificModeProtocol));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.CanChangeNegotiableSpecificMode));
        }


        // The first TA for T=15 encodes the clock stop indicator (X) and the class indicator (Y). The default values are
        // X = “clock stop not supported” and Y = “only class A supported”. For the use of clock stop, see 6.3.2. For the
        // use of the classes of operating conditions, see 6.2.1 and 6.2.4.
        // - According to Table 9, bits 8 and 7 indicate whether the card supports clock stop (≠ 00) or not (= 00) and,
        //   when supported, which state is preferred on the electrical circuit CLK when the clock is stopped.
        // Table 9 - X
        // | Bits 8 and 7 |             00           |    01   |    10   |       11      |
        // |      X       | Clock stop not supported | State L | State H | no preference |
        //
        // - According to Table 10, bits 6 to 1 indicate the classes of operating conditions accepted by the card. Each
        //   bit represents a class: bit 1 for class A, bit 2 for class B and bit 3 for class C (see 5.1.3).
        // Table 10 — Y
        // | Bits 6 to 1 | 00 0001 | 00 0010 | 00 0100 | 00 0011 | 00 0110 |   00 0111  | Any other value |
        // |      Y      |  A only |  B only |  C only | A and B | B and C | A, B and C |        RFU      |

        public ClockStopSupport? ClockStopSupport
        {
            get
            {
                byte? Ta2 = this.GetInterfaceByte(InterfaceByteGroupType.GlobalExtended, 0, InterfaceByteType.Ta);
                return Ta2.HasValue ? (ClockStopSupport) (Ta2 & 0xC0) : (ClockStopSupport?) null;
            }
        }

        public ClockStopSupport ClockStopSupportValue => this.ClockStopSupport ?? ATR.ClockStopSupport.NotSupported;

        public void SetClockStopAndOperatingConditionsToDefault()
        {
            this.SetInterfaceByte(InterfaceByteGroupType.GlobalExtended, 0, InterfaceByteType.Ta, null);
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ClockStopSupport));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ClockStopSupportValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.OperatingConditions));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.OperatingConditionsValue));
        }
        public void SetClockStopAndOperatingConditions(ClockStopSupport clockStopSupport, OperatingConditions operatingConditions)
        {
            this.SetInterfaceByte(InterfaceByteGroupType.GlobalExtended, 0, InterfaceByteType.Ta, (byte)((((byte)clockStopSupport) & 0xC0)|((byte)operatingConditions) & 0x3F));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ClockStopSupport));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ClockStopSupportValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.OperatingConditions));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.OperatingConditionsValue));
        }

        public OperatingConditions? OperatingConditions
        {
            get
            {
                byte? Ta = this.GetInterfaceByte(InterfaceByteGroupType.GlobalExtended, 0, InterfaceByteType.Ta);
                return Ta.HasValue ? (OperatingConditions?) (Ta.Value & 0x3F) : null;
            }
        }

        public OperatingConditions OperatingConditionsValue => this.OperatingConditions ?? ATR.OperatingConditions.AOnly;

        // The first TB for T=15 indicates the use of SPU by the card (see 5.2.4). The default value is “SPU not used”.
        // Coded over bits 7 to 1, the use is either standard (bit 8 set to 0), or proprietary (bit 8 set to 1). The value '00'
        // indicates that the card does not use SPU. ISO/IEC JTC 1/SC 17 reserves for future use any other value where
        // bit 8 is set to 0.
        public SpuUse? SpuUse
        {
            get
            {
                byte? Tb = this.GetInterfaceByte(InterfaceByteGroupType.GlobalExtended, 0, InterfaceByteType.Tb);
                return Tb.HasValue
                           ? Tb.Value == 0x00
                                 ? ATR.SpuUse.NotUsed
                                 : (ClockStopSupport) (Tb & 0x80) == 0x00
                                       ? ATR.SpuUse.Standard
                                       : ATR.SpuUse.Proprietary
                           : (SpuUse?) null;
            }
        }

        public SpuUse SpuUseValue => this.SpuUse ?? ATR.SpuUse.NotUsed;

        public SpuType SpuType
        {
            get
            {
                byte? Tb = this.GetInterfaceByte(InterfaceByteGroupType.GlobalExtended, 0, InterfaceByteType.Tb);
                return Tb.HasValue && Tb.Value != 0x00 /*not used*/
                           ? new SpuType(Tb.Value)
                           : null;
            }
        }

        public void SetSpu(SpuUse use, byte spuByte)
        {
            if (use == ATR.SpuUse.NotUsed && spuByte != 0x00)
            {
                throw new ArgumentException("For use=NotUsed the spuByte must be 0x00",nameof(spuByte));
            }
            if (use == ATR.SpuUse.Standard && spuByte == 0x00)
            {
                throw new ArgumentException("For use=Standard the spuByte must be different from 0x00",nameof(spuByte));
            }
            if (use == ATR.SpuUse.Standard && (spuByte & 0x80) == 0x80)
            {
                throw new ArgumentException("For use=Standard the first bit of spuByte must not be set", nameof(spuByte));
            }
            byte Use;
            switch(use)
            {
                case ATR.SpuUse.NotUsed:
                case ATR.SpuUse.Standard:
                    Use = 0x00;
                    break;
                case ATR.SpuUse.Proprietary:
                    Use = 0x80;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(use));
            }
            this.SetInterfaceByte(InterfaceByteGroupType.GlobalExtended, 0, InterfaceByteType.Tb, (byte)(Use|spuByte));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpuType));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpuUse));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpuUseValue));
        }

        public void SetSpuToDefault()
        {
            this.SetInterfaceByte(InterfaceByteGroupType.GlobalExtended, 0, InterfaceByteType.Tb, null);
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpuType));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpuUse));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpuUseValue));
        }

        public override void NotifyAtrChanged()
        {
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ParameterBytes));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.Di));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.DiValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.FiFmax));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.FiFmaxValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.VppProgrammingVoltage));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.IsVppConnected));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.IsVppConnectedValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.VppProgrammingCurrent));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ExtraGuardTime));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ExtraGuardTimeValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpecificModeImplicitFiDi));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpecificModeProtocol));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.CanChangeNegotiableSpecificMode));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ClockStopSupport));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.ClockStopSupportValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.OperatingConditions));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.OperatingConditionsValue));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpuType));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpuUse));
            this.InvokePropertyChanged(nameof(GlobalInterfaceBytes.SpuUseValue));
        }
    }
}