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
                var Element = element;
                if (element is IScrollInfo) HandleAutoScroll((IScrollInfo) Element, position);
                if (element is ScrollViewer)
                {
                    var Scroller = element.GetVisualDescendantsDepthFirst<IScrollInfo>().FirstOrDefault();
                    if (Scroller != null) HandleAutoScroll(Scroller, position);
                }
            }

            private static void HandleAutoScroll(IScrollInfo element, DragPosition position)
            {
                if ((element.CanHorizontallyScroll || element.CanVerticallyScroll) && element is Visual &&
                    element is IInputElement)
                {
                    var Child = ((Visual) element).GetHitChild(position.GetPosition((IInputElement) element));
                    if (Child is IInputElement)
                    {
                        var Position = position.GetPosition((IInputElement) Child);
                        var RectangleToShow = GetRectangleToShowFromDragPosition(Position);
                        element.MakeVisible(Child, RectangleToShow);
                    }
                }
            }

            private static Rect GetRectangleToShowFromDragPosition(Point position)
            {
                var RectangleRadiusX = SystemParameters.IconWidth;
                var RectangleRadiusY = SystemParameters.IconHeight;
                return new Rect(
                    position.X - RectangleRadiusX,
                    position.Y - RectangleRadiusY,
                    RectangleRadiusX * 2,
                    RectangleRadiusY * 2);
            }
        }
    }
}