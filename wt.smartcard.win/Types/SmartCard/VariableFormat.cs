namespace WhileTrue.Types.SmartCard
{
    /// <summary>
    /// Format of a <see cref="VariableCardCommand"/> used in <see cref="Variable"/>s
    /// </summary>
    public enum VariableFormat
    {
        /// <summary>Hexadecimal input (in the form '1234ABCD')</summary>
        Hexadecimal,
        /// <summary>Ascii input (in the form '1234') which is converted in its hexadecimal representation ('31323334')</summary>
        Ascii,
    }
}