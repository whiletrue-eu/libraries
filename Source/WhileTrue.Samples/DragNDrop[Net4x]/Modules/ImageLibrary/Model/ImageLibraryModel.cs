using WhileTrue.Classes.Components;
using WhileTrue.DragNDrop.Facades.ImageLibraryModel;
using WhileTrue.DragNDrop.Facades.ImageLibraryStore;

namespace WhileTrue.DragNDrop.Modules.ImageLibrary.Model
{
    [Component]
    internal class ImageLibraryModel : IImageLibraryModel
    {
        private readonly Group root;
        private readonly IImageLibraryStore store;

        /// <summary/>
        public ImageLibraryModel(IImageLibraryStore store)
        {
            this.store = store;
            this.root = new Group(null, this, null);
        }

        public IGroup Root
        {
            get { return this.root; }
        }

        internal IImageLibraryStore Store
        {
            get {
                return store;
            }
        }
    }
}