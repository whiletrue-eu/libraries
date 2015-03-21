using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    /// <summary>
    /// Connects the drag drop system to the source of an drag drop operation
    /// </summary>
    public interface IDragDropSource
    {
        /// <summary>
        /// Retrieves the data to be dragged when a drag operation starts
        /// </summary>
        object DragData { get; }
        /// <summary>
        /// Retrieves the allowed drag effects supported by the source
        /// </summary>
        DragDropEffects DragEffects{ get;}
        /// <summary>
        /// Notifies the source that the object was dropped, and about the drop effect. If required, the source can act upon this notification (e.g. remove its data when object was moved)
        /// </summary>
        void NotifyDropped(DragDropEffect dropEffect);
    }
}