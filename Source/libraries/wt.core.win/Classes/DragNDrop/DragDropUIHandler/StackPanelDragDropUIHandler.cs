using System;
using System.Windows;
using System.Windows.Controls;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    ///<summary>
    /// Drag and drop UI handler for StackPanel
    ///</summary>
    public class StackPanelDragDropUiHandler :PanelDragDropUiHandler
    {
        /// <summary>
        /// Type this handler is usable for
        /// </summary>
        public override Type Type => typeof(StackPanel);

        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        /// <param name="makeDroppable">if set to <c>true</c>, the control must be changed to accept drop actions.</param>
        public override IDragDropUiTargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
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