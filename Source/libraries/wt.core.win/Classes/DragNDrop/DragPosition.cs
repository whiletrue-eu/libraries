using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    ///<summary/>
    public class DragPosition
    {
        private readonly DragEventArgs dragEventArgs;

        internal DragPosition(DragEventArgs dragEventArgs)
        {
            this.dragEventArgs = dragEventArgs;
        }

        ///<summary>
        /// Gets the position relative to the given input element
        ///</summary>
        public Point GetPosition(IInputElement element)
        {
            return this.dragEventArgs.GetPosition(element);
        }
    }
}