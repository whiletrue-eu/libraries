using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.DragNDrop.Facades.ImageLibraryModel;

namespace WhileTrue.DragNDrop.Modules.ImageLibraryViewer
{
    internal class GroupAdapter : ItemAdapterBase
    {
        private readonly PropertyAdapter<string> nameAdapter;
        private readonly EnumerablePropertyAdapter<object, ItemAdapterBase> itemsAdapter;

        internal GroupAdapter(IGroup @group)
        {
            this.nameAdapter = this.CreatePropertyAdapter(
                ()=>Name,
                ()=>group.Name,
                name=>group.Name=name
                );
            this.itemsAdapter = this.CreatePropertyAdapter<object,ItemAdapterBase, IEnumerable<ItemAdapterBase>>(
                () => Items,
                () => group.Groups.Union<object>(group.Images),
                item => ItemAdapterBase.GetInstance(item)
                );
        }

        protected IEnumerable<ItemAdapterBase> Items
        {
            get { return this.itemsAdapter.GetCollection(); }
        }

        protected string Name
        {
            get { return this.nameAdapter.GetValue(); }
            set { this.nameAdapter.SetValue(value); }
        }
    }
}