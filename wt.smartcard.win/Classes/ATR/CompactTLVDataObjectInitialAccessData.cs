using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class CompactTlvDataObjectInitialAccessData : CompactTlvDataObjectBase
    {
        /*
              ISO 7816-4 ch. 8.3.3 Initial access data
     This optional data object allows the retrieval of a string of data objects defined in ISO/IEC 7816.
     The string retrieved by this data object is called the "initial data string".
     This data object is introduced by '41','42' or '45'.
     Any command APDU described in this clause is assumed to be the first command sent after the answer
     to reset. Consequently, the data available at this point may not be subsequently retrievable.
     8.3.3.1 Length='1'
     When only one byte of information is provided, it indicates the length of the command to perform
     for retrieving the initial data string. The command to perform is a READ BINARY command structured
     as follows :
     Table 81 - Coding of the command when length='1'
     CLA         '00' (see 5.4.1)
     INS         'B0'
     P1-P2       '0000'
     Lc field    Empty
     Data field  Empty
     Le field    First and only byte of value field of initial access data (indicating the number of bytes to be read)
     8.3.3.2 Length='2'
     When two bytes of information are provided, the first byte indicates the file structure
     (transparent or record) and the short identifier of the EF to be read. The second byte indicates
     the length of the READ command to perform for retrieving data string.
     Table 82 - Structure of the file byte
     b8     = 0 Record oriented file
            = 1 Transparent file
     b7-6   '00' (other values are RFU)
     b5-1   Short EF identifier
     Table 83 - Coding of the command when b8=0
     CLA         '00' (see 5.4.1)
     INS         'B2'
     P1          '01'
     P2          Short EF identifier (from the first byte of initial access data) followed by b3-1='110'
     Lc field    Empty
     Data field  Empty
     Le field    Second and last byte of bvalue field of initial access data (indicating the number of
                 bytes to be read)
     Table 84 - Coding of the command when b8=1
     CLA         '00' (see 5.4.1)
     INS         'B0'
     P1          Value of the first byte on initial access data
     P2          '00'
     Lc field    Empty
     Data field  Empty
     Le field    Second and last byte of bvalue field of initial access data (indicating the number of
                 bytes to be read) 
     8.3.3.3 Length='5'
     The value found in the initial access data object consists of the APDU of a command to perform.
     When executed this command provides the initial data string in its response data field.
          
     ISO7816-4 6.1 READ BINARY
     6.1.1 Definition and scope
     The Read Binary response message gives (part of) the content of an EF with transparent structure. 
     6.1.2 Conditional usage and security
     When the command contains a valid short EF identifier, it sets the file as current EF. The command
     is processed on the currently selected EF.
     The command can be performed only if the security status satisfies the security attributes defined
     for this EF for the read function.
     The command shall be aborted if it is applied to an EF without transparent structure. 
     6.1.3 Command message
 
     Table 27 - READ BINARY command APDU
     CLA        As defined in 5.4.1
     INS        'B0'
     P1-P2      See text below
     Lc field   Empty
     Data field Empty
     Le field   Number of bytes to be read

     If bit8=1 in P1, then bit7-6 are set to 0. bit3-1 of P1 are a short EF (Elementary File) identifier
     and P2 is the offset of the first byte to be read in date units from the beginning of the file. 
     If bit8=0 in P1, then P1||P2 is the offset of the first byte to be read in data units from the
     beginning of the file. 
     6.1.4 Response message (nominal size)
     If the Le field contains only zeroes, then within the limit of 256 for short length or 65536 for
     extended length, all the bytes until the end of the file should be read. 
         * 
      ISO 7816-4 6.5 READ RECORD(S) command
6.5.1 Definition and scope
The READ RECORD(S) response message gives the contents of the specified record(s) (or the beginning
part of one record) of an EF. 
6.5.2 Conditional usage and security
The command can be performed only if the security status satisfies the security attributes for this
EF for the read function. 
If an EF is currently selected at the time of issuing the command, then this command may be
processed without identification of this file. 
When the command contains a valid short EF identifier, it sets the file as current EF and resets
the current record pointer. 
The command shall be aborted if applied to an EF without record structure. 
6.5.3 Command message

Table 35 - READ RECORD(S) command APDU
CLA        As defined in 5.4.1
INS        'B2'
P1         Record number or record identifier of the first record to be read ('00' indicates the
           current record)
P2         Reference control, according to table 36
Lc field   Empty
Data field Empty
Le field   Number of bytes to be read

Table 36 - Coding of the reference control P2
b8 b7 b6 b5 b4 b3 b2 b1  Meaning
 0  0  0  0  0 -- -- --  Currently selected EF
 x  x  x  x  x -- -- --  Short EF identifier
 1  1  1  1  1 -- -- --  RFU
-- -- -- -- --  1  x  x  Usage of record number in P1
-- -- -- -- --  1  0  0  - Read record P1
-- -- -- -- --  1  0  1  - Read all records from P1 up to the last
-- -- -- -- --  1  1  0  - Read all records from the last up to P1
-- -- -- -- --  1  1  1  - RFU
-- -- -- -- --  0  x  x  Usage of record identifier in P1
-- -- -- -- --  0  0  0  - Read first occurence
-- -- -- -- --  0  0  1  - Read last occurrence
-- -- -- -- --  0  1  0  - Read next occurrence
-- -- -- -- --  0  1  1  - Read previous occurrence
 */
        public CompactTlvDataObjectInitialAccessData(AtrCompactTlvHistoricalCharacters owner)
            : base(owner)
        {
        }
        private InitialAccessDataBase initialAccessData;

        protected override byte[] GetValue()
        {
            return this.initialAccessData.GetValue();
        }

        protected override void UpdateValue(byte[] data)
        {
            if (data != null)
            {
                this.IsApplicable = true;
                if( data.Length == 1 )
                {
                    this.InitialAccessData = new ReadBinary(data);
                }
                else if( data.Length == 2 )
                {
                    if ((data[0] & 0x80) == 0x80)
                    {//read binary
                        this.InitialAccessData = new ReadBinary(data);
                    }
                    else
                    {//read record
                        this.InitialAccessData = new ReadRecord(data);
                    }
                }
                else if( data.Length == 5 )
                {
                    this.InitialAccessData = new CustomApdu(data);
                }
                else
                {
                    throw new ArgumentException("Length must be 1, 2 or 5 bytes");
                }
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTlvTypes Type => CompactTlvTypes.InitialAccessData;

        protected override byte[] GetDefaultValue()
        {
            return new byte[] { 0x41,0x00 };
        }

        public InitialAccessDataBase InitialAccessData
        {
            get
            {
                return this.initialAccessData;
            }
            set
            {
                value.DbC_AssureNotNull();
                this.SetAndInvoke(ref this.initialAccessData, value);
                this.NotifyChanged();
            }
        }

        public class ReadBinary : InitialAccessDataBase
        {
            public ReadBinary(byte shortFileId, byte lengthToRead)
            {
                DbC.Assure((shortFileId & 0xF0) == 0x00, "Short file ID for read binary can only have values from 0 to 15");
                this.ShortFileId = shortFileId;
                this.LengthToRead = lengthToRead;
            }

            public ReadBinary(byte lengthToRead)
            {
                this.LengthToRead = lengthToRead;
                this.ShortFileId = null;
            }

            internal ReadBinary(byte[] data)
            {
                this.ShortFileId = data.Length == 2 ? (byte) (data[0] & 0x0F) : (byte?)null;
                this.LengthToRead = data[data.Length == 1 ? 0 : 1];
            }

            public byte? ShortFileId { get; }

            public byte LengthToRead { get; }

            public override byte[] GetApdu()
            {
                return new byte[] {0x00, 0xB0, (byte) (this.ShortFileId.HasValue ? (this.ShortFileId.Value & 0x80) : 0x00), 0x00, this.LengthToRead};
            }

            internal override byte[] GetValue()
            {
                if (this.ShortFileId.HasValue)
                {
                    return new[] {(byte) (0x80 & this.ShortFileId.Value), this.LengthToRead};
                }
                else
                {
                    return new[] {this.LengthToRead};
                }
            }
        }

        public class ReadRecord : InitialAccessDataBase
        {
            public ReadRecord(byte shortFileId, byte lengthToRead)
            {
                DbC.Assure((shortFileId & 0xE0) == 0x00, "Short file ID for read record can only have values from 0 to 31");
                this.ShortFileId = shortFileId;
                this.LengthToRead = lengthToRead;
            }

            internal ReadRecord(byte[] data)
                : this((byte)(data[0] & 0x1F), data[1])
            {
            }

            public override byte[] GetApdu()
            {
                return new byte[] { 0x00, 0xB2, 0x01, (byte)(((this.ShortFileId) << 3) | 0x06), this.LengthToRead };
            }

            internal override byte[] GetValue()
            {
                return new[]{this.ShortFileId, this.LengthToRead};
            }

            public byte ShortFileId { get; }

            public byte LengthToRead { get; }
        }

        public class CustomApdu: InitialAccessDataBase
        {
            public CustomApdu(byte[] apduBytes)
            {
                this.ApduBytes = apduBytes;
            }

            public override byte[] GetApdu()
            {
                return this.ApduBytes;
            }

            internal override byte[] GetValue()
            {
                return this.ApduBytes;
            }

            public byte[] ApduBytes { get; }
        }

        public abstract class InitialAccessDataBase
        {
            internal InitialAccessDataBase()
            {
            }
            public abstract byte[] GetApdu();
            internal abstract byte[] GetValue();
        }
    }
}