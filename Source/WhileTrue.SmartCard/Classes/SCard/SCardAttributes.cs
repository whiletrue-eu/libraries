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

        VendorName = AttrClass_VendorInfo | 0x0100,
        vendorIFDType = AttrClass_VendorInfo | 0x0101,
        VendorIFDVersion = AttrClass_VendorInfo | 0x0102,
        VendorIFDSerialNumber = AttrClass_VendorInfo | 0x0103,
        ChannelID = AttrClass_Communications | 0x0110,
        ProtocolTypes = AttrClass_Protocol | 0x0120,
        DefaultClock = AttrClass_Protocol | 0x0121,
        MaximumClock = AttrClass_Protocol | 0x0122,
        DefaultDataRate = AttrClass_Protocol | 0x0123,
        MaximumDataRate = AttrClass_Protocol | 0x0124,
        MaximumIFSD = AttrClass_Protocol | 0x0125,
        PowerManagementSupport = AttrClass_PowerManagement | 0x0131,
        UserToCardAuthDevice = AttrClass_Security | 0x0140,
        UserAuthInputDevice = AttrClass_Security | 0x0142,
        Characteristics = AttrClass_Mechanical | 0x0150,
        CurrentProtocolType = AttrClass_IFDProtocol | 0x0201,
        CurrentClock = AttrClass_IFDProtocol | 0x0202,
        CurrentF = AttrClass_IFDProtocol | 0x0203,
        CurrentD = AttrClass_IFDProtocol | 0x0204,
        CurrentN = AttrClass_IFDProtocol | 0x0205,
        CurrentW = AttrClass_IFDProtocol | 0x0206,
        CurrentIFSC = AttrClass_IFDProtocol | 0x0207,
        CurrentIFSD = AttrClass_IFDProtocol | 0x0208,
        CurrentBWT = AttrClass_IFDProtocol | 0x0209,
        CurrentCWT = AttrClass_IFDProtocol | 0x020a,
        CurrentEBCEncoding = AttrClass_IFDProtocol | 0x020b,
        ExtendedBWT = AttrClass_IFDProtocol | 0x020c,
        ICCPresence = AttrClass_ICCState | 0x0300,
        ICCInterfaceStatus = AttrClass_ICCState | 0x0301,
        CurrentIOState = AttrClass_ICCState | 0x0302,
        ATRString = AttrClass_ICCState | 0x0303,
        ICCTypePerATR = AttrClass_ICCState | 0x0304,
        ESCReset = AttrClass_VendorDefined | 0xA000,
        ESCCancel = AttrClass_VendorDefined | 0xA003,
        ESCAuthRequest = AttrClass_VendorDefined | 0xA005,
        MaxInput = AttrClass_VendorDefined | 0xA007,
        DeviceUnit = AttrClass_System | 0x0001,
        DeviceInUse = AttrClass_System | 0x0002,
        //DeviceFriendlyNameA = AttrClass_System | 0x0003,
        //DeviceSystemNameA = AttrClass_System | 0x0004,
        DeviceFriendlyName = AttrClass_System | 0x0005,
        DeviceSystemName = AttrClass_System | 0x0006,
        SuppressT1IFSRequest = AttrClass_System | 0x0007,
        NumTransmissions = AttrClass_Performance | 0x0001,
        BytesTransmitted = AttrClass_Performance | 0x0002,
        TransmissionTime = AttrClass_Performance | 0x0003,
    }
}