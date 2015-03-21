using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NUnit.Framework;
using WhileTrue.Classes.Logging;

namespace WhileTrue.Classes.Framework._Unittest
{
    [TestFixture]
    public class ObservableObjectTest
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            DebugLogger.EnableLogging(typeof(NotifyChangeExpression<>), LoggingLevel.Verbose);
            DebugLogger.EnableLogging(typeof(ObservableObject), LoggingLevel.Normal);
        }
        
        public class TestObject : ObservableObject
        {
            private readonly int? id;
            private TestObject subProperty;
            private int property;

            public TestObject(int id)
                : this()
            {
                this.id = id;
            }

            public TestObject()
            {
                this.PropertyCollection = new ObservableCollection<TestObject>();
            }

            public int Property
            {
                set
                {
                    this.SetAndInvoke(() => Property, ref this.property, value, name => this.CustomPropertyChanging(this, new PropertyChangingEventArgs(name)), name => this.CustomPropertyChanged(this, new PropertyChangedEventArgs(name)));
                }
                get
                {
                    return this.property;
                }
            }

            public event PropertyChangingEventHandler CustomPropertyChanging = delegate { };
            public event PropertyChangedEventHandler CustomPropertyChanged = delegate { };

            public TestObject SubProperty
            {
                get
                {
                    return this.subProperty;
                }
                set
                {
                    this.InvokePropertyChanging(() => SubProperty);
                    this.subProperty = value;
                    this.InvokePropertyChanged(() => SubProperty);
                }
            }

            public TestObject OtherSubProperty
            {
                get;
                set;
            }

            private ObservableCollection<TestObject> propertyCollection;

            public ObservableCollection<TestObject> PropertyCollection
            {
                get { return this.propertyCollection; }
                set { this.SetAndInvoke(() => PropertyCollection, ref this.propertyCollection, value); }
            }

            public int CircularDependencyProperty
            {
                get
                {
                    this.InvokePropertyChanged(() => CircularDependencyProperty);
                    return 0;
                }
                set { throw new NotImplementedException(); }
            }

            public override string ToString()
            {
                if (this.id.HasValue)
                {
                    return string.Format("Test Object {0}", this.id.Value);
                }
                else
                {
                    return base.ToString();
                }
            }
        }

        [Test]
        public void Invoke_shall_call_the_property_changed_event()
        {
            bool ChangedEventCalled = false;
            bool ChangingEventCalled = false;
            bool CustomChangedEventCalled = false;
            bool CustomChangingEventCalled = false;
            TestObject Test = new TestObject();
            Test.PropertyChanged += (_, args) => ChangedEventCalled = args.PropertyName == "Property" ? true : false;
            Test.PropertyChanging += (_, args) => ChangingEventCalled = args.PropertyName == "Property" ? true : false;
            Test.CustomPropertyChanged += (_, args) => CustomChangedEventCalled = args.PropertyName == "Property" ? true : false;
            Test.CustomPropertyChanging += (_, args) => CustomChangingEventCalled = args.PropertyName == "Property" ? true : false;
            Test.Property = 42;

            Assert.IsTrue(ChangedEventCalled);
            Assert.IsTrue(ChangingEventCalled);
            Assert.IsTrue(CustomChangedEventCalled);
            Assert.IsTrue(CustomChangingEventCalled);
        }
    }
}