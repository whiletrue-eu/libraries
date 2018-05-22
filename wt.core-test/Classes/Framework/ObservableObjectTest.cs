using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NUnit.Framework;
using WhileTrue.Classes.Logging;

namespace WhileTrue.Classes.Framework
{
    [TestFixture]
    public class ObservableObjectTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Logging.DebugLogger.EnableLogging(typeof(NotifyChangeExpression<>), LoggingLevel.Verbose);
            Logging.DebugLogger.EnableLogging(typeof(ObservableObject), LoggingLevel.Normal);
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
                    this.SetAndInvoke(nameof(this.Property), ref this.property, value, name => this.CustomPropertyChanged(this, new PropertyChangedEventArgs(name)));
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
                    this.subProperty = value;
                    this.InvokePropertyChanged(nameof(this.SubProperty));
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
                set { this.SetAndInvoke(nameof(this.PropertyCollection), ref this.propertyCollection, value); }
            }

            public int CircularDependencyProperty
            {
                get
                {
                    this.InvokePropertyChanged(nameof(this.CircularDependencyProperty));
                    return 0;
                }
                set { throw new NotImplementedException(); }
            }

            public override string ToString()
            {
                if (this.id.HasValue)
                {
                    return $"Test Object {this.id.Value}";
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
            bool CustomChangedEventCalled = false;
            TestObject Test = new TestObject();
            Test.PropertyChanged += (_, args) => ChangedEventCalled = args.PropertyName == "Property";
            Test.CustomPropertyChanged += (_, args) => CustomChangedEventCalled = args.PropertyName == "Property";
            Test.Property = 42;

            Assert.IsTrue(ChangedEventCalled);
            Assert.IsTrue(CustomChangedEventCalled);
        }
    }
}