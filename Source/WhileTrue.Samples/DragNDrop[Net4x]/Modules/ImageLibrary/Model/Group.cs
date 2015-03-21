using System;
using System.Linq;
using WhileTrue.Classes.Utilities;
using WhileTrue.DragNDrop.Facades.ImageLibraryModel;

namespace WhileTrue.DragNDrop.Modules.ImageLibrary.Model
{
    internal class Group : IGroup
    {
        private readonly Group owner;
        private readonly ImageLibraryModel library;
        private string name;
        private readonly GroupCollection groups;
        private readonly ImageCollection images;

        public Group( Group owner, ImageLibraryModel library, string name)
        {
            this.owner = owner;
            this.library = library.DbC_AssureNotNull();
            this.name = name;

            this.images = new ImageCollection(this, this.library, from ImageName in library.Store.GetImages(name) select new Image(this, library, ImageName));
            this.groups = new GroupCollection(this, this.library, from GroupName in library.Store.GetGroups(name) select new Group(this, library, GroupName));
        }

        public IGroupCollection Groups
        {
            get { return this.groups; }
        }

        public IImageCollection Images
        {
            get { return this.images; }
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        internal void NotifyGroupAdding(Group groupToAdd)
        {
            if (this.Groups.Any(group => group.Name == groupToAdd.Name))
            {
                throw new InvalidOperationException("Group with same name already exists");
            }
        }

        internal void NotifyGroupAdded(Group groupAdded)
        {
            this.library.Store.SetGroups(this.name, this.groups.GetGroupNames());
        }

        public void NotifyImageAdding(Image image)
        {
            
        }

        public void NotifyImageAdded(Image image)
        {
            this.library.Store.SetImages(this.name, this.images.GetImagePaths());
        }
    }
}