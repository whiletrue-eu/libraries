using System.Windows.Input;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Commanding._Unittest
{
    /// <summary>
    /// Interaction logic for TestWindows.xaml
    /// </summary>
    public partial class TestWindow
    {
        private readonly SampleModel sampleModel;

        public TestWindow()
        {
            InitializeComponent();
            this.sampleModel = new SampleModel();
            this.DataContext = sampleModel;
        }
    }

    public class SampleModel : ObservableObject
    {
        public SampleModel()
        {
            this.command = new DelegateCommand<bool>(
                delegate {
                    this.CommandResult = string.Format("Clicked: {0} times", this.clickCount++);
                },
                param=>param
                );
        }

        public ICommand SampleCommand
        {
            get
            {
                return this.command;
            }
        }

        public string CommandResult
        {
            get {
                return this.commandResult;
            }
            set {this.SetAndInvoke(()=>CommandResult, ref this.commandResult, value);
            }
        }

        private bool sampleCommandParameter;
        private int clickCount;
        private string commandResult ="Not clicked yet.";
        private readonly DelegateCommand<bool> command;

        public bool SampleCommandParameter
        {
            get { return this.sampleCommandParameter; }
            set 
            { 
                this.SetAndInvoke(()=>SampleCommandParameter, ref this.sampleCommandParameter, value);
            }
        }
    }
}
