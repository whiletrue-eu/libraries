namespace WhileTrue.Classes.UnitTesting
{
    /// <summary>
    /// Index which is automatically increased every time it is accessed. Useful for unit tests. Index starts at 0.
    /// </summary>
    public class AutoIndex
    {
        private int index;
        /// <summary/>
        public static implicit operator int(AutoIndex value)
        {
            int Index = value.index;
            value.index++;
            return Index;
        }
    }
}