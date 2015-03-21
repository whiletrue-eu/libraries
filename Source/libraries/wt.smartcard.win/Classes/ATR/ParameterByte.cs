using System;

namespace WhileTrue.Classes.ATR
{
    public class ParameterByte
    {
        private readonly byte value;

        public ParameterByte(byte value)
        {
            this.value = value;
            this.HasValue=ValueIndicator.Set;
        }

        public ParameterByte(ValueIndicator valueIndicator)
        {
            this.HasValue = valueIndicator;
        }

        public enum ValueIndicator
        {
            Set,
            Unset,
            Irrelevant
        }
        public ValueIndicator HasValue { get; }

        public byte Value
        {
            get
            {
                if (this.HasValue == ValueIndicator.Set)
                {
                    return this.value;
                }
                else
                {
                    throw new InvalidOperationException("value is not set");
                }
            }
        }

        public static implicit operator ParameterByte(byte? value)
        {
            return value.HasValue? new ParameterByte(value.Value):new ParameterByte(ValueIndicator.Unset);
        }
    }
}