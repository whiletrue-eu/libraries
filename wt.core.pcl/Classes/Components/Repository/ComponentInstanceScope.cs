namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// Defines in which contain an instance will be visible from
    /// </summary>
    public enum ComponentInstanceScope
    {
        /// <summary>
        /// Only the container the component instance was created in
        /// </summary>
        Container,
        /// <summary>
        /// All containers which were created from the same repository
        /// </summary>
        Repository,
        /// <summary>
        /// All containers that are created
        /// </summary>
        Global
    }
}