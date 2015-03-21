using WhileTrue.Classes.Components;
using WhileTrue.Classes.SettingStorage;
using WhileTrue.DragNDrop.Facades.ImageLibraryStore;

namespace WhileTrue.DragNDrop.Modules.ImageLibrary.Store
{
    [Component]
    public class ImageLibraryStore : IImageLibraryStore
    {
        private readonly ITagValueSettingStore store = IsolatedSettingStorage.GetTagValueStore("ImageLibrary");

        /// <summary/>
        public ImageLibraryStore()
        {
        }

#if DEBUG
        private ImageLibraryStore( ITagValueSettingStore store )
        {
            this.store = store;
        }

        internal static ImageLibraryStore CreateForUnitTesting( ITagValueSettingStore store )
        {
            return new ImageLibraryStore(store);
        }
#endif

        public string[] GetGroups(string group)
        {
            string Key = string.Format("{0}.groups", group);
            if( this.store.ContainsKey(Key))
            {
                return (string[]) this.store[Key];
            }
            else
            {
                return new string[0];
            }
        }

        public string[] GetImages(string group)
        {
            string Key = string.Format("{0}.images", group);
            if (this.store.ContainsKey(Key))
            {
                return (string[])this.store[Key];
            }
            else
            {
                return new string[0];
            }
        }

        public void SetGroups(string group, string[] groups)
        {
            string Key = string.Format("{0}.groups", group);
            this.store[Key] = groups;
        }

        public void SetImages(string group, string[] images)
        {
            string Key = string.Format("{0}.images", group);
            this.store[Key] = images;
        }
    }
}