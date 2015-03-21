using WhileTrue.Classes.Components;
using WhileTrue.DragNDrop.Modules.ImageLibrary.Model;
using WhileTrue.DragNDrop.Modules.ImageLibrary.Store;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.DragNDrop.Modules.ImageLibrary
{
    [Component]
    public class ImageLibraryModule : IModule
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<ImageLibraryModel>();
            componentRepository.AddComponent<ImageLibraryStore>();
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }
    }
}
