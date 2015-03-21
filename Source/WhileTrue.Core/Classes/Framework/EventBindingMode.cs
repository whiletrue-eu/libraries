namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Specifies the way event handler shall be bound
    /// </summary>
    public enum EventBindingMode
    {
        /// <summary>
        /// Uses standard .Net event handlers
        /// </summary>
        Strong,
        /// <summary>
        /// Uses weak event handlers that allow garbage collection of the expression, see <see cref="WeakDelegate"/> for details
        /// </summary>
        Weak,
    }
}

