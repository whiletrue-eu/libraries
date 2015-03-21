using WhileTrue.Classes.Components;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.DragNDrop.Modules.ImageLibraryViewer
{
    [Component]
    public class ImageLibraryViewerModule : IModule
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<ImageLibraryViewerModel>();
            componentRepository.AddComponent<ImageLibraryViewerView>();
            componentRepository.AddComponent<ImageLibraryViewerPresenter>();
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }
    }
}
