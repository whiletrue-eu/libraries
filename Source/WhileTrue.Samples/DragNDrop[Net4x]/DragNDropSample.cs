using WhileTrue.Classes.Components;
using WhileTrue.DragNDrop.Facades.ImageLibraryViewer;
using WhileTrue.DragNDrop.Modules.ImageLibrary;
using WhileTrue.DragNDrop.Modules.ImageLibraryViewer;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.DragNDrop
{
    [Component]
    public class DragNDropSample : IApplicationMain
    {
        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<ImageLibraryModule>();
            componentRepository.AddComponent<ImageLibraryViewerModule>();
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }

        public int Run(ComponentContainer componentContainer)
        {
            IImageLibraryViewer ImageLibrary = componentContainer.ResolveInstance<IImageLibraryViewer>();
            ImageLibrary.Open();
            return 0;
        }
    }
}
