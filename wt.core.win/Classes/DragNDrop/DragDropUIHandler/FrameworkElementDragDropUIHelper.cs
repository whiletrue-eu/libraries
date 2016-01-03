using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    internal class FrameworkElementDragDropUiHelper : IDragDropUiHelper
    {
        public Type Type => typeof(FrameworkElement);

        public IDragDropUiHelperInstance Create(UIElement element)
        {
            element.DbC_Assure(value => value is FrameworkElement);

            return new Helper((FrameworkElement) element);
        }

        private class Helper : IDragDropUiHelperInstance
        {
            private readonly FrameworkElement element;
            
            public Helper(FrameworkElement element)
            {
                this.element = element;
            }

            public void Dispose()
            {
            }

            public void NotifyDrag(DragPosition position)
            {
                FrameworkElement Element = this.element;
                if( this.element is IScrollInfo )
                {
                    Helper.HandleAutoScroll((IScrollInfo) Element, position);
                }
                if (this.element is ScrollViewer)
                {
                    IScrollInfo Scroller = this.element.GetVisualDescendantsDepthFirst<IScrollInfo>().FirstOrDefault();
                    if (Scroller != null)
                    {
                        Helper.HandleAutoScroll(Scroller, position);
                    }
                }
            }

            private static void HandleAutoScroll(IScrollInfo element, DragPosition position)
            {
                if ( ((element.CanHorizontallyScroll) || (element.CanVerticallyScroll)) && element is Visual && element is IInputElement)
                {
                    Visual Child = ((Visual)element).GetHitChild(position.GetPosition((IInputElement)element));
                    if( Child is IInputElement)
                    {
                        Point Position = position.GetPosition((IInputElement) Child);
                        Rect RectangleToShow = Helper.GetRectangleToShowFromDragPosition(Position);
                        element.MakeVisible(Child, RectangleToShow);
                    }
                }
            }

            private static Rect GetRectangleToShowFromDragPosition(Point position)
            {
                double RectangleRadiusX = SystemParameters.IconWidth;
                double RectangleRadiusY = SystemParameters.IconHeight;
                return new Rect(
                    position.X - RectangleRadiusX,
                    position.Y - RectangleRadiusY,
                    RectangleRadiusX * 2,
                    RectangleRadiusY * 2);
            }
        }
    }
}