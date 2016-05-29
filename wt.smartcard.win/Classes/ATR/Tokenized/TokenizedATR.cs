using System.Linq;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR.Tokenized
{
    public class TokenizedAtr : ObservableObject
    {
        private readonly Atr owner;
        private AtrChecksumToken atrChecksum;

        internal TokenizedAtr(Atr owner, byte[] atr)
        {
            this.owner = owner;
            AtrReadStream AtrStream = new AtrReadStream(atr);

            //Read preamble
            this.Preamble = new AtrPreambleToken(this,AtrStream);

            //read interface byte groups
            this.InterfaceByteGroups = new AtrInterfaceByteGroupTokenCollection(this);
            NextInterfaceBytesIndicator NextInterfaceBytesIndicator = this.Preamble.NextInterfaceBytesIndicator;
            while (NextInterfaceBytesIndicator != null)
            {
                AtrInterfaceByteGroupToken InterfaceByteGroup = new AtrInterfaceByteGroupToken(this, AtrStream, NextInterfaceBytesIndicator);
                this.InterfaceByteGroups.AppendGroup(InterfaceByteGroup);

                NextInterfaceBytesIndicator = NextInterfaceBytesIndicator.TdExists 
                    ? new NextInterfaceBytesIndicator(AtrStream.GetNextByte(), false) 
                    : null;
            }

            //Read and parse historical characters
            if( this.Preamble.NumberOfHistoricalCharacters > 0 )
            {
                byte[] HistoricalCharacters = AtrStream.GetNextBytes(this.Preamble.NumberOfHistoricalCharacters);
                this.HistoricalCharacters = new AtrHistoricalCharactersToken(this, HistoricalCharacters);
            }
            else
            {
                this.HistoricalCharacters = new AtrHistoricalCharactersToken(this, new byte[0]);
            }

            //Read checksum if needed
            if( this.ChecksumRequired )
            {
                this.atrChecksum = new AtrChecksumToken(AtrStream);
            }

            //Read additional bytes
            int AdditionalBytes = AtrStream.GetRemainingLength();
            if (AdditionalBytes > 0)
            {
                this.ExtraBytes = new AtrExtraBytesToken(AtrStream,AdditionalBytes);
            }
        }

        public AtrExtraBytesToken ExtraBytes { get; private set; }

        protected bool ChecksumRequired => (from InterfaceBytes in this.InterfaceByteGroups
            where InterfaceBytes.Type != InterfaceByteGroupType.Global &&
                  InterfaceBytes.Type != InterfaceByteGroupType.T0
            select InterfaceBytes).Any();

        public AtrPreambleToken Preamble { get; }
        public AtrInterfaceByteGroupTokenCollection InterfaceByteGroups { get; }
        public AtrHistoricalCharactersToken HistoricalCharacters { get; }

        public AtrChecksumToken AtrChecksum
        {
            get { return this.atrChecksum; }
            private set { this.SetAndInvoke(ref this.atrChecksum, value); }
        }

        internal void NotifyChanged()
        {
            if (this.ChecksumRequired)
            {
                AtrWriteStream AtrStream = new AtrWriteStream();

                AtrStream.WriteBytes(this.Preamble.Bytes);
                this.InterfaceByteGroups.ForEach(_=>AtrStream.WriteBytes(_.Bytes));
                AtrStream.WriteBytes(this.HistoricalCharacters.Bytes);

                this.AtrChecksum = new AtrChecksumToken(AtrStream.ToByteArray());
            }
            else
            {
                this.AtrChecksum = null;
            }
            this.owner.NotifyChanged();
        }

        public byte[] GetBytes()
        {
            AtrWriteStream AtrStream = new AtrWriteStream();

            AtrStream.WriteBytes(this.Preamble.Bytes);
            this.InterfaceByteGroups.ForEach(_=>AtrStream.WriteBytes(_.Bytes));
            AtrStream.WriteBytes(this.HistoricalCharacters.Bytes);
            if (this.ChecksumRequired)
            {
                AtrStream.WriteBytes(this.AtrChecksum.Bytes);
            }

            return AtrStream.ToByteArray();
        }
    }
}