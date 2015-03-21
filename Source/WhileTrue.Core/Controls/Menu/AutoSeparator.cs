using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WhileTrue.Controls
{
    public class AutoSeparator : Separator
    {
        private UIElement parent;

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

                if (NoVisibleSiblingExists(PreviousChildren) || NoVisibleSiblingExists(FollowingChildren) || SeparatorAlreadyThere(FollowingChildren))
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
                if (Child is Separator && IsVisible(Child))
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
                if (IsVisible(Child)) return false;
            }

            return true;
        }

        private new static bool IsVisible(DependencyObject Child)
        {
            if (Child is UIElement)
            {
                if (((UIElement) Child).Visibility == Visibility.Visible)
                {
                    return true;
                }
            }
            return false;
        }
    }
}