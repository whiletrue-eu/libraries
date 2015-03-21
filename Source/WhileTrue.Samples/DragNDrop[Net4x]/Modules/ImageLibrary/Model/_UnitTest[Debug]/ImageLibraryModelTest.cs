// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WhileTrue.DragNDrop.Facades.ImageLibraryModel;
using WhileTrue.DragNDrop.Facades.ImageLibraryStore;

namespace WhileTrue.DragNDrop.Modules.ImageLibrary.Model._UnitTest
{
    [TestFixture]
    public class ImageLibraryModelTest
    {
        [Test]
        public void Root_group_shall_exist_in_empty_library()
        {
            ImageLibraryTestStore TestStore = new ImageLibraryTestStore();
            ImageLibraryModel Model = new ImageLibraryModel(TestStore);

            Assert.IsNotNull(Model.Root);
            Assert.IsNull(Model.Root.Name);
        }

        [Test]
        public void Group_add_shall_be_reflected_in_Model_and_store()
        {
            ImageLibraryTestStore TestStore = new ImageLibraryTestStore();
            ImageLibraryModel Model = new ImageLibraryModel(TestStore);

            IGroup Group = Model.Root.Groups.Add("G1");
            
            Assert.AreEqual("G1",Group.Name);
            Assert.AreEqual(1,Model.Root.Groups.Count());
            Assert.AreEqual(Group,Model.Root.Groups.ToArray()[0]);

            TestStore.AssertData(new Dictionary<string, Tuple<List<string>, List<string>>>
                                     {
                                         {
                                             ImageLibraryTestStore.NullKey, new Tuple<List<string>, List<string>>(
                                             new List<string>{"G1"},
                                             new List<string>())
                                             },
                                         {
                                             "G1", new Tuple<List<string>, List<string>>(
                                             new List<string>(),
                                             new List<string>())
                                             }
                                     });
        }
        [Test]
        public void Group_add_shall_be_rejected_if_same_name_is_used()
        {
            ImageLibraryTestStore TestStore = new ImageLibraryTestStore();
            ImageLibraryModel Model = new ImageLibraryModel(TestStore);

            Model.Root.Groups.Add("G1");
            Assert.Throws<InvalidOperationException>(()=>Model.Root.Groups.Add("G1"));
        }
        [Test]
        public void Image_add_shall_be_reflected_in_Model_and_store()
        {
            ImageLibraryTestStore TestStore = new ImageLibraryTestStore();
            ImageLibraryModel Model = new ImageLibraryModel(TestStore);

            IImage Image = Model.Root.Images.Add("I1");

            Assert.AreEqual("I1", Image.Path);
            Assert.AreEqual(1, Model.Root.Images.Count());
            Assert.AreEqual(Image, Model.Root.Images.ToArray()[0]);

            TestStore.AssertData(new Dictionary<string, Tuple<List<string>, List<string>>>
                                     {
                                         {
                                             ImageLibraryTestStore.NullKey, new Tuple<List<string>, List<string>>(
                                             new List<string>(),
                                             new List<string>{"I1"})
                                             }
                                     });
        }
        [Test]
        public void Image_add_shall_not_be_rejected_if_same_path_is_used()
        {
            ImageLibraryTestStore TestStore = new ImageLibraryTestStore();
            ImageLibraryModel Model = new ImageLibraryModel(TestStore);

            IImage Image1 = Model.Root.Images.Add("I1");
            IImage Image2 = Model.Root.Images.Add("I1");
            Assert.AreNotEqual(Image1,Image2);
            Assert.AreEqual(Image1.Path,Image2.Path);
        }
    }

    internal class ImageLibraryTestStore : IImageLibraryStore
    {
        private readonly Dictionary<string,Tuple<List<string>,List<string>>> data;
        public const string NullKey = "<null>";

        public ImageLibraryTestStore(Dictionary<string,Tuple<List<string>,List<string>>> data)
        {
            this.data = data;
        }

        public ImageLibraryTestStore()
        {
            this.data = new Dictionary<string, Tuple<List<string>, List<string>>>();
        }

        public string[] GetGroups(string group)
        {
            if (data.ContainsKey(group ?? NullKey) == false)
            {
                data.Add(group ?? NullKey,new Tuple<List<string>, List<string>>(new List<string>(),new List<string>()));
            }
            return data[group ?? NullKey].Item1.ToArray();
        }

        public string[] GetImages(string group)
        {
            if (data.ContainsKey(group ?? NullKey) == false)
            {
                data.Add(group ?? NullKey, new Tuple<List<string>, List<string>>(new List<string>(), new List<string>()));
            }
            return data[group ?? NullKey].Item2.ToArray();
        }

        public void SetGroups(string group, string[] groups)
        {
            if (data.ContainsKey(group ?? NullKey) == false)
            {
                data.Add(group ?? NullKey, new Tuple<List<string>, List<string>>(new List<string>(), new List<string>()));
            }
            data[group ?? NullKey].Item1.Clear();
            data[group ?? NullKey].Item1.AddRange(groups);
        }

        public void SetImages(string group, string[] images)
        {
            if (data.ContainsKey(group ?? NullKey) == false)
            {
                data.Add(group ?? NullKey, new Tuple<List<string>, List<string>>(new List<string>(), new List<string>()));
            }
            data[group ?? NullKey].Item2.Clear();
            data[group ?? NullKey].Item2.AddRange(images);
        }

        public void AssertData(Dictionary<string,Tuple<List<string>,List<string>>> otherData)
        {
            Assert.AreEqual(otherData.Count, this.data.Count);
            foreach (KeyValuePair<string, Tuple<List<string>, List<string>>> OtherDataPair in otherData)
            {
                Tuple<List<string>, List<string>> OtherData = OtherDataPair.Value;
                Assert.IsTrue(this.data.ContainsKey(OtherDataPair.Key));
                Tuple<List<string>, List<string>> Data = this.data[OtherDataPair.Key];

                Assert.AreEqual(OtherData.Item1.Count, Data.Item1.Count);
                for (int Index = 0; Index < OtherData.Item1.Count; Index++)
                {
                    Assert.AreEqual(OtherData.Item1[Index], Data.Item1[Index]);
                }

                Assert.AreEqual(OtherData.Item2.Count, Data.Item2.Count);
                for (int Index = 0; Index < OtherData.Item2.Count; Index++)
                {
                    Assert.AreEqual(OtherData.Item2[Index], Data.Item2[Index]);
                }
            }
        }
    }
}
