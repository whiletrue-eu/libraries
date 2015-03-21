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

namespace WhileTrue.Classes.Framework._Unittest
{
    [TestFixture]
    public class NotifyChangeExpressionTest
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            DebugLogger.EnableLogging(typeof(NotifyChangeExpression<>), LoggingLevel.Verbose);
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
                get { return property; }
                set
                {
                    this.property = value;
                    (this.PropertyChanged??dummyHandler)(this, new PropertyChangedEventArgs("Property"));
                }
            }

            public static string StaticProperty
            {
                get { return "I'm static!"; }
            }

            public int OtherProperty
            {
                get { return otherproperty; }
                set
                {
                    this.otherproperty = value;
                    (this.PropertyChanged ?? dummyHandler)(this, new PropertyChangedEventArgs("OtherProperty"));
                }
            }

            private TestObject innerObject;
            private int otherproperty;

            public TestObject InnerObject
            {
                get { return innerObject; }
                set
                {
                    this.innerObject = value;
                    (this.PropertyChanged ?? dummyHandler)(this, new PropertyChangedEventArgs("InnerObject"));
                }
            }

            public ObservableCollection<TestObject> ObjectCollection
            {
                get; private set;
            }

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

            NotifyChangeExpression<Func<TestObject, string>> Expr = new NotifyChangeExpression<Func<TestObject, string>>(test => test.InnerObject.Property,EventBindingMode.Strong);

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

            NotifyChangeExpression<Func<TestObject, IEnumerable<TestObject>>> Expr = new NotifyChangeExpression<Func<TestObject, IEnumerable<TestObject>>>(test => test.ObjectCollection, EventBindingMode.Strong);

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

            NotifyChangeExpression<Func<TestObject, string>> Expr = new NotifyChangeExpression<Func<TestObject, string>>(test => test.Property + test.Property + test.OtherProperty,EventBindingMode.Strong);

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

            NotifyChangeExpression<Func<TestObject, string>> Expr = new NotifyChangeExpression<Func<TestObject, string>>(test => test.InnerObject.Property,EventBindingMode.Strong);

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

            NotifyChangeExpression<Func<TestObject, int>> Expr = new NotifyChangeExpression<Func<TestObject, int>>(test => test.ObjectCollection.Max(o=>o.OtherProperty),EventBindingMode.Strong);

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

            NotifyChangeExpression<Func<TestObject, int>> Expr = new NotifyChangeExpression<Func<TestObject, int>>(test => test.ObjectCollection.Max(o => o.OtherProperty),EventBindingMode.Strong);

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
            TestObject.ObjectCollection.Add(InnerTestObject);

            NotifyChangeExpression<Func<TestObject, string>> Expr = new NotifyChangeExpression<Func<TestObject, string>>(test => test.ObjectCollection[0].Property, EventBindingMode.Weak);

            Expr.Invoke(TestObject);
            WeakReference WeakExpression = new WeakReference(Expr);

            // ReSharper disable RedundantAssignment
            Expr = null;
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
            new NotifyChangeExpression<Func<string>>(() => TestObject.StaticProperty, EventBindingMode.Strong);
        }
    }
}

