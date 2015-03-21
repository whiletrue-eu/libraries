using System;
using System.ComponentModel;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Controls.ATRView
{
    public class ATRViewerModel : ObservableObject
    {
        private Atr Atr
        {
            get;
            set;
        }

        static ATRViewerModel()
        {
            EnumerationAdapter<ProtocolType>.Items = new[]
            {
                new EnumerationAdapter<ProtocolType>(ProtocolType.T0, "T=0", "T=0 (half duplex)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T1, "T=1", "T=1 (half duplex)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T2, "T=2", "T=2 (future full duplex)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T3, "T=3", "T=3 (future full duplex)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T4, "T=4", "T=4 (enhanced half duplex)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T5, "T=5", "T=5 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T6, "T=6", "T=6 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T7, "T=7", "T=7 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T8, "T=8", "T=8 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T9, "T=9", "T=9 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T10, "T=10", "T=10 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T11, "T=11", "T=11 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T12, "T=12", "T=12 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T13, "T=13", "T=13 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.T14, "T=14", "T=14 (proprietary)"),
                new EnumerationAdapter<ProtocolType>(ProtocolType.RFU_F, "0xF", "0xF (RFU, only used to indicate global extended interface bytes)")
            };
            EnumerationAdapter<InterfaceByteGroupType>.Items = new[]
            {
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T0, "T=0", "T=0 (half duplex)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T1, "T=1", "T=1 (half duplex)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T2, "T=2", "T=2 (future full duplex)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T3, "T=3", "T=3 (future full duplex)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T4, "T=4", "T=4 (enhanced half duplex)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T5, "T=5", "T=5 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T6, "T=6", "T=6 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T7, "T=7", "T=7 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T8, "T=8", "T=8 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T9, "T=9", "T=9 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T10, "T=10", "T=10 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T11, "T=11", "T=11 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T12, "T=12", "T=12 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T13, "T=13", "T=13 (RFU ISO/IEC JTC 1/SC 17)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.T14, "T=14", "T=14 (proprietary)"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.Global, "global", "global"),
                new EnumerationAdapter<InterfaceByteGroupType>(InterfaceByteGroupType.GlobalExtended, "global extended (T=15)", "global extended (T=15)")
            };
            EnumerationAdapter<ClockStopSupport>.Items = new[]
            {
                new EnumerationAdapter<ClockStopSupport>(ClockStopSupport.NotSupported, "Not supported", "Not supported"),
                new EnumerationAdapter<ClockStopSupport>(ClockStopSupport.NoPreference, "No preference", "No preference"),
                new EnumerationAdapter<ClockStopSupport>(ClockStopSupport.StateH, "State H", "High level state"),
                new EnumerationAdapter<ClockStopSupport>(ClockStopSupport.StateL, "State L", "Low level state")
            };
            EnumerationAdapter<SPUUse>.Items = new[]
            {
                new EnumerationAdapter<SPUUse>(SPUUse.NotUsed, "Not used", "Not used"),
                new EnumerationAdapter<SPUUse>(SPUUse.Standard, "Standard", "Standard"),
                new EnumerationAdapter<SPUUse>(SPUUse.Proprietary, "Proprietary", "Proprietary")
            };
            EnumerationAdapter<OperatingConditions>.Items = new[]
            {
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.AOnly, "A only", "Class A only"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.BOnly, "B only", "Class B only"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.COnly, "C only", "Class C only"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.AandB, "A and B", "Class A and B"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.BandC, "B and C", "Class B and C"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.ABandC, "A, B and C", "Classes A, B and C"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_05, "0x05", "0x05 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_08, "0x08", "0x08 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_09, "0x09", "0x09 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_0A, "0x0A", "0x0A (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_0B, "0x0B", "0x0B (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_0C, "0x0C", "0x0C (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_0D, "0x0D", "0x0D (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_0E, "0x0E", "0x0E (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_0F, "0x0F", "0x0F (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_10, "0x10", "0x10 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_11, "0x11", "0x11 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_12, "0x12", "0x12 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_13, "0x13", "0x13 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_14, "0x14", "0x14 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_15, "0x15", "0x15 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_16, "0x16", "0x16 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_17, "0x17", "0x17 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_18, "0x18", "0x18 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_19, "0x19", "0x19 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_1A, "0x1A", "0x1A (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_1B, "0x1B", "0x1B (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_1C, "0x1C", "0x1C (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_1D, "0x1D", "0x1D (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_1E, "0x1E", "0x1E (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_1F, "0x1F", "0x1F (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_20, "0x20", "0x20 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_21, "0x21", "0x21 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_22, "0x22", "0x22 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_23, "0x23", "0x23 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_24, "0x24", "0x24 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_25, "0x25", "0x25 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_26, "0x26", "0x26 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_27, "0x27", "0x27 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_28, "0x28", "0x28 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_29, "0x29", "0x29 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_2A, "0x2A", "0x2A (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_2B, "0x2B", "0x2B (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_2C, "0x2C", "0x2C (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_2D, "0x2D", "0x2D (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_2E, "0x2E", "0x2E (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_2F, "0x2F", "0x2F (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_30, "0x30", "0x30 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_31, "0x31", "0x31 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_32, "0x32", "0x32 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_33, "0x33", "0x33 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_34, "0x34", "0x34 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_35, "0x35", "0x35 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_36, "0x36", "0x36 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_37, "0x37", "0x0x37 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_38, "0x38", "0x38 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_39, "0x39", "0x39 (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_3A, "0x3A", "0x3A (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_3B, "0x3B", "0x3B (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_3C, "0x3C", "0x3C (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_3D, "0x3D", "0x3D (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_3E, "0x3E", "0x3E (RFU)"),
                new EnumerationAdapter<OperatingConditions>(OperatingConditions.RFU_3F, "0x3F", "0x3F (RFU)")
            };
            EnumerationAdapter<VPPProgrammingCurrent>.Items = new[]
            {
                new EnumerationAdapter<VPPProgrammingCurrent>(VPPProgrammingCurrent.Current25, "25", "25 mA"),
                new EnumerationAdapter<VPPProgrammingCurrent>(VPPProgrammingCurrent.Current50, "50", "50 mA"),
                new EnumerationAdapter<VPPProgrammingCurrent>(VPPProgrammingCurrent.RFU_10, "10b", "10b (RFU, was 100 mA)"),
                new EnumerationAdapter<VPPProgrammingCurrent>(VPPProgrammingCurrent.RFU_11, "11b", "11b (RFU)")
            };
            EnumerationAdapter<RedundancyCodeType>.Items = new[]
            {
                new EnumerationAdapter<RedundancyCodeType>(RedundancyCodeType.CRC, "CRC", "CRC (Cyclic Redundancy Check)"),
                new EnumerationAdapter<RedundancyCodeType>(RedundancyCodeType.LRC, "LRC", "LRC (Longitudinal Redundancy Check)"),
            };

            EnumerationAdapter<FiFmax>.Items = new[]
            {
                new EnumerationAdapter<FiFmax>(FiFmax.Fi372_fMax4, "0", "0 (F=372, 4 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi372_fMax5, "1","1 (F=372, 5 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi558_fMax6, "2","2 (F=558, 6 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi744_fMax8, "3","3 (F=744, 8 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi1116_fMax12, "4","4 (F=1116, 12 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi1488_fMax16, "5","5 (F=1488, 16 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi1860_fMax20, "6","6 (F=1860, 20 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.RFU_7, "7","7 (RFU)"),
                new EnumerationAdapter<FiFmax>(FiFmax.RFU_8, "8","8 (RFU)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi512_fMax5, "9","9 (F=512, 5 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi768_fMax7p5, "A","A (F=768, 7.5 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi1024_fMax10, "B","B (F=1024, 10 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi1536_fMax15, "C","C (F=1536, 15 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.Fi2048_fMax20, "D","D (F=2048, 20 MHz)"),
                new EnumerationAdapter<FiFmax>(FiFmax.RFU_E, "E","E (RFU)"),
                new EnumerationAdapter<FiFmax>(FiFmax.RFU_F, "F","F (RFU)")
            };

            EnumerationAdapter<Di>.Items = new[]
            {
                new EnumerationAdapter<Di>(Di.RFU_0, "0", "0 (= RFU)"),
                new EnumerationAdapter<Di>(Di.Di1, "1", "1 (D=1)"),
                new EnumerationAdapter<Di>(Di.Di2, "2", "2 (D=2)"),
                new EnumerationAdapter<Di>(Di.Di4, "3", "3 (D=4)"),
                new EnumerationAdapter<Di>(Di.Di8, "4", "4 (D=8)"),
                new EnumerationAdapter<Di>(Di.Di16, "5", "5 (D=16)"),
                new EnumerationAdapter<Di>(Di.Di32, "6", "6 (D=32)"),
                new EnumerationAdapter<Di>(Di.Di64, "7", "7 (D=64)"),
                new EnumerationAdapter<Di>(Di.Di12, "8", "8 (D=12)"),
                new EnumerationAdapter<Di>(Di.Di20, "9", "9 (D=20)"),
                new EnumerationAdapter<Di>(Di.RFU_A, "A", "A (RFU)"),
                new EnumerationAdapter<Di>(Di.RFU_B, "B", "B (RFU)"),
                new EnumerationAdapter<Di>(Di.RFU_C, "C", "C (RFU)"),
                new EnumerationAdapter<Di>(Di.RFU_D, "D", "D (RFU)"),
                new EnumerationAdapter<Di>(Di.RFU_E, "E", "E (RFU)"),
                new EnumerationAdapter<Di>(Di.RFU_F, "F", "F (RFU)")
            };

            EnumerationAdapter<EtsiSpuSecureChannelSupport>.Items = new[]
            {
                new EnumerationAdapter<EtsiSpuSecureChannelSupport>(EtsiSpuSecureChannelSupport.NotIndicated, "Not indicated", "Support not indicated"),
                new EnumerationAdapter<EtsiSpuSecureChannelSupport>(EtsiSpuSecureChannelSupport.SecureChannelSupported, "Secure Channel", "Secure Channel supported as defined in TS 102 484"),
                new EnumerationAdapter<EtsiSpuSecureChannelSupport>(EtsiSpuSecureChannelSupport.SecuredApduRequired, "Secured APDU", "Secured APDU - Platform to Platform required as defined in TS 102 484"),
                new EnumerationAdapter<EtsiSpuSecureChannelSupport>(EtsiSpuSecureChannelSupport.RFU_01, "01b (RFU)", "01b (RFU)"),
            };

            EnumerationAdapter<HistoricalCharacterTypes>.Items = new[]
            {
                new EnumerationAdapter<HistoricalCharacterTypes>(HistoricalCharacterTypes.CompactTlv, "Compact TLV", "Compact TLV (00/80)"),
                new EnumerationAdapter<HistoricalCharacterTypes>(HistoricalCharacterTypes.DirDataReference, "Dir Data Reference", "Dir Data Reference (10)"),
                new EnumerationAdapter<HistoricalCharacterTypes>(HistoricalCharacterTypes.Proprietary, "Proprietary", "Proprietary (01-0A/11-7F/90-FF)"),
                new EnumerationAdapter<HistoricalCharacterTypes>(HistoricalCharacterTypes.Rfu, "RFU", "RFU (81-8F)"),
                new EnumerationAdapter<HistoricalCharacterTypes>(HistoricalCharacterTypes.No, "No", "No historical characters"),
            };

            IPropertyAdapterFactory<ATRViewerModel> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<ATRViewerModel>();

            tokenizedAtrAdapter = PropertyFactory.Create(
                @this => @this.TokenizedAtr,
                @this => new TokenizedAtrAdapter(@this.Atr.TokenizedAtr)
                );

            interpretedAtrAdapter = PropertyFactory.Create(
                @this => @this.InterpretedAtr,
                @this => new InterpretedAtrAdapter(@this.Atr)
                );
        }

        private static readonly ReadOnlyPropertyAdapter<ATRViewerModel, TokenizedAtrAdapter> tokenizedAtrAdapter;
        private static readonly ReadOnlyPropertyAdapter<ATRViewerModel, InterpretedAtrAdapter> interpretedAtrAdapter;
        private string atrValue;
        private string error;

        public ATRViewerModel(Atr atr)
        {
            this.Atr = atr;
            this.Atr.PropertyChanged+= this.AtrChanged;
            this.atrValue = atr.Bytes.ToHexString(" ");

            this.AddValidationForProperty(() => AtrValue)
                .AddValidation(_ => this.Error == null, _ => this.Error);
        }

        private void AtrChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ReflectionHelper.GetPropertyName(() => this.Atr.Bytes))
            {
                this.AtrValue = this.Atr.Bytes.ToHexString(" ");
            }
        }

        public InterpretedAtrAdapter InterpretedAtr
        {
            get { return interpretedAtrAdapter.GetValue(this); }
        }

        public string AtrValue
        {
            get { return this.atrValue; }
            set
            {
                this.SetAndInvoke(() => AtrValue, ref this.atrValue, value);
                try
                {
                    byte[] AtrBytes = this.atrValue.ToByteArray();
                    this.Error = null;
                    if (this.Atr.Bytes.HasEqualValue(AtrBytes) == false)
                        {//avoid recursion
                            try
                            {
                                this.Atr.Bytes = AtrBytes;
                                string AtrValue = AtrBytes.ToHexString(" ");
                                if (this.atrValue.Trim() != AtrValue)
                                {
                                    //reformat if value is different
                                    this.atrValue = AtrValue;
                                }
                            }
                            catch (Exception Exception)
                            {
                                this.Error = Exception.Message;
                            }
                        }
                }
                catch (Exception Exception)
                {
                    this.Error = "Atr is not a valid hexadecimal value";
                }
            }
        }

        public string Error
        {
            get { return this.error; }
            set { this.SetAndInvoke(() => Error, ref this.error, value); }
        }

        public TokenizedAtrAdapter TokenizedAtr
        {
            get { return tokenizedAtrAdapter.GetValue(this); }
        }
    }
}