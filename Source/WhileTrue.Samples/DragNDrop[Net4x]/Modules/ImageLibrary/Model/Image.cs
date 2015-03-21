using System.IO;
using WhileTrue.DragNDrop.Facades.ImageLibraryModel;

namespace WhileTrue.DragNDrop.Modules.ImageLibrary.Model
{
    internal class Image : IImage
    {
        private readonly Group owner;
        private readonly ImageLibraryModel library;
        private readonly string path;

        public Image(Group owner, ImageLibraryModel library, string path)
        {
            this.owner = owner;
            this.library = library;
            this.path = path;
        }

        public string Path
        {
            get { return this.path; }
        }

        public string Name
        {
            get
            {
                return System.IO.Path.GetFileName(this.path);
            }
        }
    }
}