using System;

namespace WhileTrue.Classes.DragNDrop
{
    /// <summary>
    /// Enables code to start the dragdrop operation without having direct access to the source control
    /// </summary>
    public interface IDragDropSourceAdapter : IDisposable
    {
        /// <summary>
        /// Starts the drag drop operation
        /// </summary>
        void DoDragDrop();
    }
}