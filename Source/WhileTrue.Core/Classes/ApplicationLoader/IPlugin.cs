using Mz.Classes.Components;

namespace Mz.Facades.PluginFramework
{
    [ComponentInterface]
    public interface IPlugin
    {
        void Initialize();
    }
}