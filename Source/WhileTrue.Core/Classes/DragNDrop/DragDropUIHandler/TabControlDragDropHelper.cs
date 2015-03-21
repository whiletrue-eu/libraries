using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
    public class TabControlDragDropHelper : IDragDropUIHelper
    {
        public Type Type
        {
            get { return typeof(TabControl); }
        }

        public IDragDropUIHelperInstance Create(UIElement element)
        {
            element.DbC_Assure(value => value is TabControl);

            return new TargetHandler((TabControl)element);
        }

        private class TargetHandler : IDragDropUIHelperInstance
        {
            private readonly TabControl element;
            private readonly DispatcherTimer autoSelectTabItemTimer;
            private TabItem tabItemMouseIsHoveringOver;

            public TargetHandler(TabControl element)
            {
                this.element = element;
                
                this.autoSelectTabItemTimer = new DispatcherTimer(DispatcherPriority.Send);
                this.autoSelectTabItemTimer.Tick += this.AutoSelectTabItemTimerTick;
                this.autoSelectTabItemTimer.Interval = SystemParameters.MouseHoverTime;

            }

            void AutoSelectTabItemTimerTick(object sender, EventArgs e)
            {
                if (this.tabItemMouseIsHoveringOver.IsSelected == false)
                {
                    this.tabItemMouseIsHoveringOver.IsSelected = true;
                }
                this.StopHovering();
            }

            public void Dispose()
            {
                this.StopHovering();
            }

            /// <summary>
            /// Notifies the handler of an update of the mouse position on the control
            /// </summary>
            public void NotifyDrag(DragPosition position)
            {
                this.ReevaluateHoverItem(position);
            }

            private void ReevaluateHoverItem(DragPosition position)
            {
                TabItem TabItemMouseIsHoveringOver = (TabItem)VisualTreeHelperEx.GetVisualAncestors(this.element.InputHitTest(position.GetPosition(this.element)) as DependencyObject, this.element).Where(node => node is TabItem).FirstOrDefault();
                if (this.tabItemMouseIsHoveringOver != TabItemMouseIsHoveringOver)
                {
                    this.tabItemMouseIsHoveringOver = TabItemMouseIsHoveringOver;
                    if (this.tabItemMouseIsHoveringOver == null)
                    {
                        this.StopHovering();
                    }
                    else
                    {
                        this.autoSelectTabItemTimer.Start();
                    }
                }
            }

            private void StopHovering()
            {
                this.tabItemMouseIsHoveringOver = null;
                this.autoSelectTabItemTimer.Stop();
            }
        }
    }
}