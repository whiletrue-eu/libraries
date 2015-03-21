using WhileTrue.Classes.Components;

namespace AtrEditor.About
{
    [ComponentInterface]
    public interface IAboutWindowView
    {
        void ShowModal();
        AboutWindow Model { set; }
    }
}