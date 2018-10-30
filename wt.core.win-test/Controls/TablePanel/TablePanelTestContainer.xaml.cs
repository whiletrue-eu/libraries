using System.Timers;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
    /// <summary>
    ///     Interaction logic for DialogPanelTestContainer.xaml
    /// </summary>
    partial class TablePanelTestContainer
    {
        public TablePanelTestContainer()
        {
            InitializeComponent();
        }
    }

    [TestFixture]
    public class TablePanelTest
    {
        [Test]
        [Ignore("Manual test")]
        public void Test()
        {
            var Window = new System.Windows.Window();
            Window.Content = new TablePanelTestContainer();
            Window.DataContext = new TablePanelTestContainerValues();
            Window.ShowDialog();
        }
    }


    public class TablePanelTestContainerValues : ObservableObject
    {
        private readonly Timer timer;
        private string value;

        public TablePanelTestContainerValues()
        {
            timer = new Timer(500);
            timer.Elapsed += delegate
            {
                Value += ".";
                if (Value.Length == 20) Value = "";
            };
            timer.Start();
        }

        public string Value
        {
            get => value;
            private set => SetAndInvoke(ref this.value, value);
        }
    }
}