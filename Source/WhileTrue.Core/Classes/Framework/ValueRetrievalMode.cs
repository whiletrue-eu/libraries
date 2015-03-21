namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Defines when the value is retrieved
    /// </summary>
    public enum ValueRetrievalMode
    {
        /// <summary>
        /// Value is retrieved on creation and every time the changed event is called. Value is cached
        /// </summary>
        Immediately,
        /// <summary>
        /// Value is retrieved when it is queried by the user. Value is cached
        /// </summary>
        Lazy,
        /// <summary>
        /// Value is retrieved everytime when queried by the user. Value is not cached.
        /// </summary>
        OnDemand,
    }
}