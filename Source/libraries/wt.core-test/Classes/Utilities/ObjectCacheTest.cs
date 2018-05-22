// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming
#pragma warning disable 1591
using System;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Utilities
{
    [TestFixture]
    public class ObjectCacheTest
    {
        [Test]
        public void cache_shall_return_the_same_object_for_the_same_key()
        {
            ObjectCache<int, object> Cache = new ObjectCache<int, object>(p1 => new object());

            object Object1 = Cache.GetObject(1);
            object Object2 = Cache.GetObject(2);
            object Other2 = Cache.GetObject(2);

            Assert.AreEqual(Object2,Other2);
            Assert.AreNotEqual(Object2,Object1);
        }

        [Test]
        public void on_null_values_null_shall_be_returned()
        {
            ObjectCache<object, object> Cache = new ObjectCache<object, object>(p1 => new object());

            object Object = Cache.GetObject(null);

            Assert.IsNull(Object);
        }
        
        [Test]
        public void objects_that_do_not_have_external_references_shall_be_removed_from_the_cache()
        {
            int CreateCalls = 0;
            ObjectCache<int,object> Cache = new ObjectCache<int, object>(p1=>
                                                                             {
                                                                                 CreateCalls++;
                                                                                 return new object();
                                                                             });

            object Object = Cache.GetObject(1);
            Assert.AreEqual(1, CreateCalls);

            WeakReference ObjectRef = new WeakReference(Object);
            Object = null;
            GC.Collect();
            GC.WaitForFullGCComplete(10000);
            GC.Collect(10, GCCollectionMode.Forced);

            Assert.IsFalse(ObjectRef.IsAlive);

            //Object is recreated and chache is cleaned up
            Cache.GetObject(1);
            Assert.AreEqual(2,CreateCalls);
        }

        #region one parameter
        [Test]
        public void cache_shall_return_the_same_object_for_the_same_key_for_value_keys_and_one_parameter()
        {
            ObjectCache<int, object> Cache = new ObjectCache<int, object>(p1 => new object());

            object Object1 = Cache.GetObject(1);
            object Object2 = Cache.GetObject(2);
            object Other2 = Cache.GetObject(2);

            Assert.AreEqual(Object2, Other2);
            Assert.AreNotEqual(Object2, Object1);
        }

        [Test]
        public void cache_shall_return_the_same_object_for_the_same_key_for_object_keys_and_one_parameter()
        {
            ObjectCache<object, object> Cache = new ObjectCache<object, object>(p1 => new object());

            object Key1 = new object();
            object Key2 = new object();

            object Object1 = Cache.GetObject(Key1);
            object Object2 = Cache.GetObject(Key2);
            object Other2 = Cache.GetObject(Key2);

            Assert.AreEqual(Object2, Other2);
            Assert.AreNotEqual(Object2, Object1);
        }

        #endregion
        #region two parameter
        [Test]
        public void cache_shall_return_the_same_object_for_the_same_key_for_value_keys_and_two_parameter()
        {
            ObjectCache<int, int, object> Cache = new ObjectCache<int, int, object>((p1,p2) => new object());

            object Object1 = Cache.GetObject(1,1);
            object Object2 = Cache.GetObject(2,2);
            object Other2 = Cache.GetObject(2,2);

            Assert.AreEqual(Object2, Other2);
            Assert.AreNotEqual(Object2, Object1);
        }

        [Test]
        public void cache_shall_return_the_same_object_for_the_same_key_for_object_keys_and_two_parameter()
        {
            ObjectCache<object, object, object> Cache = new ObjectCache<object, object, object>((p1, p2) => new object());

            object Key1a = new object();
            object Key1b = new object();
            object Key2a = new object();
            object Key2b = new object();

            object Object1 = Cache.GetObject(Key1a, Key1b);
            object Object2 = Cache.GetObject(Key2a, Key2b);
            object Other2 = Cache.GetObject(Key2a, Key2b);

            Assert.AreEqual(Object2, Other2);
            Assert.AreNotEqual(Object2, Object1);
        }

        #endregion
        #region three parameter
        [Test]
        public void cache_shall_return_the_same_object_for_the_same_key_for_value_keys_and_three_parameter()
        {
            ObjectCache<int, int, int, object> Cache = new ObjectCache<int, int, int, object>((p1, p2, p3) => new object());

            object Object1 = Cache.GetObject(1, 1, 1);
            object Object2 = Cache.GetObject(2, 2, 2);
            object Other2 = Cache.GetObject(2, 2, 2);

            Assert.AreEqual(Object2, Other2);
            Assert.AreNotEqual(Object2, Object1);
        }

        [Test]
        public void cache_shall_return_the_same_object_for_the_same_key_for_object_keys_and_three_parameter()
        {
            ObjectCache<object, object, object, object> Cache = new ObjectCache<object, object, object, object>((p1, p2, p3) => new object());

            object Key1a = new object();
            object Key1b = new object();
            object Key1c = new object();
            object Key2a = new object();
            object Key2b = new object();
            object Key2c = new object();

            object Object1 = Cache.GetObject(Key1a, Key1b, Key1c);
            object Object2 = Cache.GetObject(Key2a, Key2b, Key2c);
            object Other2 = Cache.GetObject(Key2a, Key2b, Key2c);

            Assert.AreEqual(Object2, Other2);
            Assert.AreNotEqual(Object2, Object1);
        }


        #endregion
        #region four parameter
        [Test]
        public void cache_shall_return_the_same_object_for_the_same_key_for_value_keys_and_four_parameter()
        {
            ObjectCache<int, int, int, int, object> Cache = new ObjectCache<int, int, int, int, object>((p1, p2, p3, p4) => new object());

            object Object1 = Cache.GetObject(1, 1, 1, 1);
            object Object2 = Cache.GetObject(2, 2, 2, 2);
            object Other2 = Cache.GetObject(2, 2, 2, 2);

            Assert.AreEqual(Object2, Other2);
            Assert.AreNotEqual(Object2, Object1);
        }

        [Test]
        public void cache_shall_return_the_same_object_for_the_same_key_for_object_keys_and_four_parameter()
        {
            ObjectCache<object, object, object,  object, object> Cache = new ObjectCache<object, object, object, object, object>((p1, p2, p3, p4) => new object());

            object Key1a = new object();
            object Key1b = new object();
            object Key1c = new object();
            object Key1d = new object();
            object Key2a = new object();
            object Key2b = new object();
            object Key2c = new object();
            object Key2d = new object();

            object Object1 = Cache.GetObject(Key1a, Key1b, Key1c, Key1d);
            object Object2 = Cache.GetObject(Key2a, Key2b, Key2c, Key2d);
            object Other2 = Cache.GetObject(Key2a, Key2b, Key2c, Key2d);

            Assert.AreEqual(Object2, Other2);
            Assert.AreNotEqual(Object2, Object1);
        }

        #endregion

    }
}
