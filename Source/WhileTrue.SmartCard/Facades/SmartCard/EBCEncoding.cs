using System;

namespace WhileTrue.Facades.SmartCard
{
    public enum EBCEncoding
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

    public static class EBCEncodingUtils
    {
        public static EBCEncoding UInt32ToECBEncoding(this uint value)
        {
            switch(value)
            {
                case 0x0:
                    return EBCEncoding.Logitudinal;
                case 0x1:
                    return EBCEncoding.Cyclic;
                default:
                    throw new ArgumentException();
            }
        }
    }
}