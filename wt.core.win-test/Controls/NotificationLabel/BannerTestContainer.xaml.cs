using System.ComponentModel;
using System.Windows.Input;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
    /// <summary>
    ///     Interaction logic for DialogPanelTestContainer.xaml
    /// </summary>
    partial class BannerTestContainer
    {
        public BannerTestContainer()
        {
            InitializeComponent();
        }

        private void BannerTestContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }

    [TestFixture]
    public class BannerTest
    {
        [Test]
        [Ignore("Manual test")]
        public void Test()
        {
            new BannerTestContainer().ShowDialog();
        }
    }

    public class BannerTestValues : IDataErrorInfo
    {
        public string X { get; set; } = "Hello, Worl";
        public string Warning { get; set; }
        public string Info { get; set; }

        public string Error { get; set; }

        #region Implementation of IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "X":
                        return X == "Hello, World" ? "Error!" : "";
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