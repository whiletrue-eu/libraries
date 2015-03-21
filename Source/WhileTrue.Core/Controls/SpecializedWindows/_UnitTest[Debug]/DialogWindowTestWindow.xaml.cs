using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WhileTrue.Controls.SpecializedWindows._UnitTest_Debug_
{
    /// <summary>
    /// Interaction logic for DialogWindowTestWindow.xaml
    /// </summary>
    public partial class DialogWindowTestWindow 
    {
        private readonly ImageSource imageSource;

        public DialogWindowTestWindow()
        {
            InitializeComponent();
            this.imageSource = this.DialogTitleImage; 
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.ClearValue(DialogSubtitleProperty);
            this.ClearValue(DialogTitleProperty);
            this.ClearValue(DialogTitleImageSourceProperty);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogTitleImage = this.imageSource;
        }
    }
}
