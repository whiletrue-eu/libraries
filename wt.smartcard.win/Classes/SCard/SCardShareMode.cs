namespace WhileTrue.Classes.SCard
{
    public enum SCardShareMode
    {
        /// <summary>
        /// This application is not willing to share this card with other applications.
        /// </summary>
        Exclusive = 1,
        /// <summary>
        /// This application is willing to share this card with other applications.
        /// </summary>
        Shared = 2,
        /// <summary>
        /// This application demands direct control of the reader, so it is not available to other applications.
        /// </summary>
        Direct = 3,
    }
}