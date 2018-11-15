using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

namespace WhileTrue.Classes.Framework
{
    [TestFixture]
    public class ObservableExpressionFactoryTest
    {
        private class TestObject : INotifyPropertyChanged
        {
            private static readonly PropertyChangedEventHandler dummyHandler = delegate { };
            public TestObject()
            {
                this.ObjectCollection = new ObservableCollection<TestObject>();
            }

            private string property;

            public string Property
            {
                get { return this.property; }
                set
                {
                    this.property = value;
                    (this.PropertyChanged ?? TestObject.dummyHandler)(this, new PropertyChangedEventArgs("Property"));
                }
            }

            public static string StaticProperty => "I'm static!";

            public int OtherProperty
            {
                get { return this.otherproperty; }
                set
                {
                    this.otherproperty = value;
                    (this.PropertyChanged ?? TestObject.dummyHandler)(this, new PropertyChangedEventArgs("OtherProperty"));
                }
            }

            private TestObject innerObject;
            private int otherproperty;

            public TestObject InnerObject
            {
                get { return this.innerObject; }
                set
                {
                    this.innerObject = value;
                    (this.PropertyChanged ?? TestObject.dummyHandler)(this, new PropertyChangedEventArgs("InnerObject"));
                }
            }

            public ObservableCollection<TestObject> ObjectCollection
            {
                get; }

            public bool IsEventHandlerRegistered()
            {
                return this.PropertyChanged != null;
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }


        [Test]
        public void Changed_shall_be_thrown_after_value_was_retrieved_through_expression_on_a_property_change()
        {
            TestObject TestObject = new TestObject();
            TestObject.InnerObject = new TestObject();
            TestObject.InnerObject.Property = "Hello, World";

            Func<TestObject, ObservableExpressionFactory.EventSink, string> CompiledExpression = ObservableExpressionFactory.Compile((TestObject test) => test.InnerObject.Property);

            bool Changed = false;

            string Result = CompiledExpression(TestObject, new ObservableExpressionFactory.EventSink(delegate { Changed = true; }));

            Assert.That(Result, Is.EqualTo("Hello, World"));
            TestObject.InnerObject.Property = "Hello, new World!";
            Assert.IsTrue(Changed);
        }

        [Test]
        public void Changed_shall_be_thrown_on_a_collection_that_is_the_result_of_the_expression()
        {
            TestObject TestObject = new TestObject();

            Func<TestObject, ObservableExpressionFactory.EventSink, ObservableCollection<TestObject>> CompiledExpression = ObservableExpressionFactory.Compile((TestObject test) => test.ObjectCollection);

            bool Changed = false;

            CompiledExpression(TestObject, new ObservableExpressionFactory.EventSink(delegate { Changed = true; }));

            TestObject.ObjectCollection.Add(new TestObject());
            Assert.IsTrue(Changed);
        }

        [Test]
        public void Changed_shall_be_thrown_only_once_on_a_property_change()
        {
            TestObject TestObject = new TestObject();
            TestObject.InnerObject = new TestObject();
            TestObject.Property = "Hello, World";

            Func<TestObject, ObservableExpressionFactory.EventSink, string> CompiledExpression = ObservableExpressionFactory.Compile((TestObject test) => test.Property + test.Property + test.OtherProperty);

            int ChangeCount = 0;

            CompiledExpression(TestObject, new ObservableExpressionFactory.EventSink(delegate { ChangeCount++; }));
            
            TestObject.Property = "Hello";
            Assert.AreEqual(1, ChangeCount);
        }

        [Test]
        public void Changed_shall_NOT_be_thrown_after_value_was_retrieved_through_expression_on_a_property_change_which_was_not_in_the_expression()
        {
            TestObject TestObject = new TestObject();
            TestObject.InnerObject = new TestObject();
            TestObject.InnerObject.Property = "Hello, World";

            Func<TestObject, ObservableExpressionFactory.EventSink, string> CompiledExpression = ObservableExpressionFactory.Compile((TestObject test) => test.InnerObject.Property);

            bool Changed = false;
            CompiledExpression(TestObject, new ObservableExpressionFactory.EventSink(delegate { Changed = true; }));

            TestObject.InnerObject.OtherProperty = 42;
            Assert.IsFalse(Changed);
        }

