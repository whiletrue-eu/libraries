using System;

namespace WhileTrue.Types.SmartCard
{
    /// <summary>
    /// Represents variable data that can be used in <see cref="VariableCardCommand"/>s.
    /// </summary>
    public class Variable
    {
        private byte[] value;

        /// <summary>
        /// Constructs a variable with the given properties.
        /// </summary>
        internal Variable(string name, byte offset, byte minLength, byte length, VariableFormat format, byte padding, bool verifyEntry)
        {
            this.Name = name;
            this.Offset = offset;
            this.MinLength = minLength;
            this.Length = length;
            this.Format = format;
            this.Padding = padding;
            this.VerifyEntry = verifyEntry;
        }

        /// <summary>
        /// Gets the name of the variable
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the offset of the variable in the data portion of the command
        /// </summary>
        public byte Offset { get; }

        /// <summary>
        /// Gets the minimal length of the variable. The rest of the data is filled with the <see cref="Padding"/> byte
        /// </summary>
        /// <remarks>must be less or equal to <see cref="Length"/></remarks>
        public byte MinLength { get; }

        /// <summary>
        /// Gets the length of the variable.
        /// </summary>
        /// <remarks>must be greater or equal to <see cref="MinLength"/></remarks>
        public byte Length { get; }

        /// <summary>
        /// Gets the padding byte that is used, if <see cref="MinLength"/> is less that <see cref="Length"/>
        /// </summary>
        public byte Padding { get; }

        /// <summary>
        /// Gets the format of the variable
        /// </summary>
        public VariableFormat Format { get; }

        /// <summary>
        /// Gets, whether the input of the variable data (while resolving the variable) shall be validated by 'double input'
        /// </summary>
        public bool VerifyEntry { get; }

        /// <summary>
        /// Gets, whether the variable was resolved by setting its <see cref="Value"/>
        /// </summary>
        public bool IsResolved => this.value != null;

        /// <summary>
        /// Gets/sets the Value of the variable.
        /// </summary>
        /// <remarks>
        /// The values length must be between <see cref="MinLength"/> and <see cref="Length"/>.
        /// The value will be padded automatically with the <see cref="Padding"/> byte.
        /// </remarks>
        public byte[] Value
        {
            get { return this.value; }
            set
            {
                if (value.Length < this.MinLength || value.Length > this.Length)
                {
                    throw new ArgumentException("Pin value must have a length between MinLength and Length");
                }

                //Do padding
                byte[] Variable = new byte[this.Length];
                for (int Index = 0; Index < Variable.Length; Index++)
                {
                    Variable[Index] = this.Padding;
                }

                Array.Copy(value, 0, Variable, 0, value.Length);

                this.value = Variable;
            }
        }
    }
}