using System;

namespace WhileTrue.Classes.ATR
{
    //Table 6.7: Coding of the first TBi (i > 2) after T = 15 of the ATR
// b8 b7 b6 b5 b4 b3 b2 b1 Meaning
//  0  0  0  0  0  0  0  0 No additional global interface parameters supported
//  1  -  -  1  -  -  -  - Low Impedance drivers and protocol available on the I/O line available (see clause 7.2.1)
//  1  1  -  -  -  -  -  - Inter-Chip USB UICC-Terminal interface supported as defined in TS 102 600 [18]
//  1  -  1  -  -  -  -  - UICC-CLF interface supported as defined in TS 102 613 [19]
//  1  -  -  -  1  -  -  - Secure Channel supported as defined in TS 102 484 [20].
//  1  -  -  -  1  1  -  - Secured APDU - Platform to Platform required as defined in TS 102 484 [20]
//NOTE: Any other value is RFU.

    public class SpuTypeEtsiCoding
    {
        private readonly byte value;

        public SpuTypeEtsiCoding(byte value)
        {
            this.value = value;
        }

        public static byte GetSpuTypeEtsiCodingAsByte(bool lowImpedanceOnIoLineAvailable, bool interChipUsbSupported, bool clfInterfaceSupported, EtsiSpuSecureChannelSupport secureChannelSupport)
        {
            return (byte) (0x80 /*proprietary usage*/|
                           (lowImpedanceOnIoLineAvailable ? 0x10 : 0x00) |
                           (interChipUsbSupported ? 0x40 : 0x00) |
                           (clfInterfaceSupported ? 0x20 : 0x00) |
                           ((byte) secureChannelSupport)
                );
        }

        public bool LowImpedanceOnIoLineAvailable => (this.value & 0x10) != 0x00;

        public bool InterChipUsbSupported => (this.value & 0x40) != 0x00;

        public bool ClfInterfaceSupported => (this.value & 0x20) != 0x00;

        public EtsiSpuSecureChannelSupport SecureChannelSupport
        {
            get
            {
                switch (this.value & 0x0C)
                {
                    case 0x00:
                        return EtsiSpuSecureChannelSupport.NotIndicated;
                    case 0x08:
                        return EtsiSpuSecureChannelSupport.SecureChannelSupported;
                    case 0x0C:
                        return EtsiSpuSecureChannelSupport.SecuredApduRequired;
                    case 0x04:
                        return EtsiSpuSecureChannelSupport.Rfu01;
                    default:
                        throw new ArgumentException();
                }
            }
        }
    }
}