using System.Windows.Input;
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.Commanding
{
    [ComponentInterface]
    public interface ICommandWrapper
    {
        ICommand Wrap(ICommand command);
    }
}