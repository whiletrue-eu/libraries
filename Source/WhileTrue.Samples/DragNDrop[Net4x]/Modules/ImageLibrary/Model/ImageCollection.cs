using System;
using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.DragNDrop.Facades.ImageLibraryModel;

namespace WhileTrue.DragNDrop.Modules.ImageLibrary.Model
{
    internal class ImageCollection : ObservableReadOnlyCollection<IImage>, IImageCollection
    {
        private readonly Group owner;
        private readonly ImageLibraryModel library;

        public ImageCollection(Group owner, ImageLibraryModel library, IEnumerable<Image> images)
        {
            this.owner = owner;
            this.library = library;
            images.ForEach(image=>this.InnerList.Add(image));
        }

        public IImage Add(string path)
        {
            Image Image = new Image(this.owner, this.library, path);
            this.owner.NotifyImageAdding(Image);
            this.InnerList.Add(Image);
            this.owner.NotifyImageAdded(Image);
            return Image;
        }

        public void Remove(IImage image)
        {
        }

        public string[] GetImagePaths()
        {
            return (from Image in this select Image.Path).ToArray();

        }
    }
}