using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class CompactTlvDataObjectIssuerIdentificationNumber : CompactTlvDataObjectBase
    {
        private string issuerIdentificationNumber;
        /*
              ISO 7816-4 ch. 8.3.1 Country/issuer indicator
     When present this data object denotes a country or an issuer.
     This data object is introduced by either '1Y' or '2Y'.
     Table 79 - Coding of the country/issuer indicator
     Tag Length    Value
     '1' variable  Country code and national date
     '2' variable  Issuer identification number
     The tag '1' is followed by the appropriate length (1 nibble) and by three digits denoting the
     country as defined in ISO 3166. Data which follows (odd number of nibbles) is chosen by the
     relevant national standardization body.
     The tag '2' is followed by the appropriate length (1 nibble) and by the issuer identification
     number as defined in part 1 of ISO/IEC 7812. If the issuer identification number contains an odd
     number of digits, then it shall be right padded with a nibble valued 'F'.
*/
        public CompactTlvDataObjectIssuerIdentificationNumber(AtrCompactTlvHistoricalCharacters owner) : base(owner)
        {
        }


        protected override byte[] GetValue()
        {
            if (this.issuerIdentificationNumber.Length%2 == 1)
            {
                return (this.issuerIdentificationNumber+"F").ToByteArray();
            }
            else
            {
                return this.issuerIdentificationNumber.ToByteArray();
            }
        }

        protected override void UpdateValue(byte[] data)
        {
            if (data != null)
            {
                this.IsApplicable = true;
                this.IssuerIdentificationNumber = data.ToHexString().TrimEnd('F');
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTlvTypes Type => CompactTlvTypes.IssuerIdentificationNumber;

        protected override byte[] GetDefaultValue()
        {
            return new byte[] { 0x20 };
        }

        public string IssuerIdentificationNumber
        {
            get { return this.issuerIdentificationNumber; }
            set
            {
                if (value.ToUpper().Trim('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F').Length == 0)
                {
                    if (value.Length < 30)
                    {
                        this.SetAndInvoke(ref this.issuerIdentificationNumber, value.ToUpper());
                        this.NotifyChanged();
                    }
                    else
                    {
                        throw new ArgumentException("identification number must have max. 30 digits", nameof(value));
                    }
                }
                else
                {
                    throw new ArgumentException("only hex digits are allowed", nameof(value));
                }
            }
        }
    }
}