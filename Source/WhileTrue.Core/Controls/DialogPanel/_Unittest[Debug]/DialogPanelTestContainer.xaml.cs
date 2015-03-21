using System.Timers;
using System.Windows;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls._Unittest
{
    /// <summary>
    /// Interaction logic for DialogPanelTestContainer.xaml
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
                                          this.value += ".";
                                          if (this.value.Length == 20)
                                          {
                                              this.value = "";
                                          }
                                          this.InvokePropertyChanged(() => Value);
                                      };
            this.timer.Start();
        }

        private string value = "";
        private readonly Timer timer;

        public string Value { get { return this.value; } }
        }
}
