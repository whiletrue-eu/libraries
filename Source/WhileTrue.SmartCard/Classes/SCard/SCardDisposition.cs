namespace WhileTrue.Classes.SCard
{
    public enum SCardDisposition
    {
        /// <summary>
        /// Don't do anything special on close
        /// </summary>
        Leave = 0,
        /// <summary>
        /// Reset the card on close
        /// </summary>
        Reset = 1,
        /// <summary>
        /// Power down the card on close
        /// </summary>
        Unpower = 2,
        /// <summary>
        /// Eject the card on close
        /// </summary>
        Eject = 3,
    }
}