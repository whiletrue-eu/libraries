using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Types.SmartCard
{
    /// <summary>
    /// APDU/TPDU response that was received from the card
    /// </summary>
    public class CardResponse
    {
        private byte[] data = new byte[0];
        private byte sw1;
        private byte sw2;

        /// <summary>
        /// Creates a card command by paring the APDU/TPDU command given.
        /// </summary>
        /// <param name="data">byte array that contains the APDU/TPDU command</param>
        /// <exception cref="ArgumentException">If data does not conatin a valid APDU/TPDU</exception>
        public CardResponse(byte[] data)
        {
            this.Deserialize(data);
        }

        public CardResponse(byte sw1, byte sw2, byte[] data)
        {
            this.sw1 = sw1;
            this.sw2 = sw2;
            this.data = data;
        }

        /// <summary>
        /// Gets the status word (SW1,SW2) returned by the card
        /// </summary>
        public ushort Status
        {
            get { return (ushort) ((this.sw1 << 8) | this.sw2); }
        }

        /// <summary>
        /// Gets the status SW1 returned by the card
        /// </summary>
        public byte SW1
        {
            get { return this.sw1; }
        }

        /// <summary>
        /// Gets the status SW2 returned by the card
        /// </summary>
        public byte SW2
        {
            get { return this.sw2; }
        }

        /// <summary>
        /// Gets the data returned by the card
        /// </summary>
        public byte[] Data
        {
            get { return (byte[]) this.data.Clone(); }
        }

        public bool DataAvailable
        {
            get { return this.data.Length > 0; }
        }

        private void Deserialize(byte[] data)
        {
            if (data.Length < 2)
            {
                throw new ArgumentException("response must at least contain two bytes (sw1, sw2)");
            }

            this.data = new byte[data.Length - 2];
            Array.Copy(data, 0, this.data, 0, data.Length - 2);
            this.sw1 = data[data.Length - 2];
            this.sw2 = data[data.Length - 1];
        }

        public byte[] Serialize()
        {
            byte[] Data = new byte[2 + this.data.Length];

            Array.Copy(this.data, 0, Data, 0, this.data.Length);
            Data[this.data.Length] = this.sw1;
            Data[this.data.Length + 1] = this.sw2;

            return Data;
        }

        public override string ToString()
        {
            return Conversion.ToHexString(this.Serialize());
        }
    }
}