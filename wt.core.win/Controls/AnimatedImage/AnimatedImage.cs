using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;
using JetBrains.Annotations;


namespace WhileTrue.Controls
{
    /// <summary>
    /// implements an image that support aniation of different frames stored within the image (e.g. animated gif)
    /// </summary>
    [PublicAPI]
    public class AnimatedImage : Control
    {
        private Bitmap animatedBitmap;
        private Stream animatedBitmapStream;

        /// <summary>
        /// Gets whether the controls is currently animating the image
        /// </summary>
        public bool IsAnimating { get; private set; }

        /// <summary>
        /// Contains the current frame bitmap to render
        /// </summary>
        public static readonly DependencyProperty CurrentFrameProperty;
        /// <summary>
        /// Gets/Sets the stretch direction of the Viewbox, which determines the restrictions on
        /// scaling that are applied to the content inside the Viewbox.  For instance, this property
        /// can be used to prevent the content from being smaller than its native size or larger than
        /// its native size.
        /// </summary>
        public static readonly System.Windows.DependencyProperty StretchDirectionProperty;
        /// <summary>
        /// Gets/Sets the Stretch on this Image.
        /// The Stretch property determines how large the Image will be drawn.
        /// </summary>
        public static readonly System.Windows.DependencyProperty StretchProperty;

        static AnimatedImage()
        {
            AnimatedImage.CurrentFrameProperty = DependencyProperty.Register(
                "CurrentFrame",
                typeof(ImageSource),
                typeof(AnimatedImage)
                );
            AnimatedImage.StretchDirectionProperty = DependencyProperty.Register(
                "StretchDirection",
                typeof(StretchDirection),
                typeof(AnimatedImage)
                );
            AnimatedImage.StretchProperty = DependencyProperty.Register(
                "Stretch",
                typeof(Stretch),
                typeof(AnimatedImage)
                );

            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedImage), new FrameworkPropertyMetadata(typeof(AnimatedImage)));
        }

        /// <summary>
        /// Uri of the animated image
        /// </summary>
        public Uri AnimatedBitmap
        {
            get { return (Uri)this.GetValue(AnimatedImage.AnimatedBitmapProperty); }
            set { this.SetValue(AnimatedImage.AnimatedBitmapProperty, value); }
        }

        /// <summary>
        /// Contains the current frame bitmap to render
        /// </summary>
        public ImageSource CurrentFrame
        {
            get { return (ImageSource)this.GetValue(AnimatedImage.CurrentFrameProperty); }
            private set { this.SetValue(AnimatedImage.CurrentFrameProperty, value); }
        }

        /// <summary>
        /// Gets/Sets the Stretch on this Image.
        /// The Stretch property determines how large the Image will be drawn.
        /// </summary>
        public Stretch Stretch
        {
            get { return (Stretch)this.GetValue(AnimatedImage.StretchProperty); }
            set { this.SetValue(AnimatedImage.StretchProperty, value); }
        }

        /// <summary>
        /// Gets/Sets the stretch direction of the Viewbox, which determines the restrictions on
        /// scaling that are applied to the content inside the Viewbox.  For instance, this property
        /// can be used to prevent the content from being smaller than its native size or larger than
        /// its native size.
        /// </summary>
        public StretchDirection StretchDirection
        {
            get { return (StretchDirection)this.GetValue(AnimatedImage.StretchDirectionProperty); }
            set { this.SetValue(AnimatedImage.StretchDirectionProperty, value); }
        }

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty AnimatedBitmapProperty =
            DependencyProperty.Register(
                "AnimatedBitmap", typeof(Uri), typeof(AnimatedImage),
                new FrameworkPropertyMetadata(null, AnimatedImage.OnAnimatedBitmapChanged));

        private ImageSource[] frames;
        private int currentFrameNumber;

        private static void OnAnimatedBitmapChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            AnimatedImage Control = (AnimatedImage)obj;

            Control.UpdateAnimatedBitmap();
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        private void UpdateAnimatedBitmap()
        {
            this.StopAnimate();

            if (this.animatedBitmapStream != null)
            {
                this.animatedBitmapStream.Close();
                this.animatedBitmapStream = null;
                this.frames = null;
                this.currentFrameNumber = 0;
            }

            if (this.AnimatedBitmap != null)
            {
                Stream ImageStream;
                try
                {
                    if( this.AnimatedBitmap.IsAbsoluteUri && File.Exists(this.AnimatedBitmap.AbsoluteUri ) )
                    {
                        ImageStream = File.OpenRead(this.AnimatedBitmap.AbsoluteUri);
                    }
                    else
                    {
                        StreamResourceInfo Resource = Application.GetResourceStream(this.AnimatedBitmap);
                        if( Resource == null)
                        {
                            throw new InvalidOperationException($"Resource '{this.AnimatedBitmap.OriginalString}' not found!");
                        }
                        else
                        {
                            ImageStream = Resource.Stream;
                        }
                    }
                }
                catch (Exception E)
                {
                    throw new InvalidOperationException($"Image could not be loaded: '{this.AnimatedBitmap.OriginalString}'", E);
                }

                this.animatedBitmapStream = ImageStream;
                this.animatedBitmap = new System.Drawing.Bitmap(this.animatedBitmapStream);

                int FrameCount = this.animatedBitmap.GetFrameCount(FrameDimension.Time);
                this.frames = new ImageSource[FrameCount];


                for (int FrameNumber = 0; FrameNumber < FrameCount; FrameNumber++)
                {
                    this.animatedBitmap.SelectActiveFrame(FrameDimension.Time, FrameNumber);
                    this.animatedBitmap.MakeTransparent();
                    IntPtr BitmapHandle = this.animatedBitmap.GetHbitmap();
                    this.frames[FrameNumber] = Imaging.CreateBitmapSourceFromHBitmap(
                        BitmapHandle,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    AnimatedImage.DeleteObject(BitmapHandle);
                }

            }

            this.StartAnimate();
        }

        private void OnFrameChanged(object o, EventArgs e)
        {
            this.ChangeSource();
        }

        void ChangeSource()
        {
            if (this.frames != null)
            {
                this.currentFrameNumber = (this.currentFrameNumber + 1) % this.frames.Length;
                ImageSource NextFrame = this.frames[this.currentFrameNumber];
                this.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                                            (Action)delegate
                                                {
                                                    this.CurrentFrame = NextFrame;
                                                });
            }
        }

        private void StopAnimate()
        {
            if (this.IsAnimating)
            {
                ImageAnimator.StopAnimate(this.animatedBitmap, this.OnFrameChanged);
                this.IsAnimating = false;
            }
        }

        private void StartAnimate()
        {
            if (!this.IsAnimating && ImageAnimator.CanAnimate(this.animatedBitmap))
            {

                ImageAnimator.Animate(this.animatedBitmap, this.OnFrameChanged);
                this.IsAnimating = true;
            }
        }
    }
}