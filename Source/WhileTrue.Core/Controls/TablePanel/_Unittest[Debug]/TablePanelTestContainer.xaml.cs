using System.Timers;
using System.Windows;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls._Unittest
{
    /// <summary>
    /// Interaction logic for DialogPanelTestContainer.xaml
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
            /*this.timer = new Timer(500);
            this.timer.Elapsed += delegate
                                      {
                                          this.value += ".";
                                          if (this.value.Length == 20)
                                          {
                                              this.value = "";
                                          }
                                          this.InvokePropertyChanged(() => Value);
                                      };
            this.timer.Start();*/
        }

        private string value = "";
        private readonly Timer timer;

        public string Value { get { return this.value; } }
        }
}
