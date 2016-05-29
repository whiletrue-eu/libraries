namespace WhileTrue.Controls
{
    /// <summary>
    /// Controls what happens if the window is closed
    /// </summary>
    public enum WindowCloseBehaviour
    {
        /// <summary>
        /// Standard close behvaiour
        /// </summary>
        Close,
        /// <summary>
        /// Window is only hidden but not closed
        /// </summary>
        Hide,
        /// <summary>
        /// Fade-Out event is triggered, so that an animation can be shown
        /// </summary>
        FadeOut
    }
}