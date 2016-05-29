using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Implements an adorner, which UI can be described by a WPF template backed by a custom view model
    /// </summary>
    public class TemplatedAdorner : Adorner
    {
        private readonly ContentPresenter adornerElement;

        /// <summary/>
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

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>
        /// The number of visual child elements for this element.
        /// </returns>
        protected override int VisualChildrenCount => 1;

        /// <summary>
        /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)"/>, and returns a child at the specified index from a collection of child elements. 
        /// </summary>
        /// <returns>
        /// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
        /// </returns>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        protected override Visual GetVisualChild(int index)
        {
            index.DbC_AssureArgumentInRange( "index",_ => _ == 0);
            return this.adornerElement;
        }

        /// <summary>
        /// Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Size"/> object representing the amount of layout space needed by the adorner.
        /// </returns>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        protected override Size MeasureOverride(Size constraint)
        {
            //this.adornerElement.Measure(constraint);
            //return this.adornerElement.DesiredSize;
            Size Result = base.MeasureOverride(constraint);
            this.InvalidateVisual();
            return Result;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement"/> derived class. 
        /// </summary>
        /// <returns>
        /// The actual size used.
        /// </returns>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            this.adornerElement.Arrange(new Rect(new Point(0, 0), finalSize));
            return new Size(this.adornerElement.ActualWidth, this.adornerElement.ActualHeight);
        }
    }
}