using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class CompactTlvDataObjectCountryCode : CompactTlvDataObjectBase
    {
        private string nationalDate;
        private Country country;
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
        public CompactTlvDataObjectCountryCode(AtrCompactTlvHistoricalCharacters owner):base(owner)
        {
            
        }

        protected override byte[] GetValue()
        {
            return (this.Country.NumberCode+this.NationalDate).ToByteArray();
        }

        protected override void UpdateValue(byte[] data)
        {
            if (data != null)
            {
                this.IsApplicable = true;
                if (data.Length >= 2)
                {
                    string Value = data.ToHexString();
                    string CountryCode = Value.Substring(0, 3);
                    string NationalDate = Value.Substring(3);

                    this.Country = Country.GetFromNumberCode(CountryCode);
                    this.NationalDate = NationalDate;
                }
                else
                {
                    throw new ArgumentException("Country code must have 3 digits (at least 2 bytes data)");
                }
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTlvTypes Type => CompactTlvTypes.CountryCode;

        protected override byte[] GetDefaultValue()
        {
            return new byte[] { 0x12,0x00,0x00 };
        }

        public Country Country
        {
            get { return this.country; }
            set
            {
                this.SetAndInvoke(ref this.country, value); 
                this.NotifyChanged();
            }
        }

        public string NationalDate
        {
            get { return this.nationalDate; }
            set
            {
                if (value.ToUpper().Trim('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F').Length == 0)
                {
                    if (value.Length < 30)
                    {
                        if (value.Length%2 == 1)
                        {
                            this.SetAndInvoke(ref this.nationalDate, value.ToUpper());
                            this.NotifyChanged();
                        }
                        else
                        {
                            throw new ArgumentException("national date must have an odd length", nameof(value));
                        }
                    }
                    else
                    {
                        throw new ArgumentException("national date must have max. 27 digits", nameof(value));
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