using WhileTrue.Classes.Components;

namespace AtrParser
{
    [ComponentInterface]
    public interface IMainWindow
    {
        bool? ShowDialog();
        int DaysLeft { get; set; }
    }
}