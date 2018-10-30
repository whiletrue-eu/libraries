using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    /// <summary>
    ///     Drag and drop target handler for TabControl
    /// </summary>
    /// <remarks>
    ///     For handling Tabpanel, the handler does not do handling for itsself but delegates to UI
    ///     handlers for this panel.
    /// </remarks>
    public class TabControlDragDropHelper : IDragDropUiHelper
    {
        /// <summary>
        ///     Type this handler is usable for
        /// </summary>
        public Type Type => typeof(TabControl);

        /// <summary>
        ///     Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        public IDragDropUiHelperInstance Create(UIElement element)
        {
            element.DbC_Assure(value => value is TabControl);

            return new TargetHandler((TabControl) element);
        }

        private class TargetHandler : IDragDropUiHelperInstance
        {
            private readonly DispatcherTimer autoSelectTabItemTimer;
            private readonly TabControl element;
            private TabItem tabItemMouseIsHoveringOver;

            public TargetHandler(TabControl element)
            {
                this.element = element;

                autoSelectTabItemTimer = new DispatcherTimer(DispatcherPriority.Send);
                autoSelectTabItemTimer.Tick += AutoSelectTabItemTimerTick;
                autoSelectTabItemTimer.Interval = SystemParameters.MouseHoverTime;
            }

            public void Dispose()
            {
                StopHovering();
            }

            /// <summary>
            ///     Notifies the handler of an update of the mouse position on the control
            /// </summary>
            public void NotifyDrag(DragPosition position)
            {
                ReevaluateHoverItem(position);
            }

            private void AutoSelectTabItemTimerTick(object sender, EventArgs e)
            {
                if (tabItemMouseIsHoveringOver.IsSelected == false) tabItemMouseIsHoveringOver.IsSelected = true;
                StopHovering();
            }

            private void ReevaluateHoverItem(DragPosition position)
            {
                var TabItemMouseIsHoveringOver = (TabItem) VisualTreeHelperEx
                    .GetVisualAncestors(element.InputHitTest(position.GetPosition(element)) as DependencyObject,
                        element).FirstOrDefault(node => node is TabItem);
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (tabItemMouseIsHoveringOver != TabItemMouseIsHoveringOver)
                {
                    tabItemMouseIsHoveringOver = TabItemMouseIsHoveringOver;
                    if (tabItemMouseIsHoveringOver == null)
                        StopHovering();
                    else
                        autoSelectTabItemTimer.Start();
                }
            }

            private void StopHovering()
            {
                tabItemMouseIsHoveringOver = null;
                autoSelectTabItemTimer.Stop();
            }
        }
    }
}