using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.Wpf;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    /// <summary>
    /// Handles UI for drag and drop for Panels
    /// </summary>
    public abstract class PanelDragDropUIHandler : IDragDropUITargetHandler
    {
        public abstract Type Type { get; }
        public abstract IDragDropUITargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable);

        protected abstract class TargetHandler<PanelType> : IDragDropUITargetHandlerInstance where PanelType:Panel
        {
            private class PanelAdornerData : ObservableObject
            {
                private readonly Orientation orientation;
                private int dropIndex;
                private Rect dropMarker;

                public PanelAdornerData(Orientation orientation)
                {
                    this.orientation = orientation;
                }

                // ReSharper disable UnusedMember.Local
                // ReSharper disable MemberCanBePrivate.Local
                public Orientation Orientation
                {
                    get
                    {
                        return this.orientation;
                    }
                }

                public int DropIndex
                {
                    get 
                    {
                        return this.dropIndex;
                    }
                    set 
                    {
                        this.SetAndInvoke(()=>DropIndex, ref this.dropIndex, value);
                    }
                }

                public Rect DropMarker
                {
                    get 
                    {
                        return this.dropMarker;
                    }
                    set 
                    {
                        this.SetAndInvoke(() => DropMarker, ref this.dropMarker, value);
                    }
                }
                // ReSharper restore UnusedMember.Local
                // ReSharper restore MemberCanBePrivate.Local
            }

            protected readonly PanelType element;
            private readonly bool makeDroppable;
            private TemplatedAdorner adorner;
            private PanelAdornerData adornerData;


            protected TargetHandler(PanelType element, bool makeDroppable)
            {
                
                this.element = element;
                this.makeDroppable = makeDroppable;

                if (makeDroppable)
                {
                    this.element.AllowDrop = true;
                }
            }

            public void Dispose()
            {
                if (this.makeDroppable)
                {
                    this.element.AllowDrop = false;
                }
            }

            public void NotifyDragStarted(DragDropEffect effect)
            {
                if (effect != DragDropEffect.None)
                {
                    this.SetUpAdorner();
                }
            }

            private void SetUpAdorner()
            {
                DragDropAdornerTemplate Template =
                    this.element.TryFindResource(new ComponentResourceKey(typeof (DragDrop), typeof (PanelType))) as DragDropAdornerTemplate
                    ?? this.element.TryFindResource(new ComponentResourceKey(typeof (DragDrop), typeof (object))) as DragDropAdornerTemplate;

                this.adornerData = new PanelAdornerData(this.GetOrientation());
                this.adorner = new TemplatedAdorner(this.element, Template, this.adornerData);
                AdornerLayer.GetAdornerLayer(this.element).Add(this.adorner);
            }

            public void NotifyDragChanged(DragDropEffect effect, DragPosition position)
            {
                if (effect == DragDropEffect.None)
                {
                    this.TearDownAdorner();
                }
                else
                {
                    int DropIndex = this.CalculateDropIndex(position.GetPosition(this.element));
                    Rect DropMarker = this.CalculateDropMarker(DropIndex);
                    this.UpdateAdorner(DropIndex, DropMarker);
                }
                
            }

            private void UpdateAdorner(int dropIndex, Rect dropMarker)
            {
                if (this.adorner == null)
                {
                    this.SetUpAdorner();
                }
                this.adornerData.DropIndex = dropIndex;
                this.adornerData.DropMarker = dropMarker;
            }

            public void NotifyDragEnded()
            {
                this.TearDownAdorner();
            }

            private void TearDownAdorner()
            {
                if (this.adorner != null)
                {
                    AdornerLayer.GetAdornerLayer(this.element).Remove(this.adorner);
                    this.adorner = null;
                    this.adornerData = null;
                }
            }

            public AdditionalDropInfo GetAdditionalDropInfo(DragPosition position)
            {
                return new AdditionalDropInfo(new DropIndex(this.CalculateDropIndex(position.GetPosition(this.element))));
            }

            protected abstract int CalculateDropIndex(Point position);
            protected abstract Rect CalculateDropMarker(int dropIndex);
            protected abstract Orientation GetOrientation();
        }


        internal static class PanelDragDropUIHandlerUtils
        {
            public static int CalculateDropIndex(Panel panel, UIElementCollection children, Orientation orientation, Point position)
            {
                HitTestResult HitTestResult = VisualTreeHelper.HitTest(panel, position);
                DependencyObject HitItem;
                if (HitTestResult != null)
                {
                    DependencyObject[] Ancestors = VisualTreeHelperEx.GetVisualAncestors(HitTestResult.VisualHit, panel);

                    if (Ancestors.Length >= 1)
                    {
                        HitItem = Ancestors[1];
                    }
                    else
                    {
                        HitItem = null; //ancestors only contain the panel itsself, or nothing. Force to go into 'else' branch
                    }
                }
                else
                {
                    HitItem = null;
                }

                if( HitItem is UIElement )
                {
                    int ItemIndex = children.IndexOf((UIElement) HitItem);
                    Rect Bounds = ((Visual)HitItem).TransformToAncestor(panel).TransformBounds(VisualTreeHelperEx.GetBounds((Visual)HitItem));

                    Rect FirstHalfBounds;
                    if (orientation == Orientation.Vertical)
                    {
                        FirstHalfBounds = new Rect(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height/2);
                    }
                    else
                    {
                        FirstHalfBounds = new Rect(Bounds.Left, Bounds.Top, Bounds.Width/2, Bounds.Height);
                    }

                    if (FirstHalfBounds.Contains(position))
                    {
                        return ItemIndex;
                    }
                    else
                    {
                        return ItemIndex + 1;
                    }
                }
                else
                {
                    // Hit cannot be processed or is on the panel -> return index to insert at the end
                    return children.Count;
                }
            }

            public static Rect CalculateDropMarker(Panel panel, UIElementCollection children, Orientation orientation, int dropIndex)
            {
                Rect DropMarker;
                if (children.Count > 0)
                {
                    if (dropIndex == 0)
                    {
                        Rect ItemRect = children[0].TransformToVisual(panel).TransformBounds(VisualTreeHelperEx.GetBounds(children[0]));
                        DropMarker = orientation == Orientation.Vertical
                                   ? new Rect(ItemRect.Left, ItemRect.Top, ItemRect.Width, 0)
                                   : new Rect(ItemRect.Left, ItemRect.Top, 0, ItemRect.Height);
                    }
                    else
                    {
                        Rect ItemRect = children[dropIndex - 1].TransformToVisual(panel).TransformBounds(VisualTreeHelperEx.GetBounds(children[dropIndex - 1]));
                        DropMarker = orientation == Orientation.Vertical
                                   ? new Rect(ItemRect.Left, ItemRect.Bottom, ItemRect.Width, 0)
                                   : new Rect(ItemRect.Right, ItemRect.Top, 0, ItemRect.Height);
                    }
                }
                else
                {
                    DropMarker = new Rect();
                }

                return DropMarker;
            }
        }
    }

    ///<summary>
    /// Drag and drop UI handler for StackPanel
    ///</summary>
    public class StackPanelDragDropUIHandler :PanelDragDropUIHandler
    {
        public override Type Type
        {
            get { return typeof(StackPanel); }
        }

        public override IDragDropUITargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
        {
            {
                element.DbC_Assure(e => e is StackPanel);

                return new StackPanelTargetHandler((StackPanel)element, makeDroppable);
            }

        }

        private class StackPanelTargetHandler: TargetHandler<StackPanel>
        {
            public StackPanelTargetHandler(StackPanel stackPanel, bool makeDroppable)
                : base(stackPanel, makeDroppable)
            {
               
            }

            protected override int CalculateDropIndex(Point position)
            {
                return PanelDragDropUIHandlerUtils.CalculateDropIndex(this.element, this.element.Children, this.element.Orientation, position);
            }

            protected override Rect CalculateDropMarker(int dropIndex)
            {
                return PanelDragDropUIHandlerUtils.CalculateDropMarker(this.element, this.element.Children, this.element.Orientation,dropIndex);
            }

            protected override Orientation GetOrientation()
            {
                return this.element.Orientation;
            }
        }
    }

    ///<summary>
    /// Drag and drop UI handler for VirtualizingStackPanel
    ///</summary>
    public class VirtualizingStackPanelDragDropUIHandler : PanelDragDropUIHandler
    {
        public override Type Type
        {
            get { return typeof(VirtualizingStackPanel); }
        }

        public override IDragDropUITargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
        {
            {
                element.DbC_Assure(e => e is VirtualizingStackPanel);

                return new VirtualizingStackPanelTargetHandler((VirtualizingStackPanel)element, makeDroppable);
            }

        }

        private class VirtualizingStackPanelTargetHandler : TargetHandler<VirtualizingStackPanel>
        {
            public VirtualizingStackPanelTargetHandler(VirtualizingStackPanel stackPanel, bool makeDroppable)
                : base(stackPanel, makeDroppable)
            {

            }

            protected override int CalculateDropIndex(Point position)
            {
                return PanelDragDropUIHandlerUtils.CalculateDropIndex(this.element, this.element.Children, this.element.Orientation, position);
            }

            protected override Rect CalculateDropMarker(int dropIndex)
            {
                return PanelDragDropUIHandlerUtils.CalculateDropMarker(this.element, this.element.Children, this.element.Orientation,dropIndex);
            }

            protected override Orientation GetOrientation()
            {
                return this.element.Orientation;
            }
        }
    }
    ///<summary>
    /// Drag and drop UI handler for TabPanel
    ///</summary>
    public class TabPanelDragDropUIHandler : PanelDragDropUIHandler
    {
        public override Type Type
        {
            get { return typeof(TabPanel); }
        }

        public override IDragDropUITargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
        {
            {
                element.DbC_Assure(e => e is TabPanel);

                return new TabPanelTargetHandler((TabPanel)element, makeDroppable);
            }

        }

        private class TabPanelTargetHandler : TargetHandler<TabPanel>
        {
            public TabPanelTargetHandler(TabPanel stackPanel, bool makeDroppable)
                : base(stackPanel, makeDroppable)
            {

            }

            protected override int CalculateDropIndex(Point position)
            {
                return PanelDragDropUIHandlerUtils.CalculateDropIndex(this.element, this.element.Children, Orientation.Horizontal, position);
            }

            protected override Rect CalculateDropMarker(int dropIndex)
            {
                return PanelDragDropUIHandlerUtils.CalculateDropMarker(this.element, this.element.Children, Orientation.Horizontal, dropIndex);
            }

            protected override Orientation GetOrientation()
            {
                return Orientation.Horizontal;
            }
        }
    }
}