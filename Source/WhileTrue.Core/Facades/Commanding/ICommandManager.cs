using System.Windows.Input;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.Commanding
{
    [ComponentInterface]
    public interface ICommandManager
    {
        ICommand this[string commandID] { get; }
        ICommand RegisterCommand(string id, ICommand command);
        ICommand RegisterCommand(ICommandIdentification command);
    }
}