using System.Collections;
using System.IO;
using System.Xaml;

namespace WhileTrue.Classes.SettingStorage
{
    internal class TagValueSettingStore : ITagValueSettingStore
    {
        private readonly string file;
        private readonly string path;
        private readonly Hashtable tagValues;

        internal TagValueSettingStore(string path, string name)
        {
            try
            {
                this.path = path;
                file = name + ".settings";
                Directory.CreateDirectory(path);
                using (var FileStream = new FileStream(Path.Combine(path, file), FileMode.OpenOrCreate, FileAccess.Read,
                    FileShare.Read))
                {
                    if (FileStream.Length > 0)
                        try
                        {
                            tagValues = (Hashtable) XamlServices.Load(FileStream);
                        }
                        catch
                        {
                            tagValues = new Hashtable();
                        }
                    else
                        tagValues = new Hashtable();
                }
            }
            catch
            {
                tagValues = new Hashtable();
            }
        }

        public object this[string key]
        {
            get => tagValues[key];
            set
            {
                tagValues[key] = value;
                Save();
            }
        }

        public bool ContainsKey(string key)
        {
            return tagValues.ContainsKey(key);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return tagValues.GetEnumerator();
        }

        private void Save()
        {
            using (var FileStream = new FileStream(Path.Combine(path, file), FileMode.Create, FileAccess.Write,
                FileShare.Write))
            {
                XamlServices.Save(FileStream, tagValues);
            }
        }
    }
}