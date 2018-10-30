using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    /// <summary>
    ///     Drag and Drop source &amp; target handler base class for UIElement and ContentElement
    /// </summary>
    public abstract class ElementDragDropUiHandlerBase<TElementType> : IDragDropUiSourceHandler,
        IDragDropUiTargetHandler where TElementType : class
    {
        /// <summary>
        ///     Type this handler is usable for
        /// </summary>
        public Type Type => typeof(TElementType);

        /// <summary>
        ///     Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        public abstract IDragDropUiSourceHandlerInstance Create(DependencyObject element,
            IDragDropSourceAdapter adapter);

        /// <summary>
        ///     Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        /// <param name="makeDroppable">if set to <c>true</c>, the control must be changed to accept drop actions.</param>
        public abstract IDragDropUiTargetHandlerInstance Create(DependencyObject element,
            IDragDropTargetAdapter adapter, bool makeDroppable);

        /// <summary>
        ///     Base class for UI feedback handlers
        /// </summary>
        protected abstract class TargetHandlerBase<TElement> : IDragDropUiTargetHandlerInstance
        {
            private readonly bool makeDroppable;
            private readonly Action<TElement> undoMakeDroppableAction;

            /// <summary />
            protected TargetHandlerBase(TElement element, bool makeDroppable, Action<TElement> makeDroppableAction,
                Action<TElement> undoMakeDroppableAction)
            {
                Element = element;
                this.makeDroppable = makeDroppable;
                this.undoMakeDroppableAction = undoMakeDroppableAction;

                if (this.makeDroppable) makeDroppableAction(element);
            }

            /// <summary>
            ///     UI Element wrapped by this handler
            /// </summary>
            protected TElement Element { get; }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (makeDroppable) undoMakeDroppableAction(Element);
            }

            /// <summary>
            ///     Notifies the handler, that a drag operation has started above the control
            /// </summary>
            /// <param name="effect"></param>
            public void NotifyDragStarted(DragDropEffect effect)
            {
                if (effect != DragDropEffect.None) SetUpAdorner();
            }

            /// <summary>
            ///     Notifies the handler, that a drag operation has ended above the control, either because
            ///     the item was dropped, or because the mouse left the controls area
            /// </summary>
            public void NotifyDragEnded()
            {
                TearDownAdorner();
            }

            /// <summary>
            ///     Notifies the handler of an update of the mouse position on the control
            /// </summary>
            public void NotifyDragChanged(DragDropEffect effect, DragPosition position)
            {
                if (effect == DragDropEffect.None)
                    TearDownAdorner();
                else
                    SetUpAdorner();
            }

            /// <summary>
            ///     Gets additonal drop information that can be provided by the handler for the
            ///     target handler implemented in the model.
            /// </summary>
            public AdditionalDropInfo GetAdditionalDropInfo(DragPosition position)
            {
                return new AdditionalDropInfo();
            }

            /// <summary>
            ///     Implemented by derived classes to set up an adorner that visualizes UI feedback for drap'n'drop oeprations on this
            ///     element
            /// </summary>
            protected abstract void SetUpAdorner();

            /// <summary>
            ///     Implemented by derived classes to remove the adorner that visualizes UI feedback for drap'n'drop oeprations on this
            ///     element
            /// </summary>
            protected abstract void TearDownAdorner();
        }

        /// <summary>
        ///     Handler that handles the start of drag operations on an UI element
        /// </summary>
        protected abstract class SourceHandlerBase : IDragDropUiSourceHandlerInstance
        {
            private readonly IDragDropSourceAdapter adapter;

            /// <summary>
            ///     UI Element wrapped by this handler
            /// </summary>
            protected readonly TElementType Element;

            private IInputElement deviceTarget;
            private EllipseGeometry dragStartHotspot;

            /// <summary />
            protected SourceHandlerBase(TElementType element, IDragDropSourceAdapter adapter)
            {
                Element = element;
                this.adapter = adapter;

// ReSharper disable DoNotCallOverridableMethodsInConstructor
                HookEvents();
// ReSharper restore DoNotCallOverridableMethodsInConstructor
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                UnhookEvents();
            }

            /// <summary>
            ///     Implemented by derived classes to hook events to detect drag start operations
            /// </summary>
            protected abstract void HookEvents();

            /// <summary>
            ///     Implemented by derived classes to unhook from events that detected drag start operations
            /// </summary>
            protected abstract void UnhookEvents();


            /// <summary>
            ///     Handles mouse move messages over the wrapped UI element
            /// </summary>
            protected void MouseMove(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed ||
                    e.MiddleButton == MouseButtonState.Pressed)
                {
                    if (dragStartHotspot == null)
                    {
                        dragStartHotspot = new EllipseGeometry(e.GetPosition(e.Device.Target),
                            SystemParameters.MinimumHorizontalDragDistance * 2,
                            SystemParameters.MinimumVerticalDragDistance * 2);
                        deviceTarget = e.Device.Target;
                        deviceTarget.CaptureMouse();
                    }
                    else
                    {
                        if (dragStartHotspot.FillContains(e.GetPosition(e.Device.Target)) == false)
                        {
                            dragStartHotspot = null;
                            deviceTarget.ReleaseMouseCapture();
                            adapter.DoDragDrop();
                        }
                    }
                }
                else
                {
                    if (dragStartHotspot != null)
                    {
                        dragStartHotspot = null;
                        deviceTarget.ReleaseMouseCapture();
                    }
                }
            }
        }
    }
}