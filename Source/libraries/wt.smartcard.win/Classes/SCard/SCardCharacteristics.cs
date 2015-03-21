// ReSharper disable UnusedMember.Global
namespace WhileTrue.Classes.SCard
{
    public enum SCardCharacteristics
    {
        /// <summary>
        /// Card swallowing mechanism 
        /// </summary>
        Swallow = 0x00000001,
        /// <summary>
        /// Card ejection mechanism 
        /// </summary>
        Eject = 0x00000002,
        /// <summary>
        /// Card capture mechanism
        /// </summary>
        Capture = 0x00000004,
    }
}