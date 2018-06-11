using Android.Views;
using wt.Classes.Forms;
using WhileTrue.Classes.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(GestureView), typeof(GestureViewRenderer))]

namespace wt.Classes.Forms
{
    public class GestureViewRenderer : Xamarin.Forms.Platform.Android.AppCompat.ViewRenderer<GestureView, Android.Views.View>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<GestureView> e)
        {
            base.OnElementChanged(e);

            if (this.Control == null)
            {
                this.SetNativeControl(new Android.Views.View(this.Context));
            }


            if (e.OldElement != null)
            {
                e.OldElement.IsPressed = false;
                this.Control.Touch -= this.Control_Touch;
            }
            if (e.NewElement != null)
            {
                this.Control.Touch += this.Control_Touch;
            }
        }


        private void Control_Touch(object sender, TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    this.Element.IsPressed = true;
                    break;
                case MotionEventActions.Up:
                    this.Element.IsPressed = false;
                    break;
            }
        }
    }
}