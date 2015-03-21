using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls._Unittest
{
    /// <summary>
    /// Interaction logic for DialogPanelTestContainer.xaml
    /// </summary>
    partial class BannerTestContainer 
    {
        public BannerTestContainer()
        {
            InitializeComponent();
        }

        private void BannerTestContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }

    [TestFixture]
    public class BannerTest
    {
        [Test,Ignore("Manual test")]
        public void Test()
        {
            new BannerTestContainer().ShowDialog();
        }
    }

    public class BannerTestValues : IDataErrorInfo
    {
        private string x = "Hello, Worl";

        public string X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        public string Error { get; set; }
        public string Warning { get; set; }
        public string Info { get; set; }

        #region Implementation of IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch(columnName)
                {
                    case "X":
                        return this.x=="Hello, World"?"Error!":"";
                    case "Error":
                        return new ValidationMessage(ValidationSeverity.Error, "Error");
                    case "Warning":
                        return new ValidationMessage(ValidationSeverity.Warning, "Warning");
                    case "Info":
                        return new ValidationMessage(ValidationSeverity.Info, "Info");
                }
                return "";
            }
        }

        #endregion
    }

}
