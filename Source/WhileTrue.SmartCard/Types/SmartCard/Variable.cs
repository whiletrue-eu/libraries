using System;

namespace WhileTrue.Types.SmartCard
{
    /// <summary>
    /// Represents variable data that can be used in <see cref="VariableCardCommand"/>s.
    /// </summary>
    public class Variable
    {
        private readonly VariableFormat format;
        private readonly byte length;
        private readonly byte minLength;
        private readonly string name;
        private readonly byte offset;
        private readonly byte padding;
        private readonly bool verifyEntry;
        private byte[] value;

        /// <summary>
        /// Constructs a variable with the given properties.
        /// </summary>
        internal Variable(string name, byte offset, byte minLength, byte length, VariableFormat format, byte padding, bool verifyEntry)
        {
            this.name = name;
            this.offset = offset;
            this.minLength = minLength;
            this.length = length;
            this.format = format;
            this.padding = padding;
            this.verifyEntry = verifyEntry;
        }

        /// <summary>
        /// Gets the name of the variable
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the offset of the variable in the data portion of the command
        /// </summary>
        public byte Offset
        {
            get { return this.offset; }
        }

        /// <summary>
        /// Gets the minimal length of the variable. The rest of the data is filled with the <see cref="Padding"/> byte
        /// </summary>
        /// <remarks>must be less or equal to <see cref="Length"/></remarks>
        public byte MinLength
        {
            get { return this.minLength; }
        }

        /// <summary>
        /// Gets the length of the variable.
        /// </summary>
        /// <remarks>must be greater or equal to <see cref="MinLength"/></remarks>
        public byte Length
        {
            get { return this.length; }
        }

        /// <summary>
        /// Gets the padding byte that is used, if <see cref="MinLength"/> is less that <see cref="Length"/>
        /// </summary>
        public byte Padding
        {
            get { return this.padding; }
        }

        /// <summary>
        /// Gets the format of the variable
        /// </summary>
        public VariableFormat Format
        {
            get { return this.format; }
        }

        /// <summary>
        /// Gets, whether the input of the variable data (while resolving the variable) shall be validated by 'double input'
        /// </summary>
        public bool VerifyEntry
        {
            get { return this.verifyEntry; }
        }

        /// <summary>
        /// Gets, whether the variable was resolved by setting its <see cref="Value"/>
        /// </summary>
        public bool IsResolved
        {
            get { return this.value != null; }
        }

        /// <summary>
        /// Gets/sets the Value of the variable.
        /// </summary>
        /// <remarks>
        /// The values length must be between <see cref="MinLength"/> and <see cref="Length"/>.
        /// The value will be padded automatically with the <see cref="padding"/> byte.
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