using Foundation;
using UIKit;
using wt.Classes.Forms;
using WhileTrue.Classes.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.PlatformConfiguration;

[assembly: ExportRenderer(typeof(GestureView), typeof(GestureViewRenderer))]

namespace wt.Classes.Forms
{
    public class GestureViewRenderer : ViewRenderer<GestureView, UIView>
    {

        class View : UIView
        {
            private readonly GestureViewRenderer parent;

            public View(GestureViewRenderer parent)
            {
                this.parent = parent;
            }

            public override void TouchesBegan(NSSet touches, UIEvent evt)
            {
                base.TouchesBegan(touches, evt);
                if(this.parent.Element!=null)
                {
                    this.parent.Element.IsPressed = true;
                }
            }

            public override void TouchesEnded(NSSet touches, UIEvent evt)
            {
                base.TouchesEnded(touches, evt);
                if(this.parent.Element!=null)
                {
                    this.parent.Element.IsPressed = false;
                }
            }

            public override void TouchesCancelled(NSSet touches, UIEvent evt)
            {
                base.TouchesCancelled(touches, evt);
                if(this.parent.Element!=null)
                {
                    this.parent.Element.IsPressed = false;
                }
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<GestureView> e)
        {
            base.OnElementChanged(e);

            if (this.Control == null)
            {
                this.SetNativeControl(new View(this));
            }


            if (e.OldElement != null)
            {
                e.OldElement.IsPressed = false;
            }
        }
    }
}