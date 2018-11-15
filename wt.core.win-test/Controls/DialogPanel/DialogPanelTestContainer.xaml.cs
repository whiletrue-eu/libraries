using System.Timers;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
    /// <summary>
    ///     Interaction logic for DialogPanelTestContainer.xaml
    /// </summary>
    partial class DialogPanelTestContainer
    {
        public DialogPanelTestContainer()
        {
            InitializeComponent();
        }
    }

    [TestFixture]
    public class DialogPanelTest
    {
        [Test]
        [Ignore("Manual test")]
        public void Test()
        {
            var Window = new System.Windows.Window();
            Window.Content = new DialogPanelTestContainer();
            Window.DataContext = new DialogPanelTestContainerValues();
            Window.ShowDialog();
        }
    }


    public class DialogPanelTestContainerValues : ObservableObject
    {
        private readonly Timer timer;

        public DialogPanelTestContainerValues()
        {
            timer = new Timer(500);
            timer.Elapsed += delegate
            {
                Value += ".";
                if (Value.Length == 20) Value = "";
                InvokePropertyChanged(nameof(Value));
            };
            timer.Start();
        }

        public string Value { get; private set; } = "";
    }
}