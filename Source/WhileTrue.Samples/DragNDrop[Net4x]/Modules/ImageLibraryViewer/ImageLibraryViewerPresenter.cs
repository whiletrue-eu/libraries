using WhileTrue.Classes.Components;
using WhileTrue.DragNDrop.Facades.ImageLibraryViewer;

namespace WhileTrue.DragNDrop.Modules.ImageLibraryViewer
{
    [Component]
    internal class ImageLibraryViewerPresenter :IImageLibraryViewer
    {
        private readonly IImageLibraryViewerView view;

        /// <summary/>
        public ImageLibraryViewerPresenter(IImageLibraryViewerModel model, IImageLibraryViewerView view)
        {
            this.view = view;
            this.view.Model = model;
        }

        public void Open()
        {
            this.view.Open();
        }
    }
}