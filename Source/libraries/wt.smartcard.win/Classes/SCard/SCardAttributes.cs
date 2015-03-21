// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace WhileTrue.Classes.SCard
{
    public enum SCardAttributes : uint
    {
        /// <summary>
        /// Vendor information definitions
        /// </summary>
        AttrClass_VendorInfo = 0x00010000,
        /// <summary>
        /// Communication definitions
        /// </summary>
        AttrClass_Communications = 0x00020000,
        /// <summary>
        /// Protocol definitions
        /// </summary>
        AttrClass_Protocol = 0x00030000,
        /// <summary>
        /// Power Management definitions
        /// </summary>
        AttrClass_PowerManagement = 0x00040000,
        /// <summary>
        /// Security Assurance definitions
        /// </summary>
        AttrClass_Security = 0x00050000,
        /// <summary>
        /// Mechanical characteristic definitions
        /// </summary>
        AttrClass_Mechanical = 0x00060000,
        /// <summary>
        /// Vendor specific definitions
        /// </summary>
        AttrClass_VendorDefined = 0x00070000,
        /// <summary>
        /// Instance Device Protocol options
        /// </summary>
        AttrClass_IFDProtocol = 0x00080000,
        /// <summary>
        /// ICC State specific definitions
        /// </summary>
        AttrClass_ICCState = 0x00090000,
        /// <summary>
        /// performace counters
        /// </summary>
        AttrClass_Performance = 0x7ffe0000,
        /// <summary>
        /// System-specific definitions
        /// </summary>
        AttrClass_System = 0x7fff0000,

        VendorName = SCardAttributes.AttrClass_VendorInfo | 0x0100,
        vendorIFDType = SCardAttributes.AttrClass_VendorInfo | 0x0101,
        VendorIFDVersion = SCardAttributes.AttrClass_VendorInfo | 0x0102,
        VendorIFDSerialNumber = SCardAttributes.AttrClass_VendorInfo | 0x0103,
        ChannelID = SCardAttributes.AttrClass_Communications | 0x0110,
        ProtocolTypes = SCardAttributes.AttrClass_Protocol | 0x0120,
        DefaultClock = SCardAttributes.AttrClass_Protocol | 0x0121,
        MaximumClock = SCardAttributes.AttrClass_Protocol | 0x0122,
        DefaultDataRate = SCardAttributes.AttrClass_Protocol | 0x0123,
        MaximumDataRate = SCardAttributes.AttrClass_Protocol | 0x0124,
        MaximumIFSD = SCardAttributes.AttrClass_Protocol | 0x0125,
        PowerManagementSupport = SCardAttributes.AttrClass_PowerManagement | 0x0131,
        UserToCardAuthDevice = SCardAttributes.AttrClass_Security | 0x0140,
        UserAuthInputDevice = SCardAttributes.AttrClass_Security | 0x0142,
        Characteristics = SCardAttributes.AttrClass_Mechanical | 0x0150,
        CurrentProtocolType = SCardAttributes.AttrClass_IFDProtocol | 0x0201,
        CurrentClock = SCardAttributes.AttrClass_IFDProtocol | 0x0202,
        CurrentF = SCardAttributes.AttrClass_IFDProtocol | 0x0203,
        CurrentD = SCardAttributes.AttrClass_IFDProtocol | 0x0204,
        CurrentN = SCardAttributes.AttrClass_IFDProtocol | 0x0205,
        CurrentW = SCardAttributes.AttrClass_IFDProtocol | 0x0206,
        CurrentIFSC = SCardAttributes.AttrClass_IFDProtocol | 0x0207,
        CurrentIFSD = SCardAttributes.AttrClass_IFDProtocol | 0x0208,
        CurrentBWT = SCardAttributes.AttrClass_IFDProtocol | 0x0209,
        CurrentCWT = SCardAttributes.AttrClass_IFDProtocol | 0x020a,
        CurrentEBCEncoding = SCardAttributes.AttrClass_IFDProtocol | 0x020b,
        ExtendedBWT = SCardAttributes.AttrClass_IFDProtocol | 0x020c,
        ICCPresence = SCardAttributes.AttrClass_ICCState | 0x0300,
        ICCInterfaceStatus = SCardAttributes.AttrClass_ICCState | 0x0301,
        CurrentIOState = SCardAttributes.AttrClass_ICCState | 0x0302,
        ATRString = SCardAttributes.AttrClass_ICCState | 0x0303,
        ICCTypePerATR = SCardAttributes.AttrClass_ICCState | 0x0304,
        ESCReset = SCardAttributes.AttrClass_VendorDefined | 0xA000,
        ESCCancel = SCardAttributes.AttrClass_VendorDefined | 0xA003,
        ESCAuthRequest = SCardAttributes.AttrClass_VendorDefined | 0xA005,
        MaxInput = SCardAttributes.AttrClass_VendorDefined | 0xA007,
        DeviceUnit = SCardAttributes.AttrClass_System | 0x0001,
        DeviceInUse = SCardAttributes.AttrClass_System | 0x0002,
        //DeviceFriendlyNameA = AttrClass_System | 0x0003,
        //DeviceSystemNameA = AttrClass_System | 0x0004,
        DeviceFriendlyName = SCardAttributes.AttrClass_System | 0x0005,
        DeviceSystemName = SCardAttributes.AttrClass_System | 0x0006,
        SuppressT1IFSRequest = SCardAttributes.AttrClass_System | 0x0007,
        NumTransmissions = SCardAttributes.AttrClass_Performance | 0x0001,
        BytesTransmitted = SCardAttributes.AttrClass_Performance | 0x0002,
        TransmissionTime = SCardAttributes.AttrClass_Performance | 0x0003,
    }
}