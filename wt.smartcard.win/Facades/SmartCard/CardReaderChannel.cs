namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// Channel the card reader is connected to
    /// </summary>
    public enum CardReaderChannel
    {
        Unknown = 0x00,
        Serial = 0x01,
        Parallel = 0x02,
        Ps2 = 0x04,
        Scsi = 0x08,
        Ide = 0x10,
        Usb = 0x20,
        Vendor0 = 0xF0,
        Vendor1 = 0xF1,
        Vendor2 = 0xF2,
        Vendor3 = 0xF3,
        Vendor4 = 0xF4,
        Vendor5 = 0xF5,
        Vendor6 = 0xF6,
        Vendor7 = 0xF7,
        Vendor8 = 0xF8,
        Vendor9 = 0xF9,
        VendorA = 0xFA,
        VendorB = 0xFB,
        VendorC = 0xFC,
        VendorD = 0xFD,
        VendorE = 0xFE,
        VendorF = 0xFF,
    }

    public static class CardReaderChannelUtils
    {
        public static CardReaderChannel UShortToCardReaderChannel(this ushort value)
        {
            switch(value)
            {
                case 0x01:
                    return CardReaderChannel.Serial;
                case 0x02:
                    return CardReaderChannel.Parallel;
                case 0x04:
                    return CardReaderChannel.Ps2;
                case 0x08:
                    return CardReaderChannel.Scsi;
                case 0x10:
                    return CardReaderChannel.Ide;
                case 0x20:
                    return CardReaderChannel.Usb;
                case 0xF0:
                    return CardReaderChannel.Vendor0;
                case 0xF1:
                    return CardReaderChannel.Vendor1;
                case 0xF2:
                    return CardReaderChannel.Vendor2;
                case 0xF3:
                    return CardReaderChannel.Vendor3;
                case 0xF4:
                    return CardReaderChannel.Vendor4;
                case 0xF5:
                    return CardReaderChannel.Vendor5;
                case 0xF6:
                    return CardReaderChannel.Vendor6;
                case 0xF7:
                    return CardReaderChannel.Vendor7;
                case 0xF8:
                    return CardReaderChannel.Vendor8;
                case 0xF9:
                    return CardReaderChannel.Vendor9;
                case 0xFA:
                    return CardReaderChannel.VendorA;
                case 0xFB:
                    return CardReaderChannel.VendorB;
                case 0xFC:
                    return CardReaderChannel.VendorC;
                case 0xFD:
                    return CardReaderChannel.VendorD;
                case 0xFE:
                    return CardReaderChannel.VendorE;
                case 0xFF:
                    return CardReaderChannel.VendorF;
                default:
                    return CardReaderChannel.Unknown;
            }
        }
    }
}