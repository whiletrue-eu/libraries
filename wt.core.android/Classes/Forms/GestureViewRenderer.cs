using Android.Content;
using Android.Views;
using wt.Classes.Forms;
using WhileTrue.Classes.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(GestureView), typeof(GestureViewRenderer))]

namespace wt.Classes.Forms
{
    public class GestureViewRenderer : Xamarin.Forms.Platform.Android.AppCompat.ViewRenderer<GestureView, View>
    {
        public GestureViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<GestureView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
                SetNativeControl(new View(Context));

            if (e.OldElement != null)
            {
                e.OldElement.IsPressed = false;
                Control.Touch -= Control_Touch;
            }

            if (e.NewElement != null)
                Control.Touch += Control_Touch;
        }

        private void Control_Touch(object sender, TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    Element.IsPressed = true;
                    break;
                case MotionEventActions.Up:
                    Element.IsPressed = false;
                    break;
            }
        }
    }
}