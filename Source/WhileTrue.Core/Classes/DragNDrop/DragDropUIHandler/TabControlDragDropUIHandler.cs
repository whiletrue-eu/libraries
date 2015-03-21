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
    public class TabControlDragDropUIHandler : IDragDropUITargetHandler
    {
        public Type Type
        {
            get { return typeof(TabControl); }
        }

        public IDragDropUITargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable)
        {
            element.DbC_Assure(e => e is TabControl);

            return new TargetHandler((TabControl)element, adapter, makeDroppable);
        }

        private class TargetHandler : IDragDropUITargetHandlerInstance
        {
            private readonly TabControl element;
            private readonly IDragDropTargetAdapter adapter;
            private readonly bool makeDroppable;
            private IDragDropUITargetHandlerInstance panelTargetHandler;

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
                    IDragDropUITargetHandler Handler = DragDrop.GetDragDropUITargetHandler(TabPanel.GetType());
                    if (Handler != null)
                    {
                        this.panelTargetHandler = Handler.Create(TabPanel, this.adapter, false);
                    }
                }
                if (this.panelTargetHandler == null)
                {
                    this.panelTargetHandler = new DummyUIHandler();
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