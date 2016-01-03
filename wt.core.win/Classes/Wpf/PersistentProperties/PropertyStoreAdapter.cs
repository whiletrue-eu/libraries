using WhileTrue.Classes.Framework;
using WhileTrue.Classes.SettingStorage;

namespace WhileTrue.Classes.Wpf
{
    internal class PropertyStoreAdapter
    {
        private readonly ITagValueSettingStore storage;
        private static readonly ITagValueSettingStore defaultStore = IsolatedSettingStorage.GetTagValueStore("PersistentProperties");
        private static readonly ObjectCache<ITagValueSettingStore, PropertyStoreAdapter> instances;

        static PropertyStoreAdapter()
        {
            PropertyStoreAdapter.instances = new ObjectCache<ITagValueSettingStore, PropertyStoreAdapter>(key => new PropertyStoreAdapter(key));
        }

        public static PropertyStoreAdapter GetInstanceFor(ITagValueSettingStore tagValueSettingStore)
        {
            return PropertyStoreAdapter.instances.GetObject(tagValueSettingStore ?? PropertyStoreAdapter.defaultStore);
        }


        private readonly ObjectCache<string,object, PropertyAdapter> properties;

        private PropertyStoreAdapter(ITagValueSettingStore storage)
        {
            this.storage = storage;
            this.properties = new ObjectCache<string, object, PropertyAdapter>((name,defaultValue) => new PropertyAdapter(name, this.storage, defaultValue));
        }

        public PropertyAdapter GetProperty(string propertyName, object defaultValue)
        {
            return this.properties.GetObject(propertyName, defaultValue);
        }
    }
}