using System.Timers;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Interaction logic for DialogPanelTestContainer.xaml
    /// </summary>
    partial class DialogPanelTestContainer 
    {
        public DialogPanelTestContainer()
        {
            this.InitializeComponent();
        }

    }

    [TestFixture]
    public class DialogPanelTest
    {
        [Test,Ignore("Manual test")]
        public void Test()
        {
            System.Windows.Window Window = new System.Windows.Window();
            Window.Content = new DialogPanelTestContainer();
            Window.DataContext = new DialogPanelTestContainerValues();
            Window.ShowDialog();

        }
    }


    public class DialogPanelTestContainerValues:ObservableObject
        {
        public DialogPanelTestContainerValues()
        {
            this.timer = new Timer(500);
            this.timer.Elapsed += delegate
                                      {
                                          this.Value += ".";
                                          if (this.Value.Length == 20)
                                          {
                                              this.Value = "";
                                          }
                                          this.InvokePropertyChanged(nameof(this.Value));
                                      };
            this.timer.Start();
        }

        private readonly Timer timer;

        public string Value { get; private set; } = "";
        }
}
