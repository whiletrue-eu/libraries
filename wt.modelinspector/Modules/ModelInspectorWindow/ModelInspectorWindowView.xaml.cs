using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Wpf;
using WhileTrue.Controls;
using Window = System.Windows.Window;

namespace WhileTrue.Modules.ModelInspectorWindow
{
    /// <summary>
    /// Interaction logic for ModelInspectorWindowView.xaml
    /// </summary>

    partial class ModelInspectorWindowView : IDisposable
    {
        private readonly NotifyIcon notifyIcon;

        public ModelInspectorWindowView()
        {
            this.InitializeComponent();

            this.notifyIcon = new ModelInspectorWindowNotifyIcon();
            this.notifyIcon.MouseDoubleClick += this.notifyIcon_MouseDoubleClick;

            this.IsVisibleChanged += this.ModelInspectorWindowView_IsVisibleChanged;
        }

        void ModelInspectorWindowView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
            {
                this.notifyIcon.Hide();
            }
            else
            {
                this.notifyIcon.Show();
            }
        }

        void notifyIcon_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Show();
        }

        public IModelInspectorWindowModel Model
        {
            set { this.DataContext = value; }
        }

        public void Dispose()
        {
            this.notifyIcon.Hide();
        }
    }

    [Component]
    internal class ModelInspectorWindowViewProvider : IModelInspectorWindowView, IDisposable
    {
        private ModelInspectorWindowView view;
        private readonly ManualResetEvent viewCreated = new ManualResetEvent(false);
        private readonly Dispatcher viewDispatcher;
        private readonly Dispatcher mainDispatcher;

        public ModelInspectorWindowViewProvider()
        {
            this.mainDispatcher = Dispatcher.CurrentDispatcher;
            
            //Create Window in another thread
            Thread ViewThread = new Thread((ThreadStart)
                       delegate
                       {
                           this.view = new ModelInspectorWindowView();
                           this.view.Dispatcher.BeginInvoke((Action)delegate { this.viewCreated.Set(); });
                           this.mainDispatcher.ShutdownStarted += delegate { this.view.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal); };
                           Dispatcher.Run();
                       });
            ViewThread.SetApartmentState(ApartmentState.STA);
            ViewThread.IsBackground = true;
            ViewThread.Name = "ModelInspectorView";
            ViewThread.Start();

            this.viewCreated.WaitOne();

            this.viewDispatcher = this.view.Dispatcher;
        }

        public IModelInspectorWindowModel Model
        {
            set
            {
                this.viewDispatcher.BeginInvoke((Action)delegate { this.view.Model = value; });
            }
        }

        public void Show()
        {
            this.viewDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)this.InvokedShowDialog);
        }

        private void InvokedShowDialog()
        {
            Window MainWindow=null;
            this.mainDispatcher.Invoke((Action) delegate { MainWindow = Application.Current.MainWindow; });
            
            if (MainWindow == null)
            {
                //Make sure that we show the dialog only after the main window is created
                //This avoids that this window itsself is becoming the main window
                this.viewDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)this.InvokedShowDialog);
            }
            else
            {
                Window ActiveWindow = WpfUtils.FindActiveWindow();
                
                this.view.ShowActivated = false;
                this.view.Show();

                if (ActiveWindow != null)
                {
                    ActiveWindow.Invoke(window=>window.Activate());
                }
            }
        }

        public void Dispose()
        {
            this.viewDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)this.view.Dispose);
        }
    }
}
