using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Types.SmartCard
{
    /// <summary>
    /// APDU/TPDU command that can be sent to the card
    /// </summary>
    public class CardCommand
    {
        /// <summary>
        /// Represents the value of <see cref="Le"/>, if LE is unset.
        /// </summary>
        public const short Unset = -1;

        private byte cla;
        private byte[] data = new byte[0];
        private byte ins;
        private byte? le;
        private byte p1;
        private byte p2;

        /// <summary>
        /// Constructs an empty card command
        /// </summary>
        /// <remarks>
        /// All properties will be set to '0x00'
        /// </remarks>
        public CardCommand()
        {
        }

        /// <summary>
        /// Constructs an empty card command
        /// </summary>
        /// <remarks>
        /// All properties will be set to '0x00'
        /// </remarks>
        public CardCommand(byte cla, byte ins, byte p1, byte p2, byte[] data)
        {
            this.cla = cla;
            this.ins = ins;
            this.p1 = p1;
            this.p2 = p2;
            this.data = data;
        }

        /// <summary>
        /// Constructs an empty card command
        /// </summary>
        /// <remarks>
        /// All properties will be set to '0x00'
        /// </remarks>
        public CardCommand(byte cla, byte ins, byte p1, byte p2)
        {
            this.cla = cla;
            this.ins = ins;
            this.p1 = p1;
            this.p2 = p2;
            this.le = 0x00;
        }

        /// <summary>
        /// Constructs an empty card command
        /// </summary>
        /// <remarks>
        /// All properties will be set to '0x00'
        /// </remarks>
        public CardCommand(byte cla, byte ins, byte p1, byte p2, byte le)
        {
            this.cla = cla;
            this.ins = ins;
            this.p1 = p1;
            this.p2 = p2;
            this.le = le;
        }

        /// <summary>
        /// Creates a card command by paring the APDU/TPDU command given.
        /// </summary>
        /// <param name="data">byte array that contains the APDU/TPDU command</param>
        /// <exception cref="ArgumentException">If data does not conatin a valid APDU/TPDU</exception>
        public CardCommand(byte[] data)
        {
            this.Deserialize(data);
        }


        /// <summary>
        /// Gets/sets the CLA byte of the command
        /// </summary>
        public virtual byte Cla
        {
            get { return this.cla; }
            set { this.cla = value; }
        }

        /// <summary>
        /// Gets/sets the INS byte of the command
        /// </summary>
        public virtual byte Ins
        {
            get { return this.ins; }
            set { this.ins = value; }
        }

        /// <summary>
        /// Gets/sets the P1 byte of the command
        /// </summary>
        public virtual byte P1
        {
            get { return this.p1; }
            set { this.p1 = value; }
        }

        /// <summary>
        /// Gets/sets the P2 byte of the command
        /// </summary>
        public virtual byte P2
        {
            get { return this.p2; }
            set { this.p2 = value; }
        }

        /// <summary>
        /// Gets/sets the Data of the command.
        /// </summary>
        /// <remarks>
        /// The LC byte is automatically generated from the length of the data
        /// </remarks>
        public virtual byte[] Data
        {
            get { return (byte[]) this.data.Clone(); }
            set { this.data = (byte[]) value.Clone(); }
        }

        /// <summary>
        /// Gets/sets the LE byte of the command.
        /// </summary>
        /// <remarks>
        /// If LE is not set, an exception is thrown.
        /// </remarks>
        /// <remarks>
        /// If LE is not set, no LE byte will be sent to the card
        /// </remarks>
        public virtual byte? Le
        {
            get { return this.le; }
            set { this.le = value; }
        }

        /// <summary>
        /// Appends the given byte array to the end of the data currently set
        /// </summary>
        /// <remarks>
        /// The LC byte is automatically generated from the length of the comlete data
        /// </remarks>
        /// <param name="data">data to append</param>
        public void AppendData(byte[] data)
        {
            byte[] Data = new byte[this.data.Length + data.Length];
            Array.Copy(this.data, 0, Data, 0, this.data.Length);
            Array.Copy(data, 0, Data, this.data.Length, data.Length);
            this.data = Data;
        }

        /// <summary>
        /// Appends the given byte to the end of the data currently set
        /// </summary>
        /// <remarks>
        /// The LC byte is automatically generated from the length of the comlete data
        /// </remarks>
        /// <param name="data">data to append</param>
        public void AppendData(byte data)
        {
            byte[] Data = new byte[this.data.Length + 1];
            Array.Copy(this.data, 0, Data, 0, this.data.Length);
            Data[this.data.Length] = data;
            this.data = Data;
        }

        /// <summary>
        /// Appends the given short value to the end of the data currently set
        /// </summary>
        /// <remarks>
        /// The LC byte is automatically generated from the length of the comlete data.
        /// The data is appended big-endian.
        /// </remarks>
        /// <param name="data">data to append</param>
        public void AppendData(ushort data)
        {
            byte[] Data = new byte[this.data.Length + 2];
            Array.Copy(this.data, 0, Data, 0, this.data.Length);
            Data[this.data.Length] = (byte) ((data & 0xFF00) >> 8);
            Data[this.data.Length + 1] = (byte) (data & 0x00FF);
            this.data = Data;
        }

        /// <summary>
        /// Appends a TLV object of the given tag and data to the end of the data currently set
        /// </summary>
        /// <remarks>
        /// The LC byte is automatically generated from the length of the comlete data.
        /// The length of the TLV object is taken from the length of the data (one byte coding).
        /// </remarks>
        /// <param name="tag">tag of the TLV object to append</param>
        /// <param name="data">data of the TLV object to append</param>
        public void AppendData(byte tag, byte[] data)
        {
            this.AppendData(tag);
            this.AppendData((byte) data.Length);
            this.AppendData(data);
        }

        /// <summary>
        /// Appends a TLV object of the given tag and data to the end of the data currently set
        /// </summary>
        /// <remarks>
        /// The LC byte is automatically generated from the length of the comlete data.
        /// The length of the TLV object is '1' (one byte coding).
        /// </remarks>
        /// <param name="tag">tag of the TLV object to append</param>
        /// <param name="data">data of the TLV object to append</param>
        public void AppendData(byte tag, byte data)
        {
            this.AppendData(tag);
            this.AppendData(0x01);
            this.AppendData(data);
        }

        /// <summary>
        /// Appends a TLV object of the given tag and data to the end of the data currently set
        /// </summary>
        /// <remarks>
        /// The LC byte is automatically generated from the length of the comlete data.
        /// The length of the TLV object is '2' (one byte coding).
        /// The data is appended big-endian.
        /// </remarks>
        /// <param name="tag">tag of the TLV object to append</param>
        /// <param name="data">data of the TLV object to append</param>
        public void AppendData(byte tag, ushort data)
        {
            this.AppendData(tag);
            this.AppendData(0x02);
            this.AppendData(data);
        }

        /// <summary>
        /// Serialises the command to a byte array
        /// </summary>
        /// <returns>byte array that conatins the command</returns>
        /// <remarks>Case 4 commands are generated if applicable</remarks>
        public byte[] Serialize()
        {
            return this.Serialize(true);
        }

        /// <summary>
        /// Serialises the command to a byte array
        /// </summary>
        /// <param name="supportsCase4">states, if case 4 commands may be generated or not. If not, the LE byte is ignored (T=0 protocol behaviour)</param>
        /// <returns>byte array that conatins the command</returns>
        public virtual byte[] Serialize(bool supportsCase4)
        {
            byte[] Data;
            if (this.data.Length == 0)
            {
                Data = new byte[5];
            }
            else
            {
                Data = new byte[5 + this.data.Length + (supportsCase4 && this.Le.HasValue ? 1 : 0)];
            }

            Data[0] = this.cla;
            Data[1] = this.ins;
            Data[2] = this.p1;
            Data[3] = this.p2;
            if (this.data.Length == 0)
            {
                if (this.Le.HasValue)
                {
                    Data[4] = this.le.Value;
                }
                else
                {
                    Data[4] = 0;
                }
            }
            else
            {
                Data[4] = (byte) this.data.Length;
                Array.Copy(this.data, 0, Data, 5, this.data.Length);
                if (supportsCase4 && this.Le.HasValue)
                {
                    Data[5 + this.data.Length] = this.le.Value;
                }
                else
                {
                    //le is ignored
                }
            }

            return Data;
        }

        private void Deserialize(byte[] data)
        {
            if (data.Length < 5)
            {
                throw new ArgumentException("APDU command must consist of at least 5 bytes");
            }

            this.cla = data[0];
            this.ins = data[1];
            this.p1 = data[2];
            this.p2 = data[3];
            if (data.Length > 5)
            {
                if (data.Length > 4 + data[4])
                {
                    this.data = new byte[data[4]];
                    Array.Copy(data, 5, this.data, 0, data[4]);
                }
                else
                {
                    throw new ArgumentException("APDU command contains an invalid lc byte or data missing");
                }
                if (data.Length > 5 + data[4])
                {
                    if (data.Length == 6 + data[4])
                    {
                        this.le = data[5 + data[4]];
                    }
                    else
                    {
                        throw new ArgumentException("APDU command contains data bytes after le");
                    }
                }
            }
            else
            {
                this.data = new byte[0];
                this.le = data[4];
            }
        }


        /// <summary>
        /// The string represenation of the command (in hexadecimal notation, using 'Case 4' of applicable)
        /// </summary>
        public override string ToString()
        {
            return Conversion.ToHexString(this.Serialize());
        }
    }
}