        [Test]
        public void Changed_shall_be_thrown_after_value_was_retrieved_through_expression_on_a_collection_modification()
        {
            TestObject TestObject = new TestObject();
            TestObject.ObjectCollection.Add(new TestObject());
            TestObject.ObjectCollection[0].OtherProperty = 42;

            Func<TestObject, ObservableExpressionFactory.EventSink, int> CompiledExpression = ObservableExpressionFactory.Compile((TestObject test) => test.ObjectCollection.Max(o => o.OtherProperty));

            bool Changed = false;
            CompiledExpression(TestObject, new ObservableExpressionFactory.EventSink(delegate { Changed = true; }));

            TestObject.ObjectCollection.Add(new TestObject { OtherProperty = 84 });
            Assert.IsTrue(Changed);
        }

        [Test]
        public void events_shall_be_deregistered_after_event_was_thrown()
        {
            TestObject TestObject = new TestObject();
            TestObject TestObject2 = new TestObject();
            TestObject.ObjectCollection.Add(TestObject2);
            TestObject.ObjectCollection[0].OtherProperty = 42;

            //evaluate side effects from other tests
            Assert.IsFalse(TestObject.IsEventHandlerRegistered());
            Assert.IsFalse(TestObject.IsEventHandlerRegistered());

            Func<TestObject, ObservableExpressionFactory.EventSink, int> CompiledExpression = ObservableExpressionFactory.Compile((TestObject test) => test.ObjectCollection.Max(o => o.OtherProperty));

            CompiledExpression(TestObject, new ObservableExpressionFactory.EventSink(delegate { }));
        

            Assert.IsTrue(TestObject.IsEventHandlerRegistered());
            Assert.IsTrue(TestObject.IsEventHandlerRegistered());

            bool Changed = false;
            CompiledExpression(TestObject, new ObservableExpressionFactory.EventSink(delegate { Changed = true; }));

            TestObject.ObjectCollection[0].OtherProperty = 42;

            Assert.IsFalse(TestObject.IsEventHandlerRegistered());
            Assert.IsFalse(TestObject.IsEventHandlerRegistered());

            Changed = false;
            TestObject.ObjectCollection.Clear();

            Assert.IsFalse(Changed);
        }


        [Test]
        public void Weak_event_binding_shall_allow_expression_to_be_collected()
        {
            TestObject TestObject = new TestObject();
            TestObject InnerTestObject = new TestObject();
            TestObject.ObjectCollection.Add(InnerTestObject);

            (WeakReference , WeakReference) Create()
            {
                Func<TestObject, ObservableExpressionFactory.EventSink, string> CompiledExpression = ObservableExpressionFactory.Compile((TestObject test) => test.ObjectCollection[0].Property);

                ObservableExpressionFactory.EventSink Callback = new ObservableExpressionFactory.EventSink(delegate { });
                CompiledExpression(TestObject, Callback);
                return (new WeakReference(CompiledExpression), new WeakReference(Callback));
            }

            (WeakReference WeakExpression, WeakReference WeakCallback) = Create();

            GC.Collect();
            // ReSharper restore RedundantAssignment

            Assert.IsFalse(WeakExpression.IsAlive);
            Assert.IsFalse(WeakCallback.IsAlive);

            //throw event to let the weakdelegates remove themselves (for coverage)
            InnerTestObject.Property = "Hello, world";
            TestObject.ObjectCollection.Clear();
        }

       


        [Test]
        public void It_shall_possible_to_use_static_members()
        {
            //Events on static properties is not supported (INotifyPropertyChanged only)
            ObservableExpressionFactory.Compile(()  => TestObject.StaticProperty);
        }
    }
}