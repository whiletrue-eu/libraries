using System;

namespace WhileTrue.Facades.SmartCard
{
    public enum EbcEncoding
    {
        /// <summary>
        /// longitudinal redundancy check (LRC)
        /// </summary>
        Logitudinal,
        
        /// <summary>
        /// cyclical redundancy check (CRC)
        /// </summary>
        Cyclic,
    }

    public static class EbcEncodingUtils
    {
        public static EbcEncoding UInt32ToEcbEncoding(this uint value)
        {
            switch(value)
            {
                case 0x0:
                    return EbcEncoding.Logitudinal;
                case 0x1:
                    return EbcEncoding.Cyclic;
                default:
                    throw new ArgumentException();
            }
        }
    }
}