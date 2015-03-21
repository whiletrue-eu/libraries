using WhileTrue.Classes.Components;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.Wpf;
using WhileTrue.Components.SmartCardUI;
using WhileTrue.Controls;

namespace WhileTrue.Components.WPFSmartCardUI
{
    /// <summary>
    /// Interaction logic for SmartCardSelectionView.xaml
    /// </summary>
    [Component]
    partial class SmartCardSelectionView : DialogWindow, ISmartCardSelectionView
    {
        public SmartCardSelectionView()
        {
            InitializeComponent();
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
