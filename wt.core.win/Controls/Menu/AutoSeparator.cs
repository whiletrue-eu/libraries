using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JetBrains.Annotations;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Implements a menu seperator that automatically gets invisible, if surrounding menu items get invisble as well.
    /// I.e., it will be invisible, if it is the first or last item shown, or if two subsequent seperators would be shown.
    /// </summary>
    [PublicAPI]
    public class AutoSeparator : Separator
    {
        private UIElement parent;

        /// <summary>
        /// Invoked when the parent of this element in the visual tree is changed. Overrides <see cref="M:System.Windows.UIElement.OnVisualParentChanged(System.Windows.DependencyObject)"/>.
        /// </summary>
        /// <param name="oldParent">The old parent element. May be null to indicate that the element did not have a visual parent previously.</param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (this.parent != null)
            {
                this.parent.LayoutUpdated -= this.DynamicSeparator_LayoutUpdated;
                this.parent = null;
            }

            base.OnVisualParentChanged(oldParent);

            if (this.VisualParent is UIElement)
            {
                this.parent = (UIElement) this.VisualParent;
                this.parent.LayoutUpdated += this.DynamicSeparator_LayoutUpdated;
            }
        }

        private void DynamicSeparator_LayoutUpdated(object sender, EventArgs e)
        {
            this.UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (this.parent != null)
            {
                List<DependencyObject> PreviousChildren = new List<DependencyObject>();
                List<DependencyObject> FollowingChildren = new List<DependencyObject>();

                bool FoundMyself = false;
                for (int Index = 0; Index < VisualTreeHelper.GetChildrenCount(this.parent); Index++)
                {
                    DependencyObject Child = VisualTreeHelper.GetChild(this.parent, Index);
                    // ReSharper disable once PossibleUnintendedReferenceComparison
                    if (Child == this)
                    {
                        FoundMyself = true;
                    }
                    else
                    {
                        if (FoundMyself)
                        {
                            FollowingChildren.Add(Child);
                        }
                        else
                        {
                            PreviousChildren.Add(Child);
                        }
                    }
                }

                PreviousChildren.Reverse();

                if (AutoSeparator.NoVisibleSiblingExists(PreviousChildren) || AutoSeparator.NoVisibleSiblingExists(FollowingChildren) || AutoSeparator.SeparatorAlreadyThere(FollowingChildren))
                {
                    this.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.Visibility = Visibility.Visible;
                }
            }
        }


        private static bool SeparatorAlreadyThere(List<DependencyObject> children)
        {
            foreach (DependencyObject Child in children)
            {
                if (Child is Separator && AutoSeparator.IsVisible(Child))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private static bool NoVisibleSiblingExists(List<DependencyObject> children)
        {
            foreach (DependencyObject Child in children)
            {
                if (AutoSeparator.IsVisible(Child)) return false;
            }

            return true;
        }

#pragma warning disable 109
        private new static bool IsVisible(DependencyObject child)
        {
            if ((child as UIElement)?.Visibility == Visibility.Visible)
            {
                return true;
            }
            return false;
        }
#pragma warning restore 109
    }
}