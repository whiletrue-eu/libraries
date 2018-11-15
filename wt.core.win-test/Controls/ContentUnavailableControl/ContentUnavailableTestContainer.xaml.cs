using NUnit.Framework;

namespace WhileTrue.Controls
{
    /// <summary>
    ///     Interaction logic for DialogPanelTestContainer.xaml
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
        [Test]
        [Ignore("Manual Test")]
        public void Test()
        {
            var window = new System.Windows.Window {Content = new ContentUnavailableTestContainer()};
            window.ShowDialog();
        }
    }
}