using NUnit.Framework;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Interaction logic for DialogPanelTestContainer.xaml
    /// </summary>
    partial class ContentUnavailableTestContainer 
    {
        public ContentUnavailableTestContainer()
        {
            this.InitializeComponent();
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
