using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WhileTrue.Classes.UIFeatures
{
    /// <summary>
    /// Interaction logic for UIFeatureManagementControlTree.xaml
    /// </summary>
    public partial class UiFeatureManagementControlTree
    {
        public UiFeatureManagementControlTree()
        {
            this.InitializeComponent();
            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); //Force visual tree to construct
        }

        public TextBox TemplatedThree => (TextBox)this.Find(this, "TemplatedThree");

        private DependencyObject Find(DependencyObject item, string name)
        {
            for (int Index = 0; Index < VisualTreeHelper.GetChildrenCount(item); Index++)
            {
                DependencyObject Child = VisualTreeHelper.GetChild(item, Index);
                if (Child is FrameworkElement)
                {
                    if (((FrameworkElement)Child).Name == name)
                    {
                        return Child;
                    }
                }
                DependencyObject Descendant = this.Find(Child, name);
                if (Descendant != null)
                {
                    return Descendant;
                }
            }
            return null;
        }
    }
}

