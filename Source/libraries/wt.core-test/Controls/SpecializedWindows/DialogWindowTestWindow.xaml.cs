using System.Windows;
using System.Windows.Media;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Interaction logic for DialogWindowTestWindow.xaml
    /// </summary>
    public partial class DialogWindowTestWindow 
    {
        private readonly ImageSource imageSource;

        public DialogWindowTestWindow()
        {
            this.InitializeComponent();
            this.imageSource = this.DialogTitleImage; 
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.ClearValue(DialogWindow.DialogSubtitleProperty);
            this.ClearValue(DialogWindow.DialogTitleProperty);
            this.ClearValue(DialogWindow.DialogTitleImageSourceProperty);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogTitleImage = this.imageSource;
        }
    }
}
