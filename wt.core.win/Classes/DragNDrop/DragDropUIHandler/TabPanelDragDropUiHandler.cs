using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    ///<summary>
    /// Drag and drop UI handler for TabPanel
    ///</summary>
    public class TabPanelDragDropUiHandler : PanelDragDropUiHandler
    {
        /// <summary>
        /// Type this handler is usable for
        /// </summary>
        public override Type Type => typeof(TabPanel);

        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        /// <param name="makeDroppable">if set to <c>true</c>, the control must be changed to accept drop actions.</param>
        public override IDragDropUiTargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
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
                return PanelDragDropUiHandlerUtils.CalculateDropIndex(this.Element, this.Element.Children, Orientation.Horizontal, position);
            }

            protected override Rect CalculateDropMarker(int dropIndex)
            {
                return PanelDragDropUiHandlerUtils.CalculateDropMarker(this.Element, this.Element.Children, Orientation.Horizontal, dropIndex);
            }

            protected override Orientation GetOrientation()
            {
                return Orientation.Horizontal;
            }
        }
    }
}