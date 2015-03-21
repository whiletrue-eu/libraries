// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using NUnit.Framework;
using WhileTrue.Controls;

namespace WhileTrue.Classes.Wpf
{
    [TestFixture]
    public class CollectionViewTest
    {

        [Test, Ignore("Manual Test")]
        public void ShowControls()
        {
            Window Window = new Window();
            Window.Content = new CollectionViewControlTree();
            Window.DataContext = new CollectionViewBackingData();

            Window.ShowDialog();
        }


    }
}

