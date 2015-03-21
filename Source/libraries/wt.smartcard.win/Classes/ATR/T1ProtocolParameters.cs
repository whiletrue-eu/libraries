using System;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public sealed class T1ProtocolParameters : ProtocolParametersBase
    {
        public T1ProtocolParameters(Atr owner)
            : base(owner, ProtocolType.T1, _=>_.Type==(InterfaceByteGroupType) ProtocolType.T1 &&_.Number!=2)
        // if not T=0 is indicated, the T=0 specific byte should not be set. In this case, there cannot be protocol specific information
        // coded in this group. This means, the T=x specific protocol indicated here has to be indicated again in a following group if data has to be set
        {
        }


//        11.4.2 Information field sizes
//          IFSC is the maximum size of information field of blocks that can be received by the card. If present, the first
//          TA for T=1 sets up the initial value of IFSC. The default value is 32.
//          IFSD is the maximum size of information field of blocks that can be received by the interface device. The initial
//          value of IFSD is 32.
//          At the start of the transmission protocol, IFSC and IFSD are initialized. During the transmission protocol, IFSC
//          and IFSD may be adjusted by S(IFS request) and S(IFS response) where INF consists of one byte named IFS.
//          In any case, the first TA for T=1 and each byte IFS shall be encoded as follows.
//          ⎯ The values '00' and 'FF' are reserved for future use.
//          ⎯ The values '01' to 'FE' encode the numbers 1 to 254.
//          NOTE 1 This document recommends an IFS value of at least '20'.
//          NOTE 2 The block size is the total number of bytes present in the prologue, information and epilogue fields. The
//          maximum block size is set to IFS plus four or five, depending upon the size of the epilogue field.

        /// <summary>
        /// IFSC is the maximum size of information field of blocks that can be received by the card. if not coded, returns <c>null</c>
        /// </summary>
        public byte? Ifsc
        {
            get
            {
                return this.GetInterfaceByte(0, InterfaceByteType.Ta);
            }
            set
            {
                if (value.HasValue)
                {
                    this.SetInterfaceByte(0, InterfaceByteType.Ta, value.Value);
                }
                else
                {
                    this.SetInterfaceByte(0, InterfaceByteType.Ta, null);
                }
                this.InvokePropertyChanged(nameof(T1ProtocolParameters.Ifsc));
                this.InvokePropertyChanged(nameof(T1ProtocolParameters.IfscValue));
            }
        }

        /// <summary>
        /// IFSC is the maximum size of information field of blocks that can be received by the card.
        /// </summary>
        public byte IfscValue => this.Ifsc ?? 32;

        //11.4.3 Waiting times
        //    By definition, CWT is the maximum delay between the leading edges of two consecutive characters in the
        //    block (see Figure 21). The minimum delay is CGT (see 11.2).
        //    NOTE When there is a potential error in the length, CWT may be used to detect the end of a block.
        //
        //         Character of a block     Next character of the same block
        //    __   ______________________   ____________________
        //      | | | | | | | | | |      | | | | | | | | | | 
        //      |_|_|_|_|_|_|_|_|_|      |_|_|_|_|_|_|_|_|_|
        //      : <--  CGT ≤ t ≤ CWT --> :
        //    Figure 21 — Character timings within the block
        //
        //    Bits 4 to 1 of the first TB for T=1 encode CWI from zero to fifteen. The default value is CWI = 13. CWT is
        //    calculated from CWI by the following formula. Therefore the minimum value is CWT = 12 etu.
        //    CWT = (11+ 2^CWI ) etu

        /// <summary>
        /// CWT is the maximum delay between the leading edges of two consecutive characters in the block (see Figure 21). if not coded, returns <c>null</c>
        /// </summary>
        public byte? Cwi
        {
            get
            {
                byte? Value = this.GetInterfaceByte(0, InterfaceByteType.Tb);
                return Value.HasValue ? (byte?)Value.Value.GetLoNibble() : null;
            }
        }

        public void SetCwIandBwi(byte cwi, byte bwi)
        {
            this.SetInterfaceByte(0, InterfaceByteType.Tb, CodingUtils.NibbleToByte(bwi,cwi));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.Cwi));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.CwiValue));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.Bwi));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.BwiValue));
        }

        public void SetCwIandBwiToDefault()
        {
            this.SetInterfaceByte(0, InterfaceByteType.Tb, null);
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.Cwi));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.CwiValue));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.Bwi));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.BwiValue));
        }

        /// <summary>
        /// CWT is the maximum delay between the leading edges of two consecutive characters in the block (see Figure 21).
        /// </summary>
        public byte CwiValue => this.Cwi ?? 13;

        //11.4.3 Waiting times
        //    By definition, BWT is the maximum delay between the leading edge of the last character of the block received
        //    by the card and the leading edge of the first character of the next block transmitted by the card (see Figure 22).
        //    BWT is used to detect an unresponsive card. The minimum delay is BGT (see 11.2).
        //
        //    Last character of a block     First character of the next block
        //    sent by the interface device  sent by the card
        //    __   ______________________   ____________________
        //      | | | | | | | | | |      | | | | | | | | | | 
        //      |_|_|_|_|_|_|_|_|_|      |_|_|_|_|_|_|_|_|_|
        //      : <--  BGT ≤ t ≤ BWT --> :
        //    Figure 22 — Block timings
        //
        //    Bits 8 to 5 of the first TB for T=1 encode BWI from zero to nine. The values 'A' to 'F' are reserved for future use.
        //    The default value is BWI = 4. BWT is calculated from BWI by the following formula.
        //    BWT = 11 etu + 2^BWI × 960 × Fd/f

        /// <summary>
        /// BWT is the maximum delay between the leading edge of the last character of the block received
        /// by the card and the leading edge of the first character of the next block transmitted by the card. 
        /// if not coded, returns <c>null</c>
        /// </summary>
        public byte? Bwi
        {
            get
            {
                byte? Value = this.GetInterfaceByte(0, InterfaceByteType.Tb);
                return Value.HasValue ? (byte?)Value.Value.GetHiNibble() : null;
            }
        }

        /// <summary>
        /// BWT is the maximum delay between the leading edge of the last character of the block received
        /// by the card and the leading edge of the first character of the next block transmitted by the card. 
        /// </summary>
        public byte BwiValue => this.Bwi ?? 4;

        //11.4.4 Redundancy code
        //    Bit 1 of the first TC for T=1 indicates the error detection code to be used:
        //    ⎯ CRC if bit 1 is set to 1;
        //    ⎯ LRC (default value) if bit 1 is set to 0.
        //    Bits 8 to 2 of the first TC for T=1 are reserved for future use and shall be set to 0.
        
        /// <summary>
        /// Redundancy code. if not coded, returns <c>null</c>
        /// </summary>
        public RedundancyCodeType? RedundancyCode
        {
            get
            {
                byte? RedundancyCode = this.GetInterfaceByte(0, InterfaceByteType.Tc);
                if (RedundancyCode.HasValue)
                switch( RedundancyCode.Value & 0x01 )
                {
                    case 0x01:
                        return RedundancyCodeType.Crc;
                    case 0x00:
                        return RedundancyCodeType.Lrc;
                    default:
                        throw new ArgumentException();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    this.SetInterfaceByte(0,InterfaceByteType.Tc, (byte?) value.Value);
                }
                else
                {
                    this.SetInterfaceByte(0, InterfaceByteType.Tc, null);
                }
                this.InvokePropertyChanged(nameof(T1ProtocolParameters.RedundancyCode));
                this.InvokePropertyChanged(nameof(T1ProtocolParameters.RedundancyCodeValue));
            }
        }

        /// <summary>
        /// Redundancy code
        /// </summary>
        public RedundancyCodeType RedundancyCodeValue => this.RedundancyCode ?? RedundancyCodeType.Lrc;

        public override void NotifyAtrChanged()
        {
            base.NotifyAtrChanged();
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.Ifsc));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.IfscValue));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.Cwi));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.CwiValue));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.Bwi));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.BwiValue));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.RedundancyCode));
            this.InvokePropertyChanged(nameof(T1ProtocolParameters.RedundancyCodeValue));
        }
    }
}