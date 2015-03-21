
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WhileTrue.Classes._Unittest
{
    /// <summary>
    /// Interaction logic for UIFeatureManagementControlTree.xaml
    /// </summary>
    public partial class ValidationTestControlTree
    {
        public ValidationTestControlTree()
        {
            InitializeComponent();
            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); //Force visual tree to construct
        }

        private static DependencyObject Find(DependencyObject item, string name)
        {
            for(int Index=0; Index < VisualTreeHelper.GetChildrenCount(item); Index++)
            {
                DependencyObject Child = VisualTreeHelper.GetChild(item, Index);
                if( Child is FrameworkElement)
                {
                    if (((FrameworkElement)Child).Name == name)
                    {
                        return Child;
                    }
                }
                DependencyObject Descendant = Find(Child, name);
                if (Descendant != null)
                {
                    return Descendant;
                }
            }
            return null;
        }
    }
}