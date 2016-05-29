using System.Windows;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    ///<summary>
    /// Drag and Drop source &amp; target handler for ContentElement
    ///</summary>
    public class FrameworkContentElementDragDropUiHandler : ElementDragDropUiHandlerBase<FrameworkContentElement>
    {
        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        public override IDragDropUiSourceHandlerInstance Create(DependencyObject element, IDragDropSourceAdapter adapter)
        {
            element.DbC_Assure(e => e is FrameworkContentElement);
            return new SourceHandler((FrameworkContentElement)element, adapter);
        }

        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        /// <param name="makeDroppable">if set to <c>true</c>, the control must be changed to accept drop actions.</param>
        public override IDragDropUiTargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
        {
            element.DbC_Assure(e => e is FrameworkContentElement);
            return new TargetHandler((FrameworkContentElement)element, makeDroppable);
        }

        /// <summary>
        /// Handler that handles drag operations for FrameworkContentElement derived UI classes
        /// </summary>
        protected class SourceHandler : SourceHandlerBase
        {
            /// <summary/>
            public SourceHandler(FrameworkContentElement element, IDragDropSourceAdapter adapter)
                : base(element, adapter)
            {
            }

            /// <summary>
            /// Implemented by derived classes to hook events to detect drag start operations
            /// </summary>
            protected override void HookEvents()
            {
                this.Element.MouseMove += this.MouseMove;
            }

            /// <summary>
            /// Implemented by derived classes to unhook from events that detected drag start operations
            /// </summary>
            protected override void UnhookEvents()
            {
                this.Element.MouseMove -= this.MouseMove;
            }

        }

        /// <summary>
        /// Handler that handles drop operations for FrameworkContentElement derived UI classes
        /// </summary>
        protected class TargetHandler : TargetHandlerBase<FrameworkContentElement>
        {

            /// <summary/>
            public TargetHandler(FrameworkContentElement element, bool makeDroppable)
                : base(element, makeDroppable, _ => _.AllowDrop = true, _ => _.AllowDrop = false)
            {
            }

            /// <summary>
            /// Implemented by derived classes to set up an adorner that visualizes UI feedback for drap'n'drop oeprations on this element
            /// </summary>
            protected override void SetUpAdorner()
            {
            }

            /// <summary>
            /// Implemented by derived classes to remove the adorner that visualizes UI feedback for drap'n'drop oeprations on this element
            /// </summary>
            protected override void TearDownAdorner()
            {
            }
        }
    }
}