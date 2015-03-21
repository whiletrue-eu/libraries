using System.Windows.Input;

namespace Mz.Classes.Controls
{
    public interface INotifyIconCallback
    {
        void MouseMoved();
        void MouseButtonUp(MouseButton button);
        void MouseButtonDown(MouseButton button);
        void MouseButtonDoubleClick(MouseButton button);
        void ContextMenu();
        void RecreationRequired();
        void MouseEnter();
        void MouseLeave();
    }
}