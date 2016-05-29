#pragma warning disable 1591
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Wpf.Validation
{
    public class BackingData : ObservableObject
    {
        public BackingData()
        {
            this.intValue = 42;
            this.stringValue = "42";

            this.fileAdapter = this.CreatePropertyAdapter(
                nameof(this.File),
                () => this.FileOrig,
                value => this.FileOrig = value
                );

            this.AddValidationForProperty(() => this.Int)
                .AddValidation(value => value > 0 && value < 100, value => new ValidationMessage(ValidationSeverity.Warning, "{0} is not between 1 and 100", value))
                .AddValidation(value => value % 2 == 0, value => new ValidationMessage(ValidationSeverity.Info, "{0} is not even", value));
            this.AddValidationForProperty(() => this.String)
                .AddValidation(value => value.Contains(this.Int.ToString()) || value == this.Int.ToString(), _ => new ValidationMessage(ValidationSeverity.Error, "String must contain the number {0}", this.Int));
            this.AddValidationForProperties(() => this.String, () => this.Int)
                .AddValidation((sValue, iValue) => sValue != iValue.ToString(), (_, __) => "Content of Int and String may not be equal");
            this.AddValidationForProperty(() => this.File)
                        .AddValidation(value => string.IsNullOrEmpty(value) || System.IO.File.Exists(value), value => "File does not exist")
                        .AddValidation(value => string.IsNullOrEmpty(value) == false, value => new ValidationMessage(ValidationSeverity.Info, "Please give filename"));
        }

        private int intValue;

        public int Int
        {
            get { return this.intValue; }
            set { this.SetAndInvoke(nameof(this.Int), ref this.intValue, value); }
        }

        private string stringValue;

        public string String
        {
            get { return this.stringValue; }
            set { this.SetAndInvoke(nameof(this.String), ref this.stringValue, value); }
        }      
        
        private string file;
        private readonly PropertyAdapter<string> fileAdapter;

        public string FileOrig
        {
            get { return this.file; }
            set { this.SetAndInvoke(nameof(this.File), ref this.file, value); }
        }
        public string File
        {
            get { return this.fileAdapter.GetValue(); }
            set { this.fileAdapter.SetValue(value); }
        }
    }
}