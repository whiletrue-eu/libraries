using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    /// <summary>
    /// Implemented by a drag &amp; drop target model
    /// </summary>
    public interface IDragDropTarget
    {
        /// <summary>
        /// Get the drop effects that are possible for the data object given. 
        /// By comparing the drop effects to the ones accepted by the source, the effective drop effects
        /// are calculated. The drop effect applied is calculated depending on modifier keys pressed by the user
        /// and the <see cref="GetDefaultEffect"/> value.
        /// </summary>
        DragDropEffects GetDropEffects(IDataObject data);
        /// <summary>
        /// Gets the default effect that shall be used when no modifier key is pressed (but only if the 
        /// source supports the effect)
        /// </summary>
        DragDropEffect GetDefaultEffect(IDataObject data);
        /// <summary>
        /// Drops the data on the target with the given drop effect. <c>additionalDropInfo</c> may contain
        /// additonal data that can be used during the drop operation, depending on the type of target UI control
        /// </summary>
        void DoDrop(IDataObject data, DragDropEffect effect, AdditionalDropInfo additionalDropInfo);
    }
}