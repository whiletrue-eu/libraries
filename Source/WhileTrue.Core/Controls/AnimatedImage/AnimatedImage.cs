using System;
using System.ComponentModel;
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
using WhileTrue.Classes.Utilities;


namespace WhileTrue.Controls
{
    public class AnimatedImage : Control
    {
        private Bitmap animatedBitmap;
        private Stream animatedBitmapStream;

        private bool isAnimating;

        public bool IsAnimating
        {
            get { return this.isAnimating; }
        }

        public static readonly DependencyProperty CurrentFrameProperty;
        public static readonly System.Windows.DependencyProperty StretchDirectionProperty;
        public static readonly System.Windows.DependencyProperty StretchProperty;

        static AnimatedImage()
        {
            CurrentFrameProperty = DependencyProperty.Register(
                "CurrentFrame",
                typeof(ImageSource),
                typeof(AnimatedImage)
                );
            StretchDirectionProperty = DependencyProperty.Register(
                "StretchDirection",
                typeof(StretchDirection),
                typeof(AnimatedImage)
                );
            StretchProperty = DependencyProperty.Register(
                "Stretch",
                typeof(Stretch),
                typeof(AnimatedImage)
                );

            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedImage), new FrameworkPropertyMetadata(typeof(AnimatedImage)));
        }

        public Uri AnimatedBitmap
        {
            get { return (Uri)this.GetValue(AnimatedBitmapProperty); }
            set { this.SetValue(AnimatedBitmapProperty, value); }
        }

        public ImageSource CurrentFrame
        {
            get { return (ImageSource)this.GetValue(CurrentFrameProperty); }
            private set { this.SetValue(CurrentFrameProperty, value); }
        }

        public Stretch Stretch
        {
            get { return (Stretch)this.GetValue(StretchProperty); }
            set { this.SetValue(StretchProperty, value); }
        }

        public StretchDirection StretchDirection
        {
            get { return (StretchDirection)this.GetValue(StretchDirectionProperty); }
            set { this.SetValue(StretchDirectionProperty, value); }
        }

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty AnimatedBitmapProperty =
            DependencyProperty.Register(
                "AnimatedBitmap", typeof(Uri), typeof(AnimatedImage),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnAnimatedBitmapChanged)));

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
                            throw new InvalidOperationException(string.Format("Resource '{0}' not found!", this.AnimatedBitmap.OriginalString));
                        }
                        else
                        {
                            ImageStream = Resource.Stream;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(string.Format("Image could not be loaded: '{0}'", this.AnimatedBitmap.OriginalString), e);
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
                    DeleteObject(BitmapHandle);
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

        public void StopAnimate()
        {
            if (this.isAnimating)
            {
                ImageAnimator.StopAnimate(this.animatedBitmap, this.OnFrameChanged);
                this.isAnimating = false;
            }
        }

        public void StartAnimate()
        {
            if (!this.isAnimating && ImageAnimator.CanAnimate(this.animatedBitmap))
            {

                ImageAnimator.Animate(this.animatedBitmap, this.OnFrameChanged);
                this.isAnimating = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}