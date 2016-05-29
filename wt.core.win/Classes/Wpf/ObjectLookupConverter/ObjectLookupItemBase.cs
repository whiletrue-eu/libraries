using System.Windows.Markup;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Defines the base class for item entries for a <see cref="ObjectLookupConverter"/>.
    /// </summary>
    [ContentProperty("Value")]
    public abstract class ObjectLookupItemBase
    {
        /// <summary>
        /// Sets/Gets the object that shall be returned
        /// </summary>
        /// <remarks>
        /// This is the objects content property
        /// </remarks>
        public object Value { get; set; }
    }
}