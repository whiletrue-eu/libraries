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
            System.Windows.DragDrop.AddPreviewDragOverHandler(this.element, this.DragOver);
            System.Windows.DragDrop.AddPreviewDragLeaveHandler(this.element, this.DragLeave);
        }

        private void DragLeave(object sender, DragEventArgs e)
        {
            this.DisposeHelpers();
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            this.UpdateHelpers(sender, e);

            DragPosition Position = new DragPosition(e);
            this.uiHelper.ForEach(helper=>helper.NotifyDrag(Position));
        }

        private void UpdateHelpers(object sender, DragEventArgs e)
        {
            DependencyObject HitObject = e.OriginalSource as DependencyObject;

            //this.HandleAutoScroll(sender, e);

            if (this.hitObject != HitObject)
            {
                this.hitObject = HitObject;

                //Dispose old helper
                this.DisposeHelpers();

                //Get new list of helper
                while (HitObject != null && HitObject != this.element)
                {
                    if (HitObject is UIElement)
                    {
                        this.uiHelper.AddRange((from Helper in DragDrop.GetDragDropUiHelper(HitObject.GetType()) select Helper.Create((UIElement) HitObject)).ToArray());
                    }
                    HitObject = VisualTreeHelper.GetParent(HitObject);
                }
            }
        }

        private void DisposeHelpers()
        {
            this.uiHelper.ForEach(helper => helper.Dispose());
            this.uiHelper.Clear();
        }

        public static DragDropHelperAdapter Create(UIElement element)
        {
            return new DragDropHelperAdapter(element);
        }
    }
}