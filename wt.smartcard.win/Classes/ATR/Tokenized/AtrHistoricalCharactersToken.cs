using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR.Tokenized
{
    /* ISO 7816-4, ch. 8 Historical bytes
     8.1 Pupose and general strucutre
     The historical bytes tell the outside world how to use the card when the transport protocol is
     ascertained according to part 3 of ISO/IEC 7816.
     The number of historical bytes (at most 15 bytes) is specified and coded as defined in part 3 of
     ISO/IEC 7816.
     The information carried by the historical bytes may also be found in an ATR file (default EF
     identifier='2F01').
     If present, the historical bytes are made up of three fields :
      * a mandatory category indicator (1 byte)
      * optional COMPACT-TLV data objects
      * a conditional status indicator (3 bytes)
     8.2 Category indicator (mandatory)
     The category indicator is the first historical byte. If the category indicator is equal to '00',
     '10' or '8X', then the format of the historical bytes shall be according to this part of ISO/IEC
     7816.
     Table 78 - Coding of the category indicator
        Value Meaning
        '00' Status information shall be present at the end of the historical bytes (not in TLV).
        '10' Specified in 8.5
        '80' Status information if present is contained in an optional COMPACT-TLV data object.
        '81'-'8F' RFU
        Other values Proprietary

     8.5 DIR data reference
     If the category indicator is '10', then the following byte is the DIR data reference. The coding
     and meaning of this byte are outside the scope of this part of the ISO/IEC 7816.
     */
    public class AtrHistoricalCharactersToken : ObservableObject,IAtrToken
    {
        private readonly TokenizedAtr owner;
        private byte[] historicalCharacters;

        internal AtrHistoricalCharactersToken(TokenizedAtr owner, byte[] historicalCharacters)
        {
            this.owner = owner;
            this.historicalCharacters = historicalCharacters;

        }

        public byte[] HistoricalCharacters
        {
            get { return this.historicalCharacters; }
            set { 
                DbC.AssureArgumentInRange(value, "value", value.Length<=15, "Historical Characters can be max. 15 bytes in length");
                this.SetAndInvoke(ref this.historicalCharacters, value, delegate
                {
                    this.owner.Preamble.NumberOfHistoricalCharacters = (byte)value.Length;
                    this.InvokePropertyChanged(nameof(AtrHistoricalCharactersToken.Bytes));
                    this.owner.NotifyChanged();
                }
                ); }
        }

        public byte[] Bytes => (byte[]) this.historicalCharacters.Clone();
    }
}