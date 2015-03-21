namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    /// Utility class for bit nibbling
    /// </summary>
    public static class CodingUtils
    {
        /// <summary/>
        public static byte GetHiNibble(this byte value)
        {
            return (byte) ((value >> 4) & 0x0F);
        }
        /// <summary/>
        public static byte GetLoNibble(this byte value)
        {
            return (byte)(value & 0x0F);
        }
        /// <summary/>
        public static byte NibbleToByte(byte hiNibble, byte loNibble)
        {
            return (byte)(((hiNibble & 0x0F)<<4) | (loNibble & 0x0F));
        }

        /// <summary/>
        public static ushort GetHiUShort(this uint value)
        {
            return (ushort)((value >> 16) & 0x0000FFFF);
        }
        /// <summary/>
        public static ushort GetLoUShort(this uint value)
        {
            return (ushort)(value & 0x0000FFFF);
        }

        /// <summary/>
        public static bool IsBitSet(this int value, int bitNo)
        {
            return (value & (0x1<<bitNo)) != 0;
        }

        /// <summary/>
        public static bool IsBitSet(this byte value, int bitNo)
        {
            return CodingUtils.IsBitSet((int)value, bitNo);
        }

        /// <summary/>
        public static int ToInt32(this byte[] value)
        {
            value.DbC_Assure(array => array.Length == 4);

            return (value[3] << 24) | (value[2] << 16) | (value[1] << 8) | value[0];
        }

        /// <summary/>
        public static uint ToUInt32(this byte[] value)
        {
            value.DbC_Assure(array => array.Length == 4);

            return (uint) ((value[3] << 24) | (value[2] << 16) | (value[1] << 8) | value[0]);
        }
    }
}
