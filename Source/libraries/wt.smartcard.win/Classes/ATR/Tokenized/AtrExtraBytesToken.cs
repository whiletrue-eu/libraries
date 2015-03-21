namespace WhileTrue.Classes.ATR.Tokenized
{
    /// <summary>
    /// 
    /// </summary>
    public class AtrExtraBytesToken : IAtrToken
    {
        internal AtrExtraBytesToken(AtrReadStream atrStream, int additionalBytes)
        {
            this.Bytes = atrStream.GetNextBytes(additionalBytes);
        }

        public byte[] Bytes { get; }
    }
}