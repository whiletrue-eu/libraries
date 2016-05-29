using System.Windows.Forms;
using Mz.Classes.Components;

namespace Mz.Facades.MenuFactory
{
    [ComponentInterface]
    public interface IMenuUIProvider
    {
        void Attach(Form form);
    }
}