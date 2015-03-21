using System;
using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.DragNDrop.Facades.ImageLibraryModel;

namespace WhileTrue.DragNDrop.Modules.ImageLibrary.Model
{
    internal class GroupCollection : ObservableReadOnlyCollection<IGroup>,IGroupCollection
    {
        private readonly Group owner;
        private readonly ImageLibraryModel library;

        public GroupCollection(Group owner, ImageLibraryModel library, IEnumerable<Group> groups)
        {
            this.owner = owner;
            this.library = library.DbC_AssureNotNull();
            groups.ForEach(group => this.InnerList.Add(group));
        }

        public IGroup Add(string name)
        {
            Group Group = new Group(this.owner, this.library, name);
            this.owner.NotifyGroupAdding(Group);
            this.InnerList.Add(Group);
            this.owner.NotifyGroupAdded(Group);
            return Group;
        }

        public void Remove(IGroup group)
        {
        }

        public string[] GetGroupNames()
        {
            return (from Group in this select Group.Name).ToArray();
        }
    }
}