using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls
{

    public class PopupNotificationMessageWindow : NotificationMessageWindow
    {
        /// <summary/>
        public static readonly DependencyProperty AutoFadeOutSecondsProperty;

        static PopupNotificationMessageWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupNotificationMessageWindow), new FrameworkPropertyMetadata(typeof(PopupNotificationMessageWindow)));

            AutoFadeOutSecondsProperty = DependencyProperty.Register(
                "AutoFadeOutSeconds",
                typeof(int),
                typeof(PopupNotificationMessageWindow),
                new FrameworkPropertyMetadata(0, AutoFadeOutSecondsChanged)
                );
        }

        private static void AutoFadeOutSecondsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((PopupNotificationMessageWindow)sender).AutoFadeOutSecondsChanged();
        }

        private readonly AutoFadeOutWatcherThread autoFadeOutWatcher;
        private readonly AutoResetEvent autoFadeOutSecondsEvent = new AutoResetEvent(false);
        private bool fadedIn;

        public PopupNotificationMessageWindow()
        {
            this.autoFadeOutWatcher = new AutoFadeOutWatcherThread(this);
            this.autoFadeOutWatcher.Start();
        }

        public int AutoFadeOutSeconds
        {
            get { return (int)this.GetValue(AutoFadeOutSecondsProperty); }
            set { this.SetValue(AutoFadeOutSecondsProperty, value); }
        }

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

        public override void FadeOut()
        {
            //fade out only if currently shpwn
            if (this.fadedIn)
            {
                this.fadedIn = false;
                base.FadeOut();
            }
        }

        public void NotifyAutoHide()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate { this.Close(); });
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
                : base("Notification Auto Fade Out Thread", true)
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