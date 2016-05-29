using System.Collections.Generic;

namespace WhileTrue.Classes.ATR
{
    internal class AtrWriteStream
    {
        private readonly List<byte> data = new List<byte>();

        public void WriteByte( byte value)
        {
            this.data.Add(value);
        }   
        public void WriteBytes( byte[] value)
        {
            this.data.AddRange(value);
        }

        public byte[] ToByteArray()
        {
            return this.data.ToArray();
        }

    }
}