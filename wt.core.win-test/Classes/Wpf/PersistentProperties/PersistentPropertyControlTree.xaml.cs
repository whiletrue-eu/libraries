﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WhileTrue.Classes.Wpf.PersistentProperties
{
    /// <summary>
    ///     Interaction logic for UIFeatureManagementControlTree.xaml
    /// </summary>
    public partial class PersistentPropertyControlTree
    {
        public PersistentPropertyControlTree()
        {
            InitializeComponent();
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); //Force visual tree to construct
        }

        public TextBox DataTemplate => (TextBox) Find(this, "DataTemplate");

        private static DependencyObject Find(DependencyObject item, string name)
        {
            for (var Index = 0; Index < VisualTreeHelper.GetChildrenCount(item); Index++)
            {
                var Child = VisualTreeHelper.GetChild(item, Index);
                if (Child is FrameworkElement)
                    if (((FrameworkElement) Child).Name == name)
                        return Child;
                var Descendant = Find(Child, name);
                if (Descendant != null) return Descendant;
            }

            return null;
        }
    }
}