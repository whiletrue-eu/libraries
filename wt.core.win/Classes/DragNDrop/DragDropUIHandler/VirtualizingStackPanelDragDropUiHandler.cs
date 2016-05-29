using System;
using System.Windows;
using System.Windows.Controls;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    ///<summary>
    /// Drag and drop UI handler for VirtualizingStackPanel
    ///</summary>
    public class VirtualizingStackPanelDragDropUiHandler : PanelDragDropUiHandler
    {
        /// <summary>
        /// Type this handler is usable for
        /// </summary>
        public override Type Type => typeof(VirtualizingStackPanel);

        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        /// <param name="makeDroppable">if set to <c>true</c>, the control must be changed to accept drop actions.</param>
        public override IDragDropUiTargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
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
                return PanelDragDropUiHandlerUtils.CalculateDropIndex(this.Element, this.Element.Children, this.Element.Orientation, position);
            }

            protected override Rect CalculateDropMarker(int dropIndex)
            {
                return PanelDragDropUiHandlerUtils.CalculateDropMarker(this.Element, this.Element.Children, this.Element.Orientation,dropIndex);
            }

            protected override Orientation GetOrientation()
            {
                return this.Element.Orientation;
            }
        }
    }
}