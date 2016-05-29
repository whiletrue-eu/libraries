using System;

namespace WhileTrue.Classes.ATR
{
    internal class AtrReadStream
    {
        private readonly byte[] data;
        private int offset;

        public AtrReadStream(byte[] atr)
        {
            this.data = atr;
            this.offset = 0;
        }

        public byte GetNextByte()
        {
            if (this.offset < this.data.Length)
            {
                int Offset = this.offset;
                this.offset++;
                return this.data[Offset];
            }
            else
            {
                throw new InvalidAtrCodingException("ATR is not long enough. At least 1 byte is missing", this.offset);
            }
        }

        public byte[] GetNextBytes(int length)
        {
            if( this.offset+length <= this.data.Length)
            {
                byte[] Data = new byte[length];
                Array.Copy(this.data, this.offset, Data, 0, length);
                this.offset += length;
                return Data;
            }
            else
            {
                throw new InvalidAtrCodingException("ATR is not long enough. At least {0} bytes are missing", this.offset + length - this.data.Length);
            }
        }

        public int GetRemainingLength()
        {
            return this.data.Length - this.offset;
        }

        public byte[] GetPreviousBytes()
        {
            byte[] Data = new byte[this.offset];
            Array.Copy(this.data, 0, Data, 0, this.offset);
            return Data;
        }
    }
}