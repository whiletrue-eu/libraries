namespace WhileTrue.Classes.ATR
{
    public class SpuType
    {
        private readonly byte value;

        public SpuType(byte spuType)
        {
            this.value = spuType;
        }

        public byte Value
        {
            get { return (byte) (this.value & 0x7F); }
        }

        public SpuTypeEtsiCoding EtsiCoding
        {
            get
            {
                if ((this.value & 0x80) == 0x80)//0x80 -> proprietary use
                {
                    return new SpuTypeEtsiCoding(this.Value);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}