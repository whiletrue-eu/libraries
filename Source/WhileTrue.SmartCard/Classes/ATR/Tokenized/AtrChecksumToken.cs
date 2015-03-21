using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR.Tokenized
{
    public class AtrChecksumToken : IAtrToken
    {
        private readonly byte checkByte;

        internal AtrChecksumToken(AtrReadStream atr)
        {
            this.CalculatedChecksum = CalculateChecksum(atr.GetPreviousBytes().GetSubArray(1));
            this.checkByte = atr.GetNextByte();
            
            this.ChecksumValid = this.checkByte == CalculatedChecksum;
        }

        internal AtrChecksumToken(byte[] atrBytesWithoutChecksum)
        {
            this.CalculatedChecksum = CalculateChecksum(atrBytesWithoutChecksum.GetSubArray(1));
            this.checkByte = this.CalculatedChecksum;

            this.ChecksumValid = true;
        }

        public byte CalculatedChecksum { get; private set; }

        public bool ChecksumValid { get; private set; }

        public static byte CalculateChecksum(IEnumerable<byte> bytesToIncludeInChecksum)
        {
            return bytesToIncludeInChecksum.Aggregate<byte, byte>(0x00, (Current, Byte) => (byte) (Current ^ Byte));
        }

        public byte CheckByte
        {
            get { return this.checkByte; }
        }

        public byte[] Bytes { get { return new[] {this.checkByte}; } 
        }
    }
}