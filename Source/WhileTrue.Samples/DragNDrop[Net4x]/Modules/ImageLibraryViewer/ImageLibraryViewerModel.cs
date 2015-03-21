using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.DragNDrop;
using WhileTrue.Classes.Framework;
using WhileTrue.DragNDrop.Facades.ImageLibraryModel;

namespace WhileTrue.DragNDrop.Modules.ImageLibraryViewer
{
    [Component]
    internal class ImageLibraryViewerModel : ObservableObject, IImageLibraryViewerModel
    {
        private readonly IImageLibraryModel model;
        private readonly EnumerablePropertyAdapter<object, ItemAdapterBase> itemsAdapter;

        public ImageLibraryViewerModel(IImageLibraryModel model)
        {
            this.model = model;

            this.itemsAdapter = this.CreatePropertyAdapter<object, ItemAdapterBase, IEnumerable<ItemAdapterBase>>(
                () => Items,
                () => model.Root.Groups.Union<object>(model.Root.Images), 
                item=> ItemAdapterBase.GetInstance(item)
                );

            this.DropTarget = DragDropTarget.GetFactory()
                .AddTypeHandler<FileDropDataType>(
                    DragDropEffects.Copy,
                    DragDropEffect.Copy,
                    ((files, effect, additionalDropInfo) => this.AddFiles(files.Files))
                )
                .Create();
        }

        private void AddFiles(IEnumerable<FileInfo> files)
        {
            foreach (FileInfo FileInfo in files)
            {
                this.model.Root.Images.Add(FileInfo.FullName);
            }
        }

        public IEnumerable<ItemAdapterBase> Items
        {
            get
            {
                return this.itemsAdapter.GetCollection();
            }
        }

        public IDragDropTarget DropTarget
        {
            get; private set;
        }
    }
}
