using System.Collections;

namespace WhileTrue.Classes.SettingStorage
{
    /// <summary>
    /// Provides isolated storage of tag-value data
    /// </summary>
    public interface ITagValueSettingStore
    {
        /// <summary/>
        object this[string key] { get; set; }
        /// <summary/>
        bool ContainsKey(string key);
        /// <summary/>
        IDictionaryEnumerator GetEnumerator();
    }
}