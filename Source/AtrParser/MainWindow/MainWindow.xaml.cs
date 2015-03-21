using System;
using System.ComponentModel;
using System.Windows.Input;
using AtrEditor.About;
using AtrParser;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace AtrEditor.MainWindow
{
    ///<summary/>
    [Component]
    public partial class MainWindow : INotifyPropertyChanged, IMainWindow
    {
        private readonly IAboutWindow aboutWindow;
        private Atr atr;
        private string atrValue;
        private string error;

        public MainWindow() :this(null)
        {
        }

        public MainWindow(IAboutWindow aboutWindow)
        {
            this.aboutWindow = aboutWindow;
            InitializeComponent();
            this.DataContext = this;
            //this.AtrValue = "3B 1F 94 80 31 00 73 12 21 13 57 4A 33 05 30 32 34 00";
            this.AtrValue = "3B 1F 95 80 31 00 73 12 21 13 57 4A 33 0E 19 32 33 00";
        } 

        public Atr Atr
        {
            get { return this.atr; }
            set { this.SetAndInvoke(()=>this.Atr, ref this.atr, value, null, this.PropertyChanged); }
        }    
        
        public string AtrValue
        {
            get { return this.atrValue; }
            set
            {
                this.SetAndInvoke(()=>this.AtrValue, ref this.atrValue, value, null, this.PropertyChanged);
                try
                {
                    if (this.Atr != null)
                    {
                        this.Atr.PropertyChanged -= this.Atr_Changed;
                    }
                    byte[] AtrBytes = this.atrValue.ToByteArray();
                    try
                    {
                        this.Atr = new Atr(AtrBytes);
                        this.Atr.PropertyChanged += this.Atr_Changed;
                        this.atrValue = AtrBytes.ToHexString(" ");
                        this.Error = null;
                    }
                    catch (Exception Exception)
                    {
                        this.Error = Exception.Message;
                    }
                }
                catch (Exception Exception)
                {
                    this.Error = "Atr is not a valid hexadecimal value";
                }
            }
        }

        public string Error
        {
            get { return this.error; }
            set { this.SetAndInvoke(() => this.Error, ref this.error, value, null, this.PropertyChanged); }
        }

        public int DaysLeft { get; set; }

        void Atr_Changed(object sender, EventArgs e)
        {
            this.atrValue = this.atr.Bytes.ToHexString(" ");
            this.PropertyChanged(this, new PropertyChangedEventArgs("AtrValue"));
        }


        public event PropertyChangedEventHandler PropertyChanged=delegate {};

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.aboutWindow.ShowModal();
        }
    }
}
