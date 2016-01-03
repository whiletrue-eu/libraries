using System.Timers;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Interaction logic for DialogPanelTestContainer.xaml
    /// </summary>
    partial class TablePanelTestContainer 
    {
        public TablePanelTestContainer()
        {
            this.InitializeComponent();
        }

    }

    [TestFixture]
    public class TablePanelTest
    {
        [Test,Ignore("Manual test")]
        public void Test()
        {
            System.Windows.Window Window = new System.Windows.Window();
            Window.Content = new TablePanelTestContainer();
            Window.DataContext = new TablePanelTestContainerValues();
            Window.ShowDialog();

        }
    }


    public class TablePanelTestContainerValues:ObservableObject
        {
        public TablePanelTestContainerValues()
        {
            this.timer = new Timer(500);
            this.timer.Elapsed += delegate
                                      {
                                          this.Value += ".";
                                          if (this.Value.Length == 20)
                                          {
                                              this.Value = "";
                                          }
                                      };
            this.timer.Start();
        }

        private readonly Timer timer;
        private string value;

        public string Value
        {
            get { return this.value; }
            private set { this.SetAndInvoke(ref this.value, value); }
        }
        }
}
