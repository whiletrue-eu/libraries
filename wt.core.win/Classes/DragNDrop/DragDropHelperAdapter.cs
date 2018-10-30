using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace WhileTrue.Classes.DragNDrop
{
    internal class DragDropHelperAdapter
    {
        private readonly UIElement element;
        private readonly List<IDragDropUiHelperInstance> uiHelper = new List<IDragDropUiHelperInstance>();
        private DependencyObject hitObject;

        private DragDropHelperAdapter(UIElement element)
        {
            this.element = element;
            System.Windows.DragDrop.AddPreviewDragOverHandler(this.element, DragOver);
            System.Windows.DragDrop.AddPreviewDragLeaveHandler(this.element, DragLeave);
        }

        private void DragLeave(object sender, DragEventArgs e)
        {
            DisposeHelpers();
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            UpdateHelpers(sender, e);

            var Position = new DragPosition(e);
            uiHelper.ForEach(helper => helper.NotifyDrag(Position));
        }

        private void UpdateHelpers(object sender, DragEventArgs e)
        {
            var HitObject = e.OriginalSource as DependencyObject;

            //this.HandleAutoScroll(sender, e);

            if (hitObject != HitObject)
            {
                hitObject = HitObject;

                //Dispose old helper
                DisposeHelpers();

                //Get new list of helper
                while (HitObject != null && HitObject != element)
                {
                    if (HitObject is UIElement)
                        uiHelper.AddRange((from Helper in DragDrop.GetDragDropUiHelper(HitObject.GetType())
                            select Helper.Create((UIElement) HitObject)).ToArray());
                    HitObject = VisualTreeHelper.GetParent(HitObject);
                }
            }
        }

        private void DisposeHelpers()
        {
            uiHelper.ForEach(helper => helper.Dispose());
            uiHelper.Clear();
        }

        public static DragDropHelperAdapter Create(UIElement element)
        {
            return new DragDropHelperAdapter(element);
        }
    }
}