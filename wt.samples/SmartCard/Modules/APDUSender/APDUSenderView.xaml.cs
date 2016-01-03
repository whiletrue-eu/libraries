using WhileTrue.Classes.Components;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    /// <summary/>
    [Component]
    partial class ApduSenderView : IApduSenderView
    {
        public ApduSenderView()
        {
            this.InitializeComponent();
        }

        public IApduSenderModel Model
        {
            set { this.DataContext = value; }
        }

        public void Open()
        {
            this.ShowDialog();
        }
    }
}
