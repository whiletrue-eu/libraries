using WhileTrue.Classes.Components;
using WhileTrue.Common.Components.CommonDialogs;
#if NET40
using WhileTrue.DragNDrop;
#endif
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
#if NET40
            //componentRepository.AddComponent<DragNDropSample>();
#endif
        }
    }
}
