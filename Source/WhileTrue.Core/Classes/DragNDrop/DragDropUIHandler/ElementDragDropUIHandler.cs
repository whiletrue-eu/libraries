using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.Wpf;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    ///<summary>
    /// Drag and Drop source & target handler base class for UIElement and ContentElement
    ///</summary>
    public abstract class ElementDragDropUIHandlerBase<ElementType> : IDragDropUISourceHandler, IDragDropUITargetHandler where ElementType : class
    {
        public Type Type
        {
            get { return typeof(ElementType); }
        }

        public abstract IDragDropUITargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable);
        public abstract IDragDropUISourceHandlerInstance Create(DependencyObject element, IDragDropSourceAdapter adapter);

        protected abstract class TargetHandlerBase<TElement> : IDragDropUITargetHandlerInstance
        {
            private readonly bool makeDroppable;
            private readonly Action<TElement> undoMakeDroppableAction;

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

            public void Dispose()
            {
                if (this.makeDroppable)
                {
                    undoMakeDroppableAction(this.Element);
                }
            }

            protected TElement Element { get; private set; }

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

            protected abstract void SetUpAdorner();
            protected abstract void TearDownAdorner();

            public AdditionalDropInfo GetAdditionalDropInfo(DragPosition position)
            {
                return new AdditionalDropInfo();
            }
        }

        protected abstract class SourceHandlerBase : IDragDropUISourceHandlerInstance
        {
            protected readonly ElementType element;
            private readonly IDragDropSourceAdapter adapter;
            private EllipseGeometry dragStartHotspot;
            private IInputElement deviceTarget;

            protected SourceHandlerBase(ElementType element, IDragDropSourceAdapter adapter)
            {
                this.element = element;
                this.adapter = adapter;

// ReSharper disable DoNotCallOverridableMethodsInConstructor
                this.HookEvents();
// ReSharper restore DoNotCallOverridableMethodsInConstructor
            }

            public void Dispose()
            {
                this.UnhookEvents();
            }

            protected abstract void HookEvents();
            protected abstract void UnhookEvents();


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

    ///<summary>
    /// Drag and Drop source & target handler for UIElement
    ///</summary>
    public class FrameworkElementDragDropUIHandler : ElementDragDropUIHandlerBase<FrameworkElement>
    {
        public override IDragDropUISourceHandlerInstance Create(DependencyObject element, IDragDropSourceAdapter adapter)
        {
            element.DbC_Assure(e => e is FrameworkElement);
            return new SourceHandler((FrameworkElement)element, adapter);
        }

        public override IDragDropUITargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
        {
            element.DbC_Assure(e => e is FrameworkElement);
            return new TargetHandler((FrameworkElement)element, makeDroppable);
        } 
        
        protected class SourceHandler : SourceHandlerBase
        {
            public SourceHandler(FrameworkElement element, IDragDropSourceAdapter adapter)
                : base(element, adapter)
            {
            }

            protected override void HookEvents()
            {
                this.element.MouseMove += MouseMove;
            }

            protected override void UnhookEvents()
            {
                this.element.MouseMove -= MouseMove;
            }
        }

        protected class TargetHandler : TargetHandlerBase<FrameworkElement>
        {
            private TemplatedAdorner adorner;

            public TargetHandler(FrameworkElement element, bool makeDroppable)
                : base(element, makeDroppable, _ => _.AllowDrop=true, _=>_.AllowDrop=false )
            {
            }

            protected override void SetUpAdorner()
            {
                DragDropAdornerTemplate Template =
                    this.Element.TryFindResource(new ComponentResourceKey(typeof(DragDrop), typeof(UIElement))) as DragDropAdornerTemplate
                    ?? this.Element.TryFindResource(new ComponentResourceKey(typeof(DragDrop), typeof(object))) as DragDropAdornerTemplate;

                AdornerLayer AdornerLayer = AdornerLayer.GetAdornerLayer(this.Element);
                if (AdornerLayer != null)
                {
                    this.adorner = new TemplatedAdorner(this.Element, Template, null);
                    AdornerLayer.Add(this.adorner);
                }
            }

            protected override void TearDownAdorner()
            {
                if (this.adorner != null)
                {
                    AdornerLayer.GetAdornerLayer(this.Element).Remove(this.adorner);
                    this.adorner = null;
                }
            }
        }
    }

    ///<summary>
    /// Drag and Drop source & target handler for ContentElement
    ///</summary>
    public class FrameworkContentElementDragDropUIHandler : ElementDragDropUIHandlerBase<FrameworkContentElement>
    {
        public override IDragDropUISourceHandlerInstance Create(DependencyObject element, IDragDropSourceAdapter adapter)
        {
            element.DbC_Assure(e => e is FrameworkContentElement);
            return new SourceHandler((FrameworkContentElement)element, adapter);
        }

        public override IDragDropUITargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
        {
            element.DbC_Assure(e => e is FrameworkContentElement);
            return new TargetHandler((FrameworkContentElement)element, makeDroppable);
        }

        protected class SourceHandler : SourceHandlerBase
        {
            public SourceHandler(FrameworkContentElement element, IDragDropSourceAdapter adapter)
                : base(element, adapter)
            {
            }

            protected override void HookEvents()
            {
                this.element.MouseMove += MouseMove;
            }

            protected override void UnhookEvents()
            {
                this.element.MouseMove -= MouseMove;
            }

        }

        protected class TargetHandler : TargetHandlerBase<FrameworkContentElement>
        {

            public TargetHandler(FrameworkContentElement element, bool makeDroppable)
                : base(element, makeDroppable, _ => _.AllowDrop = true, _ => _.AllowDrop = false)
            {
            }

            protected override void SetUpAdorner()
            {
            }

            protected override void TearDownAdorner()
            {
            }
        }
    }
}