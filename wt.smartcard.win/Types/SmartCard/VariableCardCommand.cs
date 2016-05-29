// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Types.SmartCard
{
    /// <summary>
    /// <see cref="CardCommand"/> that allows parts of the data to have a variable value
    /// </summary>
    public class VariableCardCommand : CardCommand
    {
        /// <summary>
        /// Constructs an empty card command
        /// </summary>
        /// <remarks>
        /// All properties will be set to '0x00'
        /// </remarks>
        public VariableCardCommand()
        {
        }

        /// <summary>
        /// Creates a card command by paring the APDU/TPDU command given.
        /// </summary>
        /// <param name="data">byte array that contains the APDU/TPDU command</param>
        /// <exception cref="ArgumentException">If data does not conatin a valid APDU/TPDU</exception>
        public VariableCardCommand(byte[] data)
            : base(data)
        {
        }

        /// <summary>
        /// Gets/sets the Data of the command.
        /// </summary>
        /// <remarks>
        /// The LC byte is automatically generated from the length of the data
        /// If read while variables are still unresolved, the variable data fields will have a value of '0x00'
        /// </remarks>
        public override byte[] Data
        {
            get
            {
                byte[] ApduData = base.Data;
                foreach (Variable Variable in this.Variables)
                {
                    if (Variable.IsResolved)
                    {
                        Array.Copy(Variable.Value, 0, ApduData, 5 + Variable.Offset, Variable.Length);
                    }
                    else
                    {
                        throw new UnresolvedVariableException(this, Variable);
                    }
                }
                return ApduData;
            }
            set
            {
                base.Data = value;
                this.Variables.Clear();
            }
        }

        /// <summary>
        /// Gets the variables used in the command in the order they were appended
        /// </summary>
        public VariableCollection Variables { get; } = new VariableCollection();

        /// <summary>
        /// Appends variable data with the given properties to the data currently set
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="minLength">Minimal length of the avriable</param>
        /// <param name="length">Length of the variable</param>
        /// <param name="format">Format of the variable</param>
        /// <param name="padding">Padding that is used, if minLength is less than length</param>
        /// <param name="verifyEntry">States, whether the input of the variable data (while resolving the variable) shall be validated by 'double input'</param>
        public void AppendVariableData(string name, byte minLength, byte length, VariableFormat format, byte padding, bool verifyEntry)
        {
            this.Variables.Add(new Variable(name, (byte) this.Data.Length, minLength, length, format, padding, verifyEntry));
            this.AppendData(new byte[length]);
        }

        /// <summary>
        /// Serialises the command to a byte array
        /// </summary>
        /// <param name="supportsCase4">states, if case 4 commands may be generated or not. If not, the LE byte is ignored (T=0 protocol behaviour)</param>
        /// <returns>byte array that conatins the command</returns>
        /// <exception cref="UnresolvedVariableException">Thrown, if a variable was not resolved before by setting its value.</exception>
        public override byte[] Serialize(bool supportsCase4)
        {
            byte[] ApduData = base.Serialize(supportsCase4);
            foreach (Variable Variable in this.Variables)
            {
                if (Variable.IsResolved)
                {
                    Array.Copy(Variable.Value, 0, ApduData, 5 + Variable.Offset, Variable.Length);
                }
                else
                {
                    throw new UnresolvedVariableException(this, Variable);
                }
            }
            return ApduData;
        }

        /// <summary>
        /// The string represenation of the command (in hexadecimal notation, using 'Case 4' of applicable). Variable values that are not resolved yet are replaced with '??'.
        /// </summary>
        public override string ToString()
        {
            byte[] Data = base.Serialize(true);
            foreach (Variable Variable in this.Variables)
            {
                if (Variable.IsResolved)
                {
                    Array.Copy(Variable.Value, 0, Data, 5 + Variable.Offset, Variable.Length);
                }
            }

            string HexData = Data.ToHexString();

            foreach (Variable Variable in this.Variables)
            {
                if (! Variable.IsResolved)
                {
                    HexData = HexData.Substring(0, (5 + Variable.Offset)*2) + new String('?', Variable.Length*2) + HexData.Substring((5 + Variable.Offset + Variable.Length)*2);
                }
            }
            return HexData;
        }
    }
}