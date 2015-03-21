using WhileTrue.Classes.Components;

namespace AtrEditor.About
{
    /// <summary/>
    [Component]
    public partial class AboutWindowView : IAboutWindowView
    {
        public AboutWindowView()
        {
            InitializeComponent();
        }

        public void ShowModal()
        {
            this.ShowDialog();
        }

        public AboutWindow Model { set { this.DataContext = value; } }
    }
}
