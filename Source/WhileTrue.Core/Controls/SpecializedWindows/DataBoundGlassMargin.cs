using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WhileTrue.Controls
{
    internal class DataBoundGlassMargin : GlassMargin
    {
        private readonly FrameworkElement element;

        public DataBoundGlassMargin(FrameworkElement element)
        {
            this.element = element;
            this.element.LayoutUpdated += this.ElementLayoutUpdated;
        }

        private void ElementLayoutUpdated(object sender, EventArgs e)
        {
            System.Windows.Window ParentWindow = this.GetParentWindow(this.element);
            if (ParentWindow != null && VisualTreeHelper.GetChildrenCount(ParentWindow)>0)
            {
                DependencyObject Content =  VisualTreeHelper.GetChild(ParentWindow, 0);
                Rect WindowBounds;

                GeneralTransform Transform = this.element.TransformToAncestor(ParentWindow);
                Rect ElementBounds = Transform.TransformBounds(new Rect(new Size(this.element.ActualWidth, this.element.ActualHeight)));

                if (Content is UIElement)
                {
                    WindowBounds = new Rect(((UIElement) Content).RenderSize);

                    this.Left = Math.Round(ElementBounds.Left);
                    this.Top = Math.Round(ElementBounds.Top);
                    this.Bottom = Math.Round(WindowBounds.Bottom - ElementBounds.Bottom);
                    this.Right = Math.Round(WindowBounds.Right - ElementBounds.Right);
                }
            }
        }

        private System.Windows.Window GetParentWindow(DependencyObject element)
        {
            DependencyObject Parent = VisualTreeHelper.GetParent(element);

            if (Parent is System.Windows.Window)
            {
                return (System.Windows.Window) Parent;
            }
            else if (Parent is Visual || Parent is Visual3D)
            {
                return this.GetParentWindow(Parent);
            }
            else
            {
                return null;
            }
        }
    }
}