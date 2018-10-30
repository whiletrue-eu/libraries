#pragma warning disable 1591
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Wpf.Validation
{
    public class BackingData : ObservableObject
    {
        private readonly PropertyAdapter<string> fileAdapter;

        private string file;

        private int intValue;

        private string stringValue;

        public BackingData()
        {
            intValue = 42;
            stringValue = "42";

            fileAdapter = CreatePropertyAdapter(
                nameof(File),
                () => FileOrig,
                value => FileOrig = value
            );

            AddValidationForProperty(() => Int)
                .AddValidation(value => value > 0 && value < 100,
                    value => new ValidationMessage(ValidationSeverity.Warning, "{0} is not between 1 and 100", value))
                .AddValidation(value => value % 2 == 0,
                    value => new ValidationMessage(ValidationSeverity.Info, "{0} is not even", value));
            AddValidationForProperty(() => String)
                .AddValidation(value => value.Contains(Int.ToString()) || value == Int.ToString(),
                    _ => new ValidationMessage(ValidationSeverity.Error, "String must contain the number {0}", Int));
            AddValidationForProperties(() => String, () => Int)
                .AddValidation((sValue, iValue) => sValue != iValue.ToString(),
                    (_, __) => "Content of Int and String may not be equal");
            AddValidationForProperty(() => File)
                .AddValidation(value => string.IsNullOrEmpty(value) || System.IO.File.Exists(value),
                    value => "File does not exist")
                .AddValidation(value => string.IsNullOrEmpty(value) == false,
                    value => new ValidationMessage(ValidationSeverity.Info, "Please give filename"));
        }

        public int Int
        {
            get => intValue;
            set => SetAndInvoke(nameof(Int), ref intValue, value);
        }

        public string String
        {
            get => stringValue;
            set => SetAndInvoke(nameof(String), ref stringValue, value);
        }

        public string FileOrig
        {
            get => file;
            set => SetAndInvoke(nameof(File), ref file, value);
        }

        public string File
        {
            get => fileAdapter.GetValue();
            set => fileAdapter.SetValue(value);
        }
    }
}