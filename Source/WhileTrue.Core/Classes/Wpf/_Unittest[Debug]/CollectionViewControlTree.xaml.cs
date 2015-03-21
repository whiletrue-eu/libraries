
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WhileTrue.Classes._Unittest
{
    /// <summary>
    /// Interaction logic for UIFeatureManagementControlTree.xaml
    /// </summary>
    public partial class CollectionViewControlTree
    {
        public CollectionViewControlTree()
        {
            InitializeComponent();
            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); //Force visual tree to construct
        }
    }
}


