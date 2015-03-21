using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    ///<summary>
    /// Drag and Drop source &amp; target handler base class for UIElement and ContentElement
    ///</summary>
    public abstract class ElementDragDropUiHandlerBase<TElementType> : IDragDropUiSourceHandler, IDragDropUiTargetHandler where TElementType : class
    {
        /// <summary>
        /// Type this handler is usable for
        /// </summary>
        public Type Type => typeof(TElementType);

        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        /// <param name="makeDroppable">if set to <c>true</c>, the control must be changed to accept drop actions.</param>
        public abstract IDragDropUiTargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable);

        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        public abstract IDragDropUiSourceHandlerInstance Create(DependencyObject element, IDragDropSourceAdapter adapter);

        /// <summary>
        /// Base class for UI feedback handlers
        /// </summary>
        protected abstract class TargetHandlerBase<TElement> : IDragDropUiTargetHandlerInstance
        {
            private readonly bool makeDroppable;
            private readonly Action<TElement> undoMakeDroppableAction;

            /// <summary/>
            protected TargetHandlerBase(TElement element, bool makeDroppable, Action<TElement> makeDroppableAction, Action<TElement> undoMakeDroppableAction)
            {
                this.Element = element;
                this.makeDroppable = makeDroppable;
                this.undoMakeDroppableAction = undoMakeDroppableAction;

                if (this.makeDroppable)
                {
                    makeDroppableAction(element);
                }
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (this.makeDroppable)
                {
                    this.undoMakeDroppableAction(this.Element);
                }
            }

            /// <summary>
            /// UI Element wrapped by this handler
            /// </summary>
            protected TElement Element { get; }

            /// <summary>
            /// Notifies the handler, that a drag operation has started above the control
            /// </summary>
            /// <param name="effect"></param>
            public void NotifyDragStarted(DragDropEffect effect)
            {
                if (effect != DragDropEffect.None)
                {
                    this.SetUpAdorner();
                }
            }

            /// <summary>
            /// Notifies the handler, that a drag operation has ended above the control, either because 
            /// the item was dropped, or because the mouse left the controls area
            /// </summary>
            public void NotifyDragEnded()
            {
                this.TearDownAdorner();
            }

            /// <summary>
            /// Notifies the handler of an update of the mouse position on the control
            /// </summary>
            public void NotifyDragChanged(DragDropEffect effect, DragPosition position)
            {
                if (effect == DragDropEffect.None)
                {
                    this.TearDownAdorner();
                }
                else
                {
                    this.SetUpAdorner();
                }
            }

            /// <summary>
            /// Implemented by derived classes to set up an adorner that visualizes UI feedback for drap'n'drop oeprations on this element
            /// </summary>
            protected abstract void SetUpAdorner();
            /// <summary>
            /// Implemented by derived classes to remove the adorner that visualizes UI feedback for drap'n'drop oeprations on this element
            /// </summary>
            protected abstract void TearDownAdorner();

            /// <summary>
            /// Gets additonal drop information that can be provided by the handler for the 
            /// target handler implemented in the model.
            /// </summary>
            public AdditionalDropInfo GetAdditionalDropInfo(DragPosition position)
            {
                return new AdditionalDropInfo();
            }
        }

        /// <summary>
        /// Handler that handles the start of drag operations on an UI element
        /// </summary>
        protected abstract class SourceHandlerBase : IDragDropUiSourceHandlerInstance
        {
            /// <summary>
            /// UI Element wrapped by this handler
            /// </summary>
            protected readonly TElementType Element;
            private readonly IDragDropSourceAdapter adapter;
            private EllipseGeometry dragStartHotspot;
            private IInputElement deviceTarget;

            /// <summary/>
            protected SourceHandlerBase(TElementType element, IDragDropSourceAdapter adapter)
            {
                this.Element = element;
                this.adapter = adapter;

// ReSharper disable DoNotCallOverridableMethodsInConstructor
                this.HookEvents();
// ReSharper restore DoNotCallOverridableMethodsInConstructor
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                this.UnhookEvents();
            }

            /// <summary>
            /// Implemented by derived classes to hook events to detect drag start operations
            /// </summary>
            protected abstract void HookEvents();
            /// <summary>
            /// Implemented by derived classes to unhook from events that detected drag start operations
            /// </summary>
            protected abstract void UnhookEvents();


            /// <summary>
            /// Handles mouse move messages over the wrapped UI element
            /// </summary>
            protected void MouseMove(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed)
                {
                    if (this.dragStartHotspot == null)
                    {
                        this.dragStartHotspot = new EllipseGeometry(e.GetPosition(e.Device.Target), SystemParameters.MinimumHorizontalDragDistance*2, SystemParameters.MinimumVerticalDragDistance*2);
                        this.deviceTarget = e.Device.Target;
                        this.deviceTarget.CaptureMouse();
                    }
                    else
                    {
                        if (this.dragStartHotspot.FillContains(e.GetPosition(e.Device.Target)) == false)
                        {
                            this.dragStartHotspot = null;
                            this.deviceTarget.ReleaseMouseCapture();
                            this.adapter.DoDragDrop();
                        }
                    }
                }
                else
                {
                    if (this.dragStartHotspot != null)
                    {
                        this.dragStartHotspot = null;
                        this.deviceTarget.ReleaseMouseCapture();
                    }
                }
            }
        }
    }
}