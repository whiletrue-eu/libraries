using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    ///<summary>
    /// Drag and drop target handler for TabControl
    ///</summary>
    /// <remarks>
    /// For handling Tabpanel, the handler does not do handling for itsself but delegates to UI 
    /// handlers for this panel.
    /// </remarks>
    public class TabControlDragDropUiHandler : IDragDropUiTargetHandler
    {
        /// <summary>
        /// Type this handler is usable for
        /// </summary>
        public Type Type => typeof(TabControl);

        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        /// <param name="makeDroppable">if set to <c>true</c>, the control must be changed to accept drop actions.</param>
        public IDragDropUiTargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
        {
            element.DbC_Assure(e => e is TabControl);

            return new TargetHandler((TabControl)element, adapter, makeDroppable);
        }

        private class TargetHandler : IDragDropUiTargetHandlerInstance
        {
            private readonly TabControl element;
            private readonly IDragDropTargetAdapter adapter;
            private readonly bool makeDroppable;
            private IDragDropUiTargetHandlerInstance panelTargetHandler;

            public TargetHandler(TabControl element, IDragDropTargetAdapter adapter, bool makeDroppable)
            {
                this.element = element;
                this.adapter = adapter;
                this.makeDroppable = makeDroppable;

                if (this.makeDroppable)
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

            /// <summary>
            /// Notifies the handler, that a drag operation has started above the control
            /// </summary>
            /// <param name="effect"></param>
            public void NotifyDragStarted(DragDropEffect effect)
            {
                TabPanel TabPanel = this.element.GetVisualDescendantsDepthFirst<TabPanel>().FirstOrDefault();
                if (TabPanel != null)
                {
                    IDragDropUiTargetHandler Handler = DragDrop.GetDragDropUITargetHandler(TabPanel.GetType());
                    if (Handler != null)
                    {
                        this.panelTargetHandler = Handler.Create(TabPanel, this.adapter, false);
                    }
                }
                if (this.panelTargetHandler == null)
                {
                    this.panelTargetHandler = new DummyUiHandler();
                }

                this.panelTargetHandler.NotifyDragStarted(effect);
            }

            /// <summary>
            /// Notifies the handler, that a drag operation has ended above the control, either because 
            /// the item was dropped, or because the mouse left the controls area
            /// </summary>
            public void NotifyDragEnded()
            {
                this.panelTargetHandler.NotifyDragEnded();
                this.panelTargetHandler.Dispose();
                this.panelTargetHandler = null;
            }

            /// <summary>
            /// Notifies the handler of an update of the mouse position on the control
            /// </summary>
            public void NotifyDragChanged(DragDropEffect effect, DragPosition position)
            {
                this.panelTargetHandler.NotifyDragChanged(effect, position);
            }

            public AdditionalDropInfo GetAdditionalDropInfo(DragPosition position)
            {
                return this.panelTargetHandler.GetAdditionalDropInfo(position);
            }
        }
    }
}