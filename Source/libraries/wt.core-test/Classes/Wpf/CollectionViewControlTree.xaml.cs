using System.Windows;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Interaction logic for UIFeatureManagementControlTree.xaml
    /// </summary>
    public partial class CollectionViewControlTree
    {
        public CollectionViewControlTree()
        {
            this.InitializeComponent();
            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); //Force visual tree to construct
        }
    }
}


