using WhileTrue.Classes.Framework;
using WhileTrue.Classes.SettingStorage;

namespace WhileTrue.Classes.Wpf
{
    internal class PropertyStoreAdapter
    {
        private static readonly ITagValueSettingStore defaultStore =
            IsolatedSettingStorage.GetTagValueStore("PersistentProperties");

        private static readonly ObjectCache<ITagValueSettingStore, PropertyStoreAdapter> instances;


        private readonly ObjectCache<string, object, PropertyAdapter> properties;
        private readonly ITagValueSettingStore storage;

        static PropertyStoreAdapter()
        {
            instances = new ObjectCache<ITagValueSettingStore, PropertyStoreAdapter>(key =>
                new PropertyStoreAdapter(key));
        }

        private PropertyStoreAdapter(ITagValueSettingStore storage)
        {
            this.storage = storage;
            properties = new ObjectCache<string, object, PropertyAdapter>((name, defaultValue) =>
                new PropertyAdapter(name, this.storage, defaultValue));
        }

        public static PropertyStoreAdapter GetInstanceFor(ITagValueSettingStore tagValueSettingStore)
        {
            return instances.GetObject(tagValueSettingStore ?? defaultStore);
        }

        public PropertyAdapter GetProperty(string propertyName, object defaultValue)
        {
            return properties.GetObject(propertyName, defaultValue);
        }
    }
}