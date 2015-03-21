using System.Timers;
using System.Windows;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls._Unittest
{
    /// <summary>
    /// Interaction logic for DialogPanelTestContainer.xaml
    /// </summary>
    partial class ContentUnavailableTestContainer 
    {
        public ContentUnavailableTestContainer()
        {
            InitializeComponent();
        }

    }

    [TestFixture]
    public class ContentUnavailableTest
    {
        [Test,Ignore("Manual test")]
        public void Test()
        {
            System.Windows.Window Window = new System.Windows.Window();
            Window.Content = new ContentUnavailableTestContainer();
            Window.ShowDialog();
        }
    }

}
