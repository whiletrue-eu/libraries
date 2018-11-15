// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    ///     Provides helper methods for VisualTree
    /// </summary>
    public static class VisualTreeHelperEx
    {
        /// <summary>
        ///     Enumerates through the descendats of the given <c>parent</c>, performing a 'depth first' search
        /// </summary>
        public static IEnumerable<TYpe> GetVisualDescendantsDepthFirst<TYpe>(this DependencyObject parent)
            where TYpe : class
        {
            var ChildrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var Index = 0; Index < ChildrenCount; Index++)
            {
                var Child = VisualTreeHelper.GetChild(parent, Index);
                if (Child is TYpe) yield return Child as TYpe;

                foreach (var Descendant in Child.GetVisualDescendantsDepthFirst<TYpe>()) yield return Descendant;
            }
        }

        /// <summary>
        ///     Enumerates through the descendats of the given <c>parent</c>, performing a 'breadth first' search
        /// </summary>
        public static IEnumerable<TYpe> GetVisualDescendantsBreadthFirst<TYpe>(this DependencyObject parent)
            where TYpe : class
        {
            var ChildrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var Index = 0; Index < ChildrenCount; Index++)
            {
                var Child = VisualTreeHelper.GetChild(parent, Index);
                if (Child is TYpe) yield return Child as TYpe;
            }

            for (var Index = 0; Index < ChildrenCount; Index++)
            {
                var Child = VisualTreeHelper.GetChild(parent, Index);
                foreach (var Descendant in Child.GetVisualDescendantsBreadthFirst<TYpe>()) yield return Descendant;
            }
        }

        /// <summary>
        ///     Returns a list of ancestors of the given <c>start</c> visual (not returned in the list),
        ///     up to the given <c>end</c> visual (which is included in the list), or the root of the
        ///     visual tree if the <c>end</c> visual is not an ancestor. The list is returned in tree
        ///     hierarchy order, i.e. <c>end</c> / root first
        /// </summary>
        public static DependencyObject[] GetVisualAncestors(DependencyObject start, DependencyObject end)
        {
            var Ancestors = new List<DependencyObject>();
            var Parent = start;
            while (Parent != null && Parent != end)
            {
                Parent = VisualTreeHelper.GetParent(Parent);
                Ancestors.Add(Parent);
            }

            Ancestors.Reverse();
            return Ancestors.ToArray();
        }

        /// <summary>
        ///     Returns this first ancestor of the given type
        /// </summary>
        public static T GetVisualAncestor<T>(this DependencyObject start) where T : DependencyObject
        {
            var Parent = VisualTreeHelper.GetParent(start);
            while (Parent != null)
            {
                if (Parent is T) return (T) Parent;
                Parent = VisualTreeHelper.GetParent(Parent);
            }

            return null;
        }

        /// <summary>
        ///     Returns the bounds of the element, at location 0,0
        /// </summary>
        public static Rect GetBounds(Visual uiElement)
        {
            if (uiElement is FrameworkElement)
                return new Rect(0, 0, ((FrameworkElement) uiElement).ActualWidth,
                    ((FrameworkElement) uiElement).ActualHeight);
            return VisualTreeHelper.GetDescendantBounds(uiElement);
        }

        /// <summary>
        ///     Returns the bounds of the element, at location within reference
        /// </summary>
        public static Rect GetBounds(Visual reference, Visual uiElement)
        {
            var Bounds = GetBounds(uiElement);
            return uiElement.TransformToVisual(reference).TransformBounds(Bounds);
        }

        /// <summary>
        ///     Returns the visual child of the given element that is under position
        /// </summary>
        public static Visual GetHitChild(this Visual element, Point position)
        {
            for (var Index = 0; Index < VisualTreeHelper.GetChildrenCount(element); Index++)
            {
                var Child = VisualTreeHelper.GetChild(element, Index) as Visual;
                if (Child != null && GetBounds(element, Child).Contains(position)) return Child;
            }

            return null;
        }
    }
}