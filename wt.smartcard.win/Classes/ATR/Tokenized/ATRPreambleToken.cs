using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR.Tokenized
{
    public class AtrPreambleToken : ObservableObject, IAtrToken
    {
        private readonly TokenizedAtr owner;
        private byte numberOfHistoricalCharacters;

        internal AtrPreambleToken(TokenizedAtr owner, AtrReadStream atr)
        {
            this.owner = owner;
            /* Initial character TS
    The initial character TS encodes the convention used for encoding of the ATR (and further communications until the next reset).
    In direct [resp. inverse] convention, bits with logic value ‘1’ are transferred as a High voltage (H) [resp. a Low voltage (L)];
    bits with logic value ‘0’ are transferred as L [resp. H]; and least-significant bit of each data byte is first (resp. last) in the
    physical transmission by the card.
    For  direct  convention, TS is (H) L H H L H H H L L H (H) and encodes the byte ‘3B’.
    For inverse convention, TS is (H) L H H L L L L L L H (H) and encodes the byte ‘3F’.
    [ (H) represents the idle (High, Mark) state of the I/O line. The 8 data bits are shown in italic.]
    In the following bytes of the ATR, bits are numbered 1st to 8th from low-order to high-order, and their value noted 0 or 1, 
    regardless of the chronological order and electrical representation, defined by TS.
    TS also allows the card reader to confirm or determine the duration of bits, denoted Elementary Time Unit (ETU),
    as one third of the delay between the first and second H-to-L transition in TS. This is optional, and the principal definition of 
    ETU in the ATR of standard-compliant asynchronous Smart Cards is 372 periods of the clock received by the card.
    Historical note: provision for cards that use an internal clock source and a fixed ETU of 1/9600 second during ATR existed in ISO/IEC 7816-3:1989,
    and was removed from the 1997 edition onwards.
 */
            byte Ts = atr.GetNextByte();
            switch (Ts)
            {
                case 0x3B:
                    this.CodingConvention = CodingConvention.Direct;
                    break;
                case 0x3F:
                    this.CodingConvention = CodingConvention.Inverse;
                    break;
                default:
                    throw new InvalidAtrCodingException("TS character is invalid");
            }

            /* Format byte T0
    The Format byte T0 encodes in its 4 low-order bits the number Y of historical bytes, in range [0..15].
    It also encodes in its 4 high-order bits the presence of at most 4 other interface bytes: TA1 (resp. TB1, TC1, TD1)
    follow, in that order, if the 5th (resp. 6th, 7th, 8th) bit of T0 is 1.
 */
            byte T0 = atr.GetNextByte();
            this.numberOfHistoricalCharacters = T0.GetLoNibble();
            this.NextInterfaceBytesIndicator = new NextInterfaceBytesIndicator(T0,true);
            this.NextInterfaceBytesIndicator.PropertyChanged += this.interfaceBytesIndicatorForNextGroup_PropertyChanged;
        }

        private void interfaceBytesIndicatorForNextGroup_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.InvokePropertyChanged(nameof(AtrPreambleToken.Bytes));
        }

        public byte[] Bytes => new[] {(byte)(this.CodingConvention==CodingConvention.Direct?0x3B:0x3F), this.NextInterfaceBytesIndicator.GetAsByte(this.NumberOfHistoricalCharacters) };

        public NextInterfaceBytesIndicator NextInterfaceBytesIndicator
        {
            get; }

        public CodingConvention CodingConvention { get; }

        public byte NumberOfHistoricalCharacters
        {
            get { return this.numberOfHistoricalCharacters; }
            internal set
            {
                this.SetAndInvoke( ref this.numberOfHistoricalCharacters, value, delegate
                    {
                        this.InvokePropertyChanged(nameof(AtrPreambleToken.Bytes));
                        this.owner.NotifyChanged();
                    });
            }
        }
    }
}