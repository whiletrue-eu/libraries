using WhileTrue.Classes.Framework;
using WhileTrue.Classes.SettingStorage;

namespace WhileTrue.Classes.Wpf
{
    internal class PropertyAdapter : ObservableObject
    {
        private readonly string key;
        private readonly ITagValueSettingStore propertyStore;
        private object value;

        public PropertyAdapter(string key, ITagValueSettingStore propertyStore, object defaultValue)
        {
            this.key = key;
            this.propertyStore = propertyStore;
            value = this.propertyStore.ContainsKey(key) ? this.propertyStore[key] : defaultValue;
        }

        public object Value
        {
            get => value;
            set
            {
                propertyStore[key] = value;
                SetAndInvoke(nameof(Value), ref this.value, value);
            }
        }
    }
}