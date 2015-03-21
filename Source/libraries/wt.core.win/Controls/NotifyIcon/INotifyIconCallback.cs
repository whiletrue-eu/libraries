using System.Windows.Input;

namespace WhileTrue.Controls
{
    /// <summary>
    /// callback handler for a NotifyIcon
    /// </summary>
    public interface INotifyIconCallback
    {
        /// <summary>
        /// called when the mouse is moved over the icon
        /// </summary>
        void MouseMoved();
        /// <summary>
        /// called when the mouse button was released
        /// </summary>
        void MouseButtonUp(MouseButton button);
        /// <summary>
        /// called when the mouse button is pressed
        /// </summary>
        void MouseButtonDown(MouseButton button);
        /// <summary>
        /// called when the mouse button is double-clicked
        /// </summary>
        void MouseButtonDoubleClick(MouseButton button);
        /// <summary>
        /// called when right mouse button was cliked to retrieve the context menu
        /// </summary>
        void ContextMenu();
        /// <summary>
        /// called when the notify icon has to be recreated
        /// </summary>
        void RecreationRequired();
        /// <summary>
        /// Called when mouse enteres the icon
        /// </summary>
        void MouseEnter();
        /// <summary>
        /// Called when mouse leaves the icon
        /// </summary>
        void MouseLeave();
    }
}