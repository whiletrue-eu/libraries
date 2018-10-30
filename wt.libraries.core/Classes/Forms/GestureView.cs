using Xamarin.Forms;

namespace WhileTrue.Classes.Forms
{
    public class GestureView : ContentView
    {
        public static readonly BindableProperty IsPressedProperty =
            BindableProperty.Create(@"IsPressed", typeof(bool), typeof(GestureView), false, BindingMode.TwoWay);

        public bool IsPressed
        {
            set => SetValue(IsPressedProperty, value);
            get => (bool) GetValue(IsPressedProperty);
        }
    }
}