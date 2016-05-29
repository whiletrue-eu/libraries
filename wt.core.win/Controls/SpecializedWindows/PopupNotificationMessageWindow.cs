using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls
{

    /// <summary>
    /// Implements a popup toast window that appears fading in from where the system tray is located, adoptinng to the taskbar location of the screen
    /// </summary>
    public class PopupNotificationMessageWindow : NotificationMessageWindow
    {
        /// <summary/>
        public static readonly DependencyProperty AutoFadeOutSecondsProperty;

        static PopupNotificationMessageWindow()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupNotificationMessageWindow), new FrameworkPropertyMetadata(typeof(PopupNotificationMessageWindow)));

            PopupNotificationMessageWindow.AutoFadeOutSecondsProperty = DependencyProperty.Register(
                "AutoFadeOutSeconds",
                typeof(int),
                typeof(PopupNotificationMessageWindow),
                new FrameworkPropertyMetadata(0, PopupNotificationMessageWindow.AutoFadeOutSecondsChanged)
                );
        }

        private static void AutoFadeOutSecondsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((PopupNotificationMessageWindow)sender).AutoFadeOutSecondsChanged();
        }

        private readonly AutoFadeOutWatcherThread autoFadeOutWatcher;
        private readonly AutoResetEvent autoFadeOutSecondsEvent = new AutoResetEvent(false);
        private bool fadedIn;

        /// <summary/>
        public PopupNotificationMessageWindow()
        {
            this.autoFadeOutWatcher = new AutoFadeOutWatcherThread(this);
            this.autoFadeOutWatcher.Start();
        }

        /// <summary>
        /// Seconds to wait before fading out
        /// </summary>
        public int AutoFadeOutSeconds
        {
            get { return (int)this.GetValue(PopupNotificationMessageWindow.AutoFadeOutSecondsProperty); }
            set { this.SetValue(PopupNotificationMessageWindow.AutoFadeOutSecondsProperty, value); }
        }

        ///<summary>
        /// Fades in, waiting for the user to dismiss the notification
        ///</summary>
        public override void FadeIn()
        {
            //suppress fade in, if already shown, but set seconds to 0 to disable auto fade out
            this.AutoFadeOutSeconds = 0;
            if (this.fadedIn == false)
            {
                this.fadedIn = true;
                base.FadeIn();
            }
        }
        
        /// <summary>
        /// Start fading in, wating <c>autoFadeOutSeconds</c> before automatically fading out
        /// </summary>
        public void FadeIn(int autoFadeOutSeconds)
        {
            //fade in if not already shown, if shown and auto fasde out is set (>0), reset the auto fade value
            if( this.fadedIn &&this.AutoFadeOutSeconds>0 )
            {
                this.AutoFadeOutSeconds = autoFadeOutSeconds;
            }

            if (this.fadedIn == false)
            {
                this.fadedIn = true;
                this.AutoFadeOutSeconds = autoFadeOutSeconds;
                base.FadeIn();
            }
        }

        ///<summary>
        /// Fade out the notification
        ///</summary>
        public override void FadeOut()
        {
            //fade out only if currently shpwn
            if (this.fadedIn)
            {
                this.fadedIn = false;
                base.FadeOut();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void NotifyAutoHide()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)this.Close);
        }

        private void AutoFadeOutSecondsChanged()
        {
            if( this.AutoFadeOutSeconds > 0 )
            {
                this.autoFadeOutSecondsEvent.Set();
            }
        }

        private class AutoFadeOutWatcherThread : ThreadBase
        {
            private readonly PopupNotificationMessageWindow owner;

            public AutoFadeOutWatcherThread(PopupNotificationMessageWindow owner)
                : base("Notification Auto Fade Out Thread", isBackgroundThread: true)
            {
                this.owner = owner;
            }

            protected override void Run()
            {
                while (true)
                {
                    int AutoFadeOutSeconds = 0;
                    this.owner.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                 (Action)delegate
                                                 {
                                                     AutoFadeOutSeconds = this.owner.AutoFadeOutSeconds;
                                                 });
                    if (AutoFadeOutSeconds > 1)
                    {
                        AutoFadeOutSeconds--;
                    }
                    else if (AutoFadeOutSeconds == 1)
                    {
                        this.owner.NotifyAutoHide();
                        AutoFadeOutSeconds = 0;
                        this.owner.autoFadeOutSecondsEvent.WaitOne();
                    }
                    this.owner.Dispatcher.Invoke(DispatcherPriority.Normal,
                                 (Action)delegate
                                 {
                                     this.owner.AutoFadeOutSeconds = AutoFadeOutSeconds;
                                 });
                    this.Sleep(1000);
                }
                // ReSharper disable FunctionNeverReturns
            }
            // ReSharper restore FunctionNeverReturns
        }
    }



}