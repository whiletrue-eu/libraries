using System.Windows;
using System.Windows.Documents;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.Wpf;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    /// <summary>
    ///     Drag and Drop source &amp; target handler for UIElement
    /// </summary>
    public class FrameworkElementDragDropUiHandler : ElementDragDropUiHandlerBase<FrameworkElement>
    {
        /// <summary>
        ///     Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        public override IDragDropUiSourceHandlerInstance Create(DependencyObject element,
            IDragDropSourceAdapter adapter)
        {
            element.DbC_Assure(e => e is FrameworkElement);
            return new SourceHandler((FrameworkElement) element, adapter);
        }

        /// <summary>
        ///     Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        /// <param name="makeDroppable">if set to <c>true</c>, the control must be changed to accept drop actions.</param>
        public override IDragDropUiTargetHandlerInstance Create(DependencyObject element,
            IDragDropTargetAdapter adapter, bool makeDroppable)
        {
            element.DbC_Assure(e => e is FrameworkElement);
            return new TargetHandler((FrameworkElement) element, makeDroppable);
        }

        /// <summary>
        ///     Handles drag start for FraneworkElement derived UI elements
        /// </summary>
        protected class SourceHandler : SourceHandlerBase
        {
            /// <summary />
            public SourceHandler(FrameworkElement element, IDragDropSourceAdapter adapter)
                : base(element, adapter)
            {
            }

            /// <summary>
            ///     Implemented by derived classes to hook events to detect drag start operations
            /// </summary>
            protected override void HookEvents()
            {
                Element.MouseMove += MouseMove;
            }

            /// <summary>
            ///     Implemented by derived classes to unhook from events that detected drag start operations
            /// </summary>
            protected override void UnhookEvents()
            {
                Element.MouseMove -= MouseMove;
            }
        }

        /// <summary>
        ///     Handles drop actions for FrameworkELement derived UI classes
        /// </summary>
        protected class TargetHandler : TargetHandlerBase<FrameworkElement>
        {
            private TemplatedAdorner adorner;

            /// <summary />
            public TargetHandler(FrameworkElement element, bool makeDroppable)
                : base(element, makeDroppable, _ => _.AllowDrop = true, _ => _.AllowDrop = false)
            {
            }

            /// <summary>
            ///     Implemented by derived classes to set up an adorner that visualizes UI feedback for drap'n'drop oeprations on this
            ///     element
            /// </summary>
            protected override void SetUpAdorner()
            {
                var Template =
                    Element.TryFindResource(new ComponentResourceKey(typeof(DragDrop), typeof(UIElement))) as
                        DragDropAdornerTemplate
                    ?? Element.TryFindResource(new ComponentResourceKey(typeof(DragDrop), typeof(object))) as
                        DragDropAdornerTemplate;

                var AdornerLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(Element);
                if (AdornerLayer != null)
                {
                    adorner = new TemplatedAdorner(Element, Template, null);
                    AdornerLayer.Add(adorner);
                }
            }

            /// <summary>
            ///     Implemented by derived classes to remove the adorner that visualizes UI feedback for drap'n'drop oeprations on this
            ///     element
            /// </summary>
            protected override void TearDownAdorner()
            {
                if (adorner != null)
                {
                    AdornerLayer.GetAdornerLayer(Element).Remove(adorner);
                    adorner = null;
                }
            }
        }
    }
}