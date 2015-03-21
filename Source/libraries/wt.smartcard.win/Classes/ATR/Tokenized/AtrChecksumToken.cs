using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR.Tokenized
{
    public class AtrChecksumToken : IAtrToken
    {
        internal AtrChecksumToken(AtrReadStream atr)
        {
            this.CalculatedChecksum = AtrChecksumToken.CalculateChecksum(atr.GetPreviousBytes().GetSubArray(1));
            this.CheckByte = atr.GetNextByte();
            
            this.ChecksumValid = this.CheckByte == this.CalculatedChecksum;
        }

        internal AtrChecksumToken(byte[] atrBytesWithoutChecksum)
        {
            this.CalculatedChecksum = AtrChecksumToken.CalculateChecksum(atrBytesWithoutChecksum.GetSubArray(1));
            this.CheckByte = this.CalculatedChecksum;

            this.ChecksumValid = true;
        }

        public byte CalculatedChecksum { get; }

        public bool ChecksumValid { get; private set; }

        public static byte CalculateChecksum(IEnumerable<byte> bytesToIncludeInChecksum)
        {
            return bytesToIncludeInChecksum.Aggregate<byte, byte>(0x00, (current, Byte) => (byte) (current ^ Byte));
        }

        public byte CheckByte { get; }

        public byte[] Bytes => new[] {this.CheckByte};
    }
}