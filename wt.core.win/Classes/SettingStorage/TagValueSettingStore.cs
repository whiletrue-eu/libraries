using System.Collections;
using System.IO;
using System.Xaml;

namespace WhileTrue.Classes.SettingStorage
{
    internal class TagValueSettingStore : ITagValueSettingStore
    {
        private readonly string path;
        private readonly Hashtable tagValues;
        private readonly string file;

        internal TagValueSettingStore(string path, string name)
        {
            try
            {
                this.path = path;
                this.file = name + ".settings";
                Directory.CreateDirectory(path);
                using (FileStream FileStream = new FileStream(Path.Combine(path, this.file), FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                {
                    if (FileStream.Length > 0)
                    {
                        try
                        {
                            this.tagValues = (Hashtable)XamlServices.Load(FileStream);
                        }
                        catch
                        {
                            this.tagValues = new Hashtable();
                        }
                    }
                    else
                    {
                        this.tagValues = new Hashtable();
                    }
                }
            }
            catch
            {
                this.tagValues = new Hashtable();
            }
        }

        public object this[string key]
        {
            get { return this.tagValues[key]; }
            set
            {
                this.tagValues[key] = value;
                this.Save();
            }
        }

        private void Save()
        {
            using (FileStream FileStream = new FileStream(Path.Combine(this.path, this.file), FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                XamlServices.Save(FileStream, this.tagValues);
            }
        }

        public bool ContainsKey(string key)
        {
            return this.tagValues.ContainsKey(key);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return this.tagValues.GetEnumerator();
        }
    }
}