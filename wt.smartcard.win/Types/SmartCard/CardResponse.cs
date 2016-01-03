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
            this.Sw1 = sw1;
            this.Sw2 = sw2;
            this.data = data;
        }

        /// <summary>
        /// Gets the status word (SW1,SW2) returned by the card
        /// </summary>
        public ushort Status => (ushort) ((this.Sw1 << 8) | this.Sw2);

        /// <summary>
        /// Gets the status SW1 returned by the card
        /// </summary>
        public byte Sw1 { get; private set; }

        /// <summary>
        /// Gets the status SW2 returned by the card
        /// </summary>
        public byte Sw2 { get; private set; }

        /// <summary>
        /// Gets the data returned by the card
        /// </summary>
        public byte[] Data => (byte[]) this.data.Clone();

        public bool DataAvailable => this.data.Length > 0;

        private void Deserialize(byte[] data)
        {
            if (data.Length < 2)
            {
                throw new ArgumentException("response must at least contain two bytes (sw1, sw2)");
            }

            this.data = new byte[data.Length - 2];
            Array.Copy(data, 0, this.data, 0, data.Length - 2);
            this.Sw1 = data[data.Length - 2];
            this.Sw2 = data[data.Length - 1];
        }

        public byte[] Serialize()
        {
            byte[] Data = new byte[2 + this.data.Length];

            Array.Copy(this.data, 0, Data, 0, this.data.Length);
            Data[this.data.Length] = this.Sw1;
            Data[this.data.Length + 1] = this.Sw2;

            return Data;
        }

        public override string ToString()
        {
            return Conversion.ToHexString(this.Serialize());
        }
    }
}