namespace WhileTrue.Classes.ATR
{
    public enum CompactTLVTypes:byte
    {
        RFU_40=0x40,
        CountryCode=0x41,
        IssuerIdentificationNumber=0x42,
        CardServiceData=0x43,
        InitialAccessData=0x44,
        CardIssuerData=0x45,
        PreIssuingData=0x46,
        CardCapabilities = 0x47,
        StatusIndicator = 0x48,
        RFU_49 = 0x49,
        RFU_4A = 0x4A,
        RFU_4B = 0x4B,
        RFU_4C = 0x4C,
        RFU_4D = 0x4D,
        RFU_4E = 0x4E,
        ApplicationIdentifer = 0x4F,
    }
}