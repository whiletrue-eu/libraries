using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.Wpf;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    /// <summary>
    ///     Handles UI for drag and drop for Panels
    /// </summary>
    public abstract class PanelDragDropUiHandler : IDragDropUiTargetHandler
    {
        /// <summary>
        ///     Type this handler is usable for
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        ///     Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        /// <param name="makeDroppable">if set to <c>true</c>, the control must be changed to accept drop actions.</param>
        public abstract IDragDropUiTargetHandlerInstance Create(DependencyObject element,
            IDragDropTargetAdapter adapter, bool makeDroppable);

        /// <summary>
        ///     Handler for drag 'n' drop feedback on the UI of the panel
        /// </summary>
        protected abstract class TargetHandler<TPanelType> : IDragDropUiTargetHandlerInstance where TPanelType : Panel
        {
            /// <summary>
            ///     UI element wrapped in this handler
            /// </summary>
            protected readonly TPanelType Element;

            private readonly bool makeDroppable;
            private TemplatedAdorner adorner;
            private PanelAdornerData adornerData;


            /// <summary />
            protected TargetHandler(TPanelType element, bool makeDroppable)
            {
                Element = element;
                this.makeDroppable = makeDroppable;

                if (makeDroppable) Element.AllowDrop = true;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (makeDroppable) Element.AllowDrop = false;
            }

            /// <summary>
            ///     Notifies the handler, that a drag operation has started above the control
            /// </summary>
            public void NotifyDragStarted(DragDropEffect effect)
            {
                if (effect != DragDropEffect.None) SetUpAdorner();
            }

            /// <summary>
            ///     Notifies the handler of an update of the mouse position on the control
            /// </summary>
            public void NotifyDragChanged(DragDropEffect effect, DragPosition position)
            {
                if (effect == DragDropEffect.None)
                {
                    TearDownAdorner();
                }
                else
                {
                    var DropIndex = CalculateDropIndex(position.GetPosition(Element));
                    var DropMarker = CalculateDropMarker(DropIndex);
                    UpdateAdorner(DropIndex, DropMarker);
                }
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
            ///     Gets additonal drop information that can be provided by the handler for the
            ///     target handler implemented in the model.
            /// </summary>
            public AdditionalDropInfo GetAdditionalDropInfo(DragPosition position)
            {
                return new AdditionalDropInfo(new DropIndex(CalculateDropIndex(position.GetPosition(Element))));
            }

            private void SetUpAdorner()
            {
                var Template =
                    Element.TryFindResource(new ComponentResourceKey(typeof(DragDrop), typeof(TPanelType))) as
                        DragDropAdornerTemplate
                    ?? Element.TryFindResource(new ComponentResourceKey(typeof(DragDrop), typeof(object))) as
                        DragDropAdornerTemplate;

                adornerData = new PanelAdornerData(GetOrientation());
                adorner = new TemplatedAdorner(Element, Template, adornerData);
                AdornerLayer.GetAdornerLayer(Element).Add(adorner);
            }

            private void UpdateAdorner(int dropIndex, Rect dropMarker)
            {
                if (adorner == null) SetUpAdorner();
                adornerData.DropIndex = dropIndex;
                adornerData.DropMarker = dropMarker;
            }

            private void TearDownAdorner()
            {
                if (adorner != null)
                {
                    AdornerLayer.GetAdornerLayer(Element).Remove(adorner);
                    adorner = null;
                    adornerData = null;
                }
            }

            /// <summary>
            ///     used to calculate drop index from mouse position in dervied classes
            /// </summary>
            protected abstract int CalculateDropIndex(Point position);

            /// <summary>
            ///     used to set the drop marker as UI feddback in dervied classes
            /// </summary>
            protected abstract Rect CalculateDropMarker(int dropIndex);

            /// <summary>
            ///     get orientation in dervied classes that are collections of items
            /// </summary>
            protected abstract Orientation GetOrientation();

            private class PanelAdornerData : ObservableObject
            {
                private int dropIndex;
                private Rect dropMarker;

                public PanelAdornerData(Orientation orientation)
                {
                    Orientation = orientation;
                }

                // ReSharper disable UnusedMember.Local
                // ReSharper disable MemberCanBePrivate.Local
                public Orientation Orientation { get; }

                public int DropIndex
                {
                    get => dropIndex;
                    set => SetAndInvoke(nameof(DropIndex), ref dropIndex, value);
                }

                public Rect DropMarker
                {
                    get => dropMarker;
                    set => SetAndInvoke(nameof(DropMarker), ref dropMarker, value);
                }
                // ReSharper restore UnusedMember.Local
                // ReSharper restore MemberCanBePrivate.Local
            }
        }


        internal static class PanelDragDropUiHandlerUtils
        {
            public static int CalculateDropIndex(Panel panel, UIElementCollection children, Orientation orientation,
                Point position)
            {
                var HitTestResult = VisualTreeHelper.HitTest(panel, position);
                DependencyObject HitItem;
                if (HitTestResult != null)
                {
                    var Ancestors = VisualTreeHelperEx.GetVisualAncestors(HitTestResult.VisualHit, panel);

                    HitItem = Ancestors.Length >= 1 ? Ancestors[1] : null;
                }
                else
                {
                    HitItem = null;
                }

                if (HitItem is UIElement)
                {
                    var ItemIndex = children.IndexOf((UIElement) HitItem);
                    var Bounds = ((Visual) HitItem).TransformToAncestor(panel)
                        .TransformBounds(VisualTreeHelperEx.GetBounds((Visual) HitItem));

                    var FirstHalfBounds = orientation == Orientation.Vertical
                        ? new Rect(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height / 2)
                        : new Rect(Bounds.Left, Bounds.Top, Bounds.Width / 2, Bounds.Height);

                    if (FirstHalfBounds.Contains(position))
                        return ItemIndex;
                    return ItemIndex + 1;
                }

                // Hit cannot be processed or is on the panel -> return index to insert at the end
                return children.Count;
            }

            public static Rect CalculateDropMarker(Panel panel, UIElementCollection children, Orientation orientation,
                int dropIndex)
            {
                Rect DropMarker;
                if (children.Count > 0)
                {
                    if (dropIndex == 0)
                    {
                        var ItemRect = children[0].TransformToVisual(panel)
                            .TransformBounds(VisualTreeHelperEx.GetBounds(children[0]));
                        DropMarker = orientation == Orientation.Vertical
                            ? new Rect(ItemRect.Left, ItemRect.Top, ItemRect.Width, 0)
                            : new Rect(ItemRect.Left, ItemRect.Top, 0, ItemRect.Height);
                    }
                    else
                    {
                        var ItemRect = children[dropIndex - 1].TransformToVisual(panel)
                            .TransformBounds(VisualTreeHelperEx.GetBounds(children[dropIndex - 1]));
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
}