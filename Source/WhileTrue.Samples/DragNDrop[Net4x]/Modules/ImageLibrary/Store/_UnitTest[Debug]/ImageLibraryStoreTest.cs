// ReSharper disable InconsistentNaming
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using WhileTrue.Classes.SettingStorage;

namespace WhileTrue.DragNDrop.Modules.ImageLibrary.Store._UnitTest
{
    [TestFixture]
    public class ImageLibraryStoreTest
    {
        [Test]
        public void Values_shall_be_returned_in_getters()
        {
            TestTagValueStore BackendStore = new TestTagValueStore();

            ImageLibraryStore Store = ImageLibraryStore.CreateForUnitTesting(BackendStore);
            Store.SetGroups("Group1",new[]{"G1.G1", "G1.G2" });
            Store.SetImages("Group1",new[]{"G1.I1", "G1.I2" });

            string[] Groups = Store.GetGroups("Group1");
            string[] Images = Store.GetImages("Group1");

            Assert.AreEqual(2, Groups.Length);
            Assert.AreEqual("G1.G1", Groups[0]);
            Assert.AreEqual("G1.G2", Groups[1]);
            Assert.AreEqual(2, Images.Length);
            Assert.AreEqual("G1.I1", Images[0]);
            Assert.AreEqual("G1.I2", Images[1]);
        }
        [Test]
        public void Values_shall_be_stored_in_backend_store()
        {
            TestTagValueStore BackendStore = new TestTagValueStore();

            ImageLibraryStore Store = ImageLibraryStore.CreateForUnitTesting(BackendStore);
            Store.SetGroups("Group1",new[]{"G1.G1", "G1.G2" });
            Store.SetImages("Group1",new[]{"G1.I1", "G1.I2" });

            string[] Groups = (string[]) BackendStore["Group1.groups"];
            string[] Images = (string[]) BackendStore["Group1.images"];

            Assert.AreEqual(2, Groups.Length);
            Assert.AreEqual("G1.G1", Groups[0]);
            Assert.AreEqual("G1.G2", Groups[1]);
            Assert.AreEqual(2, Images.Length);
            Assert.AreEqual("G1.I1", Images[0]);
            Assert.AreEqual("G1.I2", Images[1]);
        }      
        [Test]
        public void Unknown_groups_shall_return_empty_lists()
        {
            TestTagValueStore BackendStore = new TestTagValueStore();

            ImageLibraryStore Store = ImageLibraryStore.CreateForUnitTesting(BackendStore);

            string[] Groups = Store.GetGroups("UnknownGroup");
            string[] Images = Store.GetImages("UnknownGroup");

            Assert.AreEqual(0, Groups.Length);
            Assert.AreEqual(0, Images.Length);
        }
        
        [Test]
        public void Null_shall_be_supported_as_valid_group_name()
        {
            TestTagValueStore BackendStore = new TestTagValueStore();

            ImageLibraryStore Store = ImageLibraryStore.CreateForUnitTesting(BackendStore);

            Store.SetGroups(null, new string[0]);
            Store.SetImages(null, new string[0]);

            string[] Groups = Store.GetGroups(null);
            string[] Images = Store.GetImages(null);

            Assert.AreEqual(0, Groups.Length);
            Assert.AreEqual(0, Images.Length);
        }
    }

    internal class TestTagValueStore : Dictionary<string,object>, ITagValueSettingStore
    {
        IDictionaryEnumerator ITagValueSettingStore.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
