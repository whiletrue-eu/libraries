using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Wpf
{
    public class TemplatedAdorner : Adorner
    {
        private readonly ContentPresenter adornerElement;


        public TemplatedAdorner(UIElement adornedElement, DataTemplate template, object dataContext)
            : base(adornedElement)
        {
            this.adornerElement = new ContentPresenter();
            this.adornerElement.Content = dataContext;
            this.adornerElement.ContentTemplate = template;
            this.adornerElement.Visibility = Visibility.Visible;
            this.adornerElement.SetBinding(FrameworkElement.WidthProperty, new Binding {Source = adornedElement, Path = new PropertyPath("ActualWidth")});
            this.adornerElement.SetBinding(FrameworkElement.HeightProperty, new Binding { Source = adornedElement, Path = new PropertyPath("ActualHeight") });
            
            this.AddVisualChild(this.adornerElement);
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            index.DbC_AssureArgumentInRange( "index",_ => _ == 0);
            return this.adornerElement;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            //this.adornerElement.Measure(constraint);
            //return this.adornerElement.DesiredSize;
            Size Result = base.MeasureOverride(constraint);
            this.InvalidateVisual();
            return Result;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.adornerElement.Arrange(new Rect(new Point(0, 0), finalSize));
            return new Size(this.adornerElement.ActualWidth, this.adornerElement.ActualHeight);
        }
    }
}