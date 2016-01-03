using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Controls.ATRViewerControl.Model;

namespace WhileTrue.Controls
{
    /// <summary/>
    public class AtrViewer : Control, INotifyPropertyChanged
    {
        public static DependencyProperty AtrProperty = DependencyProperty.Register("Atr", typeof (Atr), typeof (AtrViewer), new FrameworkPropertyMetadata(default(Atr),AtrViewer.AtrChanged));
        private AtrViewerModel atrModel;

        static AtrViewer()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(AtrViewer), new FrameworkPropertyMetadata(typeof(AtrViewer)));
        }

        private static void AtrChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if( e.NewValue != null )
            {
                ((AtrViewer) d).AtrModel = new AtrViewerModel((Atr)e.NewValue);
            }
            else
            {
                ((AtrViewer) d).AtrModel = null;
            }
        }

        public Atr Atr
        {
            get { return (Atr) this.GetValue(AtrViewer.AtrProperty); }
            set { this.SetValue(AtrViewer.AtrProperty, value); }
        }

        public AtrViewerModel AtrModel
        {
            get { return this.atrModel; }
            private set { this.SetAndInvoke(this.PropertyChanged, ref this.atrModel, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate{};
    }
}