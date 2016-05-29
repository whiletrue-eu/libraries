using WhileTrue.Classes.Components;
using WhileTrue.Common.Components.CommonDialogs;
using WhileTrue.SmartCard;

namespace WhileTrue
{
    /// <summary/>
    public partial class App
    {
        protected override void AddComponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<WpfCommonDialogProvider>();
            componentRepository.AddComponent<SmartCardSample>();
        }
    }
}
