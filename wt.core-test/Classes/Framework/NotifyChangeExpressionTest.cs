// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable 1591 //xml doc

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using WhileTrue.Classes.Logging;

namespace WhileTrue.Classes.Framework
{
    [TestFixture]
    public class NotifyChangeExpressionTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Logging.DebugLogger.EnableLogging(typeof(NotifyChangeExpression<>), LoggingLevel.Verbose);
        }

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
                    (this.PropertyChanged??TestObject.dummyHandler)(this, new PropertyChangedEventArgs("Property"));
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
            TestObject.InnerObject= new TestObject();
            TestObject.InnerObject.Property = "Hello, World";

            NotifyChangeExpression<Func<TestObject, string>> Expr = new NotifyChangeExpression<Func<TestObject, string>>(test => test.InnerObject.Property);

            bool Changed = false;
            Expr.Changed += delegate { Changed = true; };

            Expr.Invoke(TestObject);

            TestObject.InnerObject.Property = "Hello, new World!";
            Assert.IsTrue(Changed);
        }

        [Test]
        public void Changed_shall_be_thrown_on_a_collection_that_is_the_result_of_the_expression()
        {
            TestObject TestObject = new TestObject();

            NotifyChangeExpression<Func<TestObject, IEnumerable<TestObject>>> Expr = new NotifyChangeExpression<Func<TestObject, IEnumerable<TestObject>>>(test => test.ObjectCollection);

            bool Changed = false;
            Expr.Changed += delegate { Changed = true; };

            Expr.Invoke(TestObject);

            TestObject.ObjectCollection.Add(new TestObject());
            Assert.IsTrue(Changed);
        }

        [Test]
        public void Changed_shall_be_thrown_only_once_on_a_property_change()
        {
            TestObject TestObject = new TestObject();
            TestObject.InnerObject = new TestObject();
            TestObject.Property = "Hello, World";

            NotifyChangeExpression<Func<TestObject, string>> Expr = new NotifyChangeExpression<Func<TestObject, string>>(test => test.Property + test.Property + test.OtherProperty);

            int ChangeCount = 0;
            Expr.Changed += delegate { ChangeCount++; };

            Expr.Invoke(TestObject);

            TestObject.Property = "Hello";
            Assert.AreEqual(1,ChangeCount);
        }

        [Test]
        public void Changed_shall_NOT_be_thrown_after_value_was_retrieved_through_expression_on_a_property_change_which_was_not_in_the_expression()
        {
            TestObject TestObject = new TestObject();
            TestObject.InnerObject = new TestObject();
            TestObject.InnerObject.Property = "Hello, World";

            NotifyChangeExpression<Func<TestObject, string>> Expr = new NotifyChangeExpression<Func<TestObject, string>>(test => test.InnerObject.Property);

            bool Changed = false;
            Expr.Changed += delegate { Changed = true; };

            Expr.Invoke(TestObject);

            TestObject.InnerObject.OtherProperty = 42;
            Assert.IsFalse(Changed);
        }

        [Test]
        public void Changed_shall_be_thrown_after_value_was_retrieved_through_expression_on_a_collection_modification()
        {
            TestObject TestObject = new TestObject();
            TestObject.ObjectCollection.Add(new TestObject());
            TestObject.ObjectCollection[0].OtherProperty = 42;

            NotifyChangeExpression<Func<TestObject, int>> Expr = new NotifyChangeExpression<Func<TestObject, int>>(test => test.ObjectCollection.Max(o=>o.OtherProperty));

            bool Changed = false;
            Expr.Changed += delegate { Changed = true; };

            Expr.Invoke(TestObject);

            TestObject.ObjectCollection.Add(new TestObject{OtherProperty = 84});
            Assert.IsTrue(Changed);
        }

        [Test]
        public void events_shall_be_deregistered_after_event_was_thrown()
        {
            TestObject TestObject = new TestObject();
            TestObject TestObject2 = new TestObject();
            TestObject.ObjectCollection.Add(TestObject2);
            TestObject.ObjectCollection[0].OtherProperty = 42;

            NotifyChangeExpression<Func<TestObject, int>> Expr = new NotifyChangeExpression<Func<TestObject, int>>(test => test.ObjectCollection.Max(o => o.OtherProperty));

            Expr.Invoke(TestObject);

            Assert.IsTrue(TestObject.IsEventHandlerRegistered());
            Assert.IsTrue(TestObject.IsEventHandlerRegistered());

            bool Changed = false;
            Expr.Changed += delegate { Changed = true; };

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

            WeakReference Create() //Scope to allow garbage collection
            {
                TestObject.ObjectCollection.Add(InnerTestObject);

                NotifyChangeExpression<Func<TestObject, string>> Expr = new NotifyChangeExpression<Func<TestObject, string>>(test => test.ObjectCollection[0].Property);

                Expr.Invoke(TestObject);
                return new WeakReference(Expr);
            }

            WeakReference WeakExpression = Create();

            GC.Collect();
            // ReSharper restore RedundantAssignment

            Assert.IsFalse(WeakExpression.IsAlive);

            //throw event to let the weakdelegates remove themselves (for coverage)
            InnerTestObject.Property = "Hello, world";
            TestObject.ObjectCollection.Clear();
        }


        [Test]
        public void It_shall_possible_to_use_static_members()
        {
            //Events on static properties is not supported (INotifyPropertyChanged only)
            new NotifyChangeExpression<Func<string>>(() => TestObject.StaticProperty);
        }
    }
}

