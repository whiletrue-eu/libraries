using WhileTrue.Classes.Components;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Components.WPFSmartCardUI
{
    /// <summary>
    /// Interaction logic for SmartCardSelectionView.xaml
    /// </summary>
    [Component]
    partial class SmartCardSelectionView : ISmartCardSelectionView
    {
        public SmartCardSelectionView()
        {
            this.InitializeComponent();
        }

        public void ShowModal()
        {
            this.ShowDialog();
            if (this.DialogResult != true)
            {
                throw new UserCancelException();
            }
        }

        public ISmartCardSelectionModel Model
        {
            set { this.DataContext = value; }
        }
    }
}
