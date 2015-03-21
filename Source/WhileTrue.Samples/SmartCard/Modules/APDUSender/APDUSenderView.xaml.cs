using WhileTrue.Classes.Components;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    /// <summary/>
    [Component]
    partial class APDUSenderView : IAPDUSenderView
    {
        public APDUSenderView()
        {
            InitializeComponent();
        }

        public IAPDUSenderModel Model
        {
            set { this.DataContext = value; }
        }

        public void Open()
        {
            this.ShowDialog();
        }
    }
}
