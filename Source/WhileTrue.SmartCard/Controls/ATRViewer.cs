using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Controls.ATRView;

namespace WhileTrue.Controls
{
    /// <summary/>
    public class ATRViewer : Control, INotifyPropertyChanged
    {
        public static DependencyProperty AtrProperty = DependencyProperty.Register("Atr", typeof (Atr), typeof (ATRViewer), new FrameworkPropertyMetadata(default(Atr),atrChanged));
        private ATRViewerModel atrModel;

        static ATRViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ATRViewer), new FrameworkPropertyMetadata(typeof(ATRViewer)));
        }

        private static void atrChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if( e.NewValue != null )
            {
                ((ATRViewer) d).AtrModel = new ATRViewerModel((Atr)e.NewValue);
            }
            else
            {
                ((ATRViewer) d).AtrModel = null;
            }
        }

        public Atr Atr
        {
            get { return (Atr) GetValue(AtrProperty); }
            set { SetValue(AtrProperty, value); }
        }

        public ATRViewerModel AtrModel
        {
            get { return this.atrModel; }
            private set { this.SetAndInvoke(() => this.AtrModel, ref this.atrModel, value, null, this.PropertyChanged); }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate{};
    }
}