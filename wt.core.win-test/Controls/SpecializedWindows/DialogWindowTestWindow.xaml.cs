using System.Windows;
using System.Windows.Media;

namespace WhileTrue.Controls
{
    /// <summary>
    ///     Interaction logic for DialogWindowTestWindow.xaml
    /// </summary>
    public partial class DialogWindowTestWindow
    {
        private readonly ImageSource imageSource;

        public DialogWindowTestWindow()
        {
            InitializeComponent();
            imageSource = DialogTitleImage;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ClearValue(DialogSubtitleProperty);
            ClearValue(DialogTitleProperty);
            ClearValue(DialogTitleImageSourceProperty);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogTitleImage = imageSource;
        }
    }
}