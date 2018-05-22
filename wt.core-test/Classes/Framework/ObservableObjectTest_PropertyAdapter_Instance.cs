#pragma warning disable 1591
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ValueParameterNotUsed
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using WhileTrue.Classes.Logging;
#pragma warning disable 219

namespace WhileTrue.Classes.Framework
{
    [TestFixture]
    public class ObservableObjectTest_PropertyAdapter_Instance
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Logging.DebugLogger.EnableLogging(typeof (NotifyChangeExpression<>), LoggingLevel.Verbose);
            Logging.DebugLogger.EnableLogging(typeof (ObservableObject), LoggingLevel.Normal);
        }

        public class TestObject : ObservableObject
        {
            private readonly int? id;
            private TestObject subProperty;
            private int property;
            private int property2;

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
                set { this.SetAndInvoke(nameof(this.Property), ref this.property, value, name => this.CustomPropertyChanged(this, new PropertyChangedEventArgs(name))); }
                get { return this.property; }
            }

            public int Property2
            {
                set { this.SetAndInvoke(nameof(this.Property2), ref this.property2, value, name => this.CustomPropertyChanged(this, new PropertyChangedEventArgs(name))); }
                get { return this.property2; }
            }

            public event PropertyChangingEventHandler CustomPropertyChanging = delegate { };
            public event PropertyChangedEventHandler CustomPropertyChanged = delegate { };

            public TestObject SubProperty
            {
                get { return this.subProperty; }
                set
                {
                    this.subProperty = value;
                    this.InvokePropertyChanged(nameof(this.SubProperty));
                }
            }

            public TestObject OtherSubProperty { get; set; }

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

            public string PropertyThatInvokesChangeOnAccess
            {
                get
                {
                    this.InvokePropertyChanged(nameof(this.PropertyThatInvokesChangeOnAccess));
                    return "PropertyThatInvokesChangeOnAccess";
                }
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

        private class TestProxy : ObservableObject
        {
            private readonly TestObject value;
            private readonly PropertyAdapter<int> propertyAdapter;
            private readonly ReadOnlyPropertyAdapter<int> complexPropertyAdapter;
            private readonly ReadOnlyPropertyAdapter<int> twoPropertiesAdapter;

            public TestProxy(TestObject value)
            {
                this.value = value;
                this.propertyAdapter = this.CreatePropertyAdapter(
                    nameof(this.Property),
                    () => this.value.Property,
                    _ => this.value.Property = _
                    );
                this.twoPropertiesAdapter = this.CreatePropertyAdapter(
                    nameof(this.TwoProperties),
                    () => this.value.Property + this.value.Property2
                    );
                this.complexPropertyAdapter = this.CreatePropertyAdapter(
                    nameof(this.ComplexProperty),
                    () => this.value.Property + this.value.Property + this.value.Property
                    );
            }

            public int Property
            {
                get { return this.propertyAdapter.GetValue(); }
                set { this.propertyAdapter.SetValue(value); }
            }

            public int TwoProperties => this.twoPropertiesAdapter.GetValue();

            public int ComplexProperty => this.complexPropertyAdapter.GetValue();
        }

        [Test]
        public void Event_shall_be_routed_through_proxy_class_when_it_registeres_to_it()
        {
            bool EventCalled = false;
            TestObject TestObject = new TestObject();
            TestProxy TestProxy = new TestProxy(TestObject);

            TestProxy.PropertyChanged += (_, args) => EventCalled = args.PropertyName == "Property" ? true : EventCalled;

            TestProxy.Property.ToString(); //For lazy. execute once

            TestObject.Property = 42;

            Assert.IsTrue(EventCalled);
            Assert.AreEqual(42, TestProxy.Property);
        }

        [Test]
        public void Setter_shall_be_simply_forwarded()
        {
            TestObject TestObject = new TestObject();
            TestProxy TestProxy = new TestProxy(TestObject);

            TestObject.Property = 42;

            TestProxy.Property = 21;

            Assert.AreEqual(21, TestObject.Property);
        }

        [Test]
        public void Adapter_registered_two_two_properties_of_the_same_object_shall_react_on_events_for_both()
        {
            TestObject TestObject = new TestObject();
            TestProxy TestProxy = new TestProxy(TestObject);

            TestObject.Property = 42;
            TestObject.Property2 = 42;

            bool EventCalled;
            TestProxy.PropertyChanged += (s, e) => { if (e.PropertyName == "TwoProperties") EventCalled = true; };

            Assert.AreEqual(84, TestProxy.TwoProperties);

            EventCalled = false;
            TestObject.Property = 21;

            Assert.AreEqual(63, TestProxy.TwoProperties);
            Assert.IsTrue(EventCalled);

            EventCalled = false;
            TestObject.Property2 = 21;

            Assert.AreEqual(42, TestProxy.TwoProperties);
            Assert.IsTrue(EventCalled);


        }

        [Test]
        public void Event_shall_only_be_called_once()
        {
            int EventCallCount = 0;
            TestObject TestObject = new TestObject();
            TestProxy TestProxy = new TestProxy(TestObject);

            TestProxy.PropertyChanged += (_, args) => EventCallCount += args.PropertyName == "ComplexProperty" ? 1 : 0;
            TestProxy.ComplexProperty.ToString(); //For lazy: execute once

            TestObject.Property = 42;

            Assert.AreEqual(1, EventCallCount);
        }



        private class TestProxy2 : ObservableObject
        {
            private readonly TestObject value;
            private readonly ReadOnlyPropertyAdapter<int> otherNamedPropertyAdapter;

            public TestProxy2(TestObject value)
            {
                this.value = value;
                this.otherNamedPropertyAdapter = this.CreatePropertyAdapter(
                    nameof(this.OtherNamedProperty),
                    () => this.value.Property
                    );
            }

            public int OtherNamedProperty => this.otherNamedPropertyAdapter.GetValue();
        }


        [Test]
        public void Event_shall_be_routed_through_proxy_class_when_it_registeres_to_it_and_shall_fire_renamed_events()
        {
            bool EventCalled = false;
            TestObject TestObject = new TestObject();
            TestProxy2 TestProxy = new TestProxy2(TestObject);

            TestProxy.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "OtherNamedProperty":
                        EventCalled = true;
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
            };
            TestProxy.OtherNamedProperty.ToString(); //For lazy: execute once

            TestObject.Property = 42;

            Assert.IsTrue(EventCalled);
            Assert.AreEqual(42, TestProxy.OtherNamedProperty);
        }

        private class TestProxy3 : ObservableObject
        {
            private readonly TestObject value;
            private readonly ReadOnlyPropertyAdapter<int> propertyAdapter;

            public TestProxy3(TestObject value)
            {
                this.value = value;

                this.propertyAdapter = this.CreatePropertyAdapter(
                    nameof(this.Property),
                    () => this.value.SubProperty.Property
                    );
            }

            public int Property => this.propertyAdapter.GetValue();
        }


        [Test]
        public void Event_shall_be_routed_through_proxy_on_subproperty()
        {
            bool EventCalled = false;
            TestObject TestObject = new TestObject {SubProperty = new TestObject()};
            TestProxy3 TestProxy = new TestProxy3(TestObject);

            TestProxy.PropertyChanged += (_, args) => EventCalled = args.PropertyName == "Property";

            TestProxy.Property.ToString(); //For lazy: execute once

            TestObject.SubProperty.Property = 42;

            Assert.IsTrue(EventCalled);
            Assert.AreEqual(42, TestProxy.Property);
        }


        [Test]
        public void Event_on_a_subproperty_shall_be_removed_and_readded_if_subproperty_is_changed()
        {
            TestObject SubProperty1 = new TestObject();
            TestObject SubProperty2 = new TestObject();
            TestObject TestObject = new TestObject {SubProperty = SubProperty1};
            TestProxy3 TestProxy = new TestProxy3(TestObject);

            bool EventCalled;
            TestProxy.PropertyChanged += (_, args) => EventCalled = args.PropertyName == "Property";
            TestObject.SubProperty = SubProperty2;

            EventCalled = false;
            TestProxy.Property.ToString(); //For lazy: execute once

            //old subproperty shall be removed
            SubProperty1.Property = 42;
            Assert.IsFalse(EventCalled);

            TestProxy.Property.ToString(); //For lazy: execute once

            //new subproperty shall be added
            SubProperty2.Property = 42;
            Assert.IsTrue(EventCalled);

            TestProxy.Property.ToString(); //For lazy: execute once

            //shall work if new value is null
            TestObject.SubProperty = null;
            Assert.Throws<NullReferenceException>(() => TestProxy.Property.ToString());
        }

        private class TestProxy5 : ObservableObject
        {
            private readonly TestObject value;
            private readonly ReadOnlyPropertyAdapter<int> propertyAdapter;

            public TestProxy5(TestObject value)
            {
                this.value = value;
                this.propertyAdapter = this.CreatePropertyAdapter(nameof(this.Property), () => this.value.SubProperty.SubProperty.Property);
            }

            public int Property => this.propertyAdapter.GetValue();
        }

        [Test]
        public void Event_on_a_subproperty_shall_be_removed_and_readded_if_subproperty_is_changed_even_on_2nd_level()
        {
            bool EventCalled = false;
            TestObject SubProperty11 = new TestObject();
            TestObject SubProperty1 = new TestObject {SubProperty = SubProperty11};
            TestObject TestObject = new TestObject {SubProperty = SubProperty1};
            TestProxy5 TestProxy = new TestProxy5(TestObject);
            TestObject SubProperty21 = new TestObject();
            TestObject SubProperty2 = new TestObject {SubProperty = SubProperty21};


            TestProxy.PropertyChanged += (_, args) => EventCalled = args.PropertyName == "Property";
            TestProxy.Property.ToString(); //For lazy: execute once

            //subproperty shall be registered while creation
            SubProperty11.Property = 42;
            Assert.IsTrue(EventCalled);

            TestObject.SubProperty = SubProperty2;

            EventCalled = false;

            TestProxy.Property.ToString(); //For lazy: execute once

            //old subproperty shall be removed
            SubProperty11.Property = 42;
            Assert.IsFalse(EventCalled);

            TestProxy.Property.ToString(); //For lazy: execute once

            //new subproperty shall be added
            SubProperty21.Property = 42;
            Assert.IsTrue(EventCalled);

            TestProxy.Property.ToString(); //For lazy: execute once

            //shall work if new value is null
            TestObject.SubProperty = null;
            Assert.Throws<NullReferenceException>(() => TestProxy.Property.ToString());
        }

        private class TestProxy6 : ObservableObject
        {
            private readonly EnumerablePropertyAdapter<TestObject, TestProxy6> collectionPropertyAdapter;

            public TestProxy6(TestObject value, int property = 0)
            {
                this.Source = value;
                this.DependentProperty = property;
                this.collectionPropertyAdapter = this.CreatePropertyAdapter(
                    nameof(this.CollectionProperty),
                    () => TestProxy6.ReturnOrThrow(this.Source.PropertyCollection, this.ThrowException),
                    item => new TestProxy6(item, value.Property));
            }

            private static IEnumerable<TestObject> ReturnOrThrow(ObservableCollection<TestObject> propertyCollection, bool throwException)
            {
                if (throwException)
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    return propertyCollection;
                }
            }

            public IEnumerable<TestProxy6> CollectionProperty => this.collectionPropertyAdapter.GetCollection();

            public TestObject Source { get; }

            public int DependentProperty { get; }

            private bool throwException;

            public bool ThrowException
            {
                get { return this.throwException; }
                set { this.SetAndInvoke(nameof(this.ThrowException), ref this.throwException, value); }
            }
        }

        [Test]
        public void Collection_properties_shall_be_converted_to_observable_collections()
        {
            TestObject TestObject = new TestObject();
            TestProxy6 TestProxy = new TestProxy6(TestObject);

            Assert.IsInstanceOf<INotifyCollectionChanged>(TestProxy.CollectionProperty);
        }

        [Test]
        public void Collection_properties_shall_return_the_items()
        {
            TestObject TestObject = new TestObject();
            TestObject Item1 = new TestObject();
            TestObject Item2 = new TestObject();
            TestObject.PropertyCollection.Add(Item1);
            TestObject.PropertyCollection.Add(Item2);

            TestProxy6 TestProxy = new TestProxy6(TestObject);

            TestProxy6[] Items = TestProxy.CollectionProperty.ToArray();

            Assert.AreEqual(Item1, Items[0].Source);
            Assert.AreEqual(Item2, Items[1].Source);
        }

        [Test]
        public void Collection_properties_shall_reflect_insert_of_item()
        {
            TestObject TestObject = new TestObject();
            TestObject Item1 = new TestObject(1);
            TestObject Item2 = new TestObject(2);
            TestObject Item3 = new TestObject(3);
            TestObject.PropertyCollection.Add(Item1);
            TestObject.PropertyCollection.Add(Item3);

            TestProxy6 TestProxy = new TestProxy6(TestObject);

            TestObject.PropertyCollection.Insert(1, Item2);


            TestProxy6[] Items = TestProxy.CollectionProperty.ToArray();

            Assert.AreEqual(Item1, Items[0].Source);
            Assert.AreEqual(Item2, Items[1].Source);
            Assert.AreEqual(Item3, Items[2].Source);
        }

        [Test]
        public void Collection_properties_shall_reflect_insert_of_item_already_in_list()
        {
            TestObject TestObject = new TestObject();
            TestObject Item1 = new TestObject(1);
            TestObject Item2 = new TestObject(2);
            TestObject.PropertyCollection.Add(Item1);
            TestObject.PropertyCollection.Add(Item2);

            TestProxy6 TestProxy = new TestProxy6(TestObject);

            TestObject.PropertyCollection.Insert(0, Item2);


            TestProxy6[] Items = TestProxy.CollectionProperty.ToArray();

            Assert.AreEqual(Item2, Items[0].Source);
            Assert.AreEqual(Item1, Items[1].Source);
            Assert.AreEqual(Item2, Items[2].Source);
        }

        [Test]
        public void Collection_properties_shall_reflect_deletion_of_item()
        {
            TestObject TestObject = new TestObject();
            TestObject Item1 = new TestObject(1);
            TestObject Item2 = new TestObject(2);
            TestObject Item3 = new TestObject(3);
            TestObject.PropertyCollection.Add(Item1);
            TestObject.PropertyCollection.Add(Item2);
            TestObject.PropertyCollection.Add(Item3);

            TestProxy6 TestProxy = new TestProxy6(TestObject);

            TestObject.PropertyCollection.Remove(Item2);


            TestProxy6[] Items = TestProxy.CollectionProperty.ToArray();

            Assert.AreEqual(Item1, Items[0].Source);
            Assert.AreEqual(Item3, Items[1].Source);
        }

        [Test]
        public void Collection_properties_shall_reflect_deletion_of_item_thats_twice_in_the_list()
        {
            TestObject TestObject = new TestObject();
            TestObject Item1 = new TestObject(1);
            TestObject Item2 = new TestObject(2);
            TestObject.PropertyCollection.Add(Item2);
            TestObject.PropertyCollection.Add(Item1);
            TestObject.PropertyCollection.Add(Item2);

            TestProxy6 TestProxy = new TestProxy6(TestObject);

            TestObject.PropertyCollection.Remove(Item2);


            TestProxy6[] Items = TestProxy.CollectionProperty.ToArray();

            Assert.AreEqual(Item1, Items[0].Source);
            Assert.AreEqual(Item2, Items[1].Source);
        }

        [Test]
        public void Collection_properties_shall_reflect_change_on_complete_reset()
        {
            TestObject TestObject = new TestObject();
            TestObject Item1 = new TestObject(1);
            TestObject Item2 = new TestObject(2);
            TestObject Item3 = new TestObject(3);
            TestObject Item4 = new TestObject(4);
            TestObject Item5 = new TestObject(5);
            TestObject.PropertyCollection.Add(Item1);
            TestObject.PropertyCollection.Add(Item2);
            TestObject.PropertyCollection.Add(Item3);

            TestProxy6 TestProxy = new TestProxy6(TestObject);


            ObservableCollection<TestObject> NewList = new ObservableCollection<TestObject>();
            NewList.Add(Item5);
            NewList.Add(Item4);
            NewList.Add(Item3);
            NewList.Add(Item2);

            List<NotifyCollectionChangedEventArgs> Events = new List<NotifyCollectionChangedEventArgs>();
            ((INotifyCollectionChanged) TestProxy.CollectionProperty).CollectionChanged += (s, e) => Events.Add(e);
            bool ChangedEventCalled = false;
            TestProxy.PropertyChanged += (s, e) => ChangedEventCalled = true;


            TestObject.PropertyCollection = NewList;


            TestProxy6[] Items = TestProxy.CollectionProperty.ToArray();

            Assert.AreEqual(Item5, Items[0].Source);
            Assert.AreEqual(Item4, Items[1].Source);
            Assert.AreEqual(Item3, Items[2].Source);
            Assert.AreEqual(Item2, Items[3].Source);

            Assert.AreEqual(5, Events.Count); // insert #5 on index 0, insert #4 on index 1, move #3 from 4 to 2, move #2 from 4 to 3, remove #1 from index 4
        }

        [Test]
        public void Collection_properties_shall_support_clearing_the_list()
        {
            TestObject TestObject = new TestObject();
            TestObject Item1 = new TestObject(1);
            TestObject Item2 = new TestObject(2);
            TestObject Item3 = new TestObject(3);
            TestObject.PropertyCollection.Add(Item1);
            TestObject.PropertyCollection.Add(Item2);
            TestObject.PropertyCollection.Add(Item3);

            TestProxy6 TestProxy = new TestProxy6(TestObject);


            List<NotifyCollectionChangedEventArgs> Events = new List<NotifyCollectionChangedEventArgs>();
            ((INotifyCollectionChanged) TestProxy.CollectionProperty).CollectionChanged += (s, e) => Events.Add(e);
            bool ChangedEventCalled = false;
            TestProxy.PropertyChanged += (s, e) => ChangedEventCalled = true;


            TestObject.PropertyCollection.Clear();


            TestProxy6[] Items = TestProxy.CollectionProperty.ToArray();

            Assert.AreEqual(0, Items.Length);

            Assert.AreEqual(1, Events.Count); // clear
        }

        private class TestProxy10 : ObservableObject
        {
            private readonly EnumerablePropertyAdapter<TestObject, TestProxy10> collectionPropertyAdapter;

            public TestProxy10(TestObject value)
            {
                this.Source = value;
                this.collectionPropertyAdapter = this.CreatePropertyAdapter(
                    nameof(this.CollectionProperty),
                    () => from Item in this.Source.PropertyCollection where Item.Property == 0 select Item,
                    item => new TestProxy10(item));
            }

            public IEnumerable<TestProxy10> CollectionProperty => this.collectionPropertyAdapter.GetCollection();

            public TestObject Source { get; }
        }


        [Test]
        public void Collection_shall_update_correctly_if_source_items_are_filtered_using_a_property()
        {
            TestObject TestObject = new TestObject();
            TestObject Item1 = new TestObject(1) {Property = 1};
            TestObject Item2 = new TestObject(2) {Property = 0};
            TestObject Item3 = new TestObject(3) {Property = 1};
            TestObject.PropertyCollection.Add(Item1);
            TestObject.PropertyCollection.Add(Item2);
            TestObject.PropertyCollection.Add(Item3);

            TestProxy10 TestProxy = new TestProxy10(TestObject);


            List<NotifyCollectionChangedEventArgs> Events = new List<NotifyCollectionChangedEventArgs>();
            ((INotifyCollectionChanged) TestProxy.CollectionProperty).CollectionChanged += (s, e) => Events.Add(e);
            bool ChangedEventCalled = false;
            TestProxy.PropertyChanged += (s, e) => ChangedEventCalled = true;



            TestProxy10[] Items = TestProxy.CollectionProperty.ToArray();
            Assert.AreEqual(1, Items.Length);
            Assert.AreEqual(Item2, Items[0].Source);

            //Switch on/off same value

            Item2.Property = 1;

            Items = TestProxy.CollectionProperty.ToArray();
            Assert.AreEqual(0, Items.Length);

            Assert.AreEqual(1, Events.Count); // clear

            Events.Clear();
            ChangedEventCalled = false;

            Item2.Property = 0;

            Items = TestProxy.CollectionProperty.ToArray();
            Assert.AreEqual(1, Items.Length);
            Assert.AreEqual(Item2, Items[0].Source);

            Assert.AreEqual(1, Events.Count); // clear

            //Switch on/off different value
            Events.Clear();
            ChangedEventCalled = false;

            Item2.Property = 1;

            Items = TestProxy.CollectionProperty.ToArray();
            Assert.AreEqual(0, Items.Length);

            Assert.AreEqual(1, Events.Count); // clear

            Events.Clear();
            ChangedEventCalled = false;

            Item1.Property = 0;

            Items = TestProxy.CollectionProperty.ToArray();
            Assert.AreEqual(1, Items.Length);
            Assert.AreEqual(Item1, Items[0].Source);

            Assert.AreEqual(1, Events.Count); // clear
        }


        [Test]
        public void Collection_properties_shall_be_completely_reset_if_inputs_to_adapter_creation_expression_change()
        {
            TestObject TestObject = new TestObject();
            TestObject Item1 = new TestObject(1);
            TestObject Item2 = new TestObject(2);
            TestObject Item3 = new TestObject(3);
            TestObject.PropertyCollection.Add(Item1);
            TestObject.PropertyCollection.Add(Item2);
            TestObject.PropertyCollection.Add(Item3);

            TestProxy6 TestProxy = new TestProxy6(TestObject);

            //All objects must be re-created because of the dependency to 'property' in the adapter creation
            TestObject.Property = 42;


            TestProxy6[] Items = TestProxy.CollectionProperty.ToArray();

            Assert.AreEqual(42, Items[0].DependentProperty);
            Assert.AreEqual(42, Items[1].DependentProperty);
            Assert.AreEqual(42, Items[2].DependentProperty);
        }

        [Test]
        public void Collection_properties_shall_issue_the_exception_if_one_was_thrown_retrieving_the_value()
        {
            TestObject TestObject = new TestObject();
            TestObject Item1 = new TestObject(1);
            TestObject Item2 = new TestObject(2);
            TestObject.PropertyCollection.Add(Item1);
            TestObject.PropertyCollection.Add(Item2);

            TestProxy6 TestProxy = new TestProxy6(TestObject);
            TestProxy.ThrowException = true;

            Assert.Throws<InvalidOperationException>(() => TestProxy.CollectionProperty.ToArray());

            TestProxy.ThrowException = false;
            TestProxy6[] Items = TestProxy.CollectionProperty.ToArray();

            Assert.AreEqual(Item1, Items[0].Source);
            Assert.AreEqual(Item2, Items[1].Source);
        }


        private class TestProxy4 : ObservableObject
        {
            private readonly TestObject2 value;
            private readonly ReadOnlyPropertyAdapter<int> propertyAdapter;

            public TestProxy4(TestObject2 value)
            {
                this.value = value;
                this.propertyAdapter = this.CreatePropertyAdapter(nameof(this.Property), () => this.value.Property);
            }

            public int Property => this.propertyAdapter.GetValue();
        }

        private class TestObject2
        {
            public int Property { get; set; }
        }

        [Test]
        public void Value_change_shall_be_ignored_if_subproperty_does_not_implement_INotifyPropertyChanged()
        {
            TestObject2 TestObject = new TestObject2 {Property = 21};
            TestProxy4 TestProxy = new TestProxy4(TestObject);

            Assert.AreEqual(21, TestProxy.Property);

            bool EventCalled = false;
            TestProxy.Property.ToString();
            TestProxy.PropertyChanged += (s, e) => { if (e.PropertyName == "Property") EventCalled = true; };


            TestObject.Property = 42;

            Assert.IsFalse(EventCalled);

            //Value shall not be changed -> it is cached, no event received, cache not updated
            Assert.AreEqual(21, TestProxy.Property);
        }


        private class WeakAdapter : ObservableObject
        {
            private readonly TestObject value;
            private readonly ReadOnlyPropertyAdapter<int> propertyAdapter;

            public WeakAdapter(TestObject value)
            {
                this.value = value;
                this.propertyAdapter = this.CreatePropertyAdapter(nameof(this.Property), () => this.value.Property);
            }

            public int Property
            {
                get
                {
                    try
                    {
                        return this.propertyAdapter.GetValue();
                    }
                    catch
                    {
                        return 0;
                    }
                }
            }
        }

        [Test]
        public void weak_property_adapter_shall_allow_adapter_to_be_released()
        {
            WeakReference Create(TestObject testObject)
            {
                WeakAdapter WeakAdapter = new WeakAdapter(testObject);
                WeakReference weakAdapterReference = new WeakReference(WeakAdapter);
                return weakAdapterReference;
            }

            TestObject TestObject = new TestObject();
            WeakReference WeakAdapterReference = Create(TestObject);

            GC.Collect();

            Assert.IsFalse(WeakAdapterReference.IsAlive);
        }



        private class TestProxy7 : ObservableObject
        {
            private readonly TestObject value;
            private readonly PropertyAdapter<int> propertyAdapter;

            public TestProxy7(TestObject value)
            {
                this.value = value;
                this.propertyAdapter = this.CreatePropertyAdapter(
                    nameof(this.Property),
                    () => this.value.CircularDependencyProperty,
                    val => this.value.CircularDependencyProperty = val
                    );
            }

            public int Property
            {
                get { return this.propertyAdapter.GetValue(); }
                set { this.propertyAdapter.SetValue(value); }
            }
        }


#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private class TestSource2 : ObservableObject
        {
            public TestSource2()
            {
                this.Value = "Hello, world";
            }

            public string Value { get; set; }

            public override bool Equals(object obj)
            {
                return true;
            }
        }
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

        private class TestProxy8 : ObservableObject
        {
            public TestSource2 Source { get; }

            public TestProxy8(TestSource2 source)
            {
                this.Source = source;
                this.valueAdapter = this.CreatePropertyAdapter(
                    nameof(this.Value),
                    () => this.Source.Value,
                    _ => this.Source.Value = _
                    );
            }

            private readonly PropertyAdapter<string> valueAdapter;

            public string Value
            {
                get { return this.valueAdapter.GetValue(); }
                set { this.valueAdapter.SetValue(value); }
            }

            public override bool Equals(object obj)
            {
                if (obj is TestProxy8)
                {
                    return this.Source.Equals(((TestProxy8) obj).Source);
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return this.Source.GetHashCode();
            }
        }


        [Test]
        public void StackOverflow_shall_be_avoided_if_objects_do_not_implement_equals_correctly()
        {
            //Use case: 'equals' is overwritten and indicates equality for this object, so that a
            //changed event seems to come from the wrong object (evaluated on the 'equal' object instead
            //of the one that really fired it). In some circumstances, this can lead to an endless loop.
            //because of this, 'referenceequals' must be taken.
            //the situation is very special (see implementation above) and comes down to implementing
            //an equal without implementing gethascode and redirecting the equal and gethash from the proxy
            //to the wrapped class
            // ReSharper disable once ObjectCreationAsStatement
            new TestProxy8(new TestSource2());
        }

        private class TestProxy9 : ObservableObject
        {
            private readonly EnumerablePropertyAdapter<TestObject, TestProxy9> collectionAdapter;
            private readonly ReadOnlyPropertyAdapter<string> collectionDerivedAdapter;

            public TestProxy9(TestObject source)
            {
                this.collectionAdapter = this.CreatePropertyAdapter(
                    nameof(this.Collection),
                    () => source.PropertyCollection,
                    value => new TestProxy9(value)
                    );
                this.collectionDerivedAdapter = this.CreatePropertyAdapter(
                    nameof(this.CollectionDerived),
                    () => string.Join("-", (from Value in this.Collection select Value.ToString()).ToArray())
                    );
            }

            public string CollectionDerived => this.collectionDerivedAdapter.GetValue();

            protected IEnumerable<TestProxy9> Collection => this.collectionAdapter.GetCollection();
        }


        [Test]
        public void Re_Registrations_after_first_event_shall_succeed()
        {
            TestObject TestObject = new TestObject();
            TestProxy9 Proxy = new TestProxy9(TestObject);

            bool PropertyChanged = false;
            Proxy.PropertyChanged += (s, e) => { if (e.PropertyName == "CollectionDerived") PropertyChanged = true; };

            Proxy.CollectionDerived.GetHashCode(); //For lazy: execute once

            TestObject.PropertyCollection.Add(new TestObject());
            Assert.IsTrue(PropertyChanged);

            PropertyChanged = false;
            Proxy.CollectionDerived.GetHashCode(); //For lazy: execute once

            TestObject.PropertyCollection.Add(new TestObject());
            Assert.IsTrue(PropertyChanged);
        }


        #region new test classes

        private delegate void RowsChangedEventHandler(object sender, EventArgs args);

        private sealed class ModelWithRows
        {
            private readonly List<RowThing> rows = new List<RowThing>();

            public IEnumerable<RowThing> Rows => this.rows;

            public void AddRow(RowThing rowThing)
            {
                this.rows.Add(rowThing);
                ModelWithRows.RowsChanged(this, new EventArgs());
            }

            public void InvokeRowChanged()
            {
                ModelWithRows.RowsChanged(null, EventArgs.Empty);
            }

            public static event RowsChangedEventHandler RowsChanged = delegate { };
        }

        private sealed class RowThing
        {
            public RowThing(int id)
            {
                this.ID = id;
            }

            public int ID { get; set; }
        }

        private sealed class Thing
        {
            private static readonly Dictionary<RowThing, Thing> instances = new Dictionary<RowThing, Thing>();

            public static Thing GetInstance(RowThing rowThing)
            {
                Thing TheThing;
                if (Thing.instances.ContainsKey(rowThing))
                {
                    TheThing = Thing.instances[rowThing];
                }
                else
                {
                    TheThing = new Thing(rowThing);
                    Thing.instances.Add(rowThing, TheThing);
                }
                return TheThing;
            }

            private readonly RowThing row;

            private Thing(RowThing row)
            {
                this.row = row;
            }

            public int ID => this.row.ID;
        }

        private sealed class ThingContainer : ObservableObject
        {
            private readonly ModelWithRows model;

            public ThingContainer(ModelWithRows model)
            {
                this.model = model;
                ModelWithRows.RowsChanged += this.ModelWithRows_RowsChanged;
            }

            private void ModelWithRows_RowsChanged(object sender, EventArgs args)
            {
                this.InvokePropertyChanged(nameof(this.EvenThings));
            }

            public IEnumerable<Thing> AllThings => from Row in this.model.Rows select Thing.GetInstance(Row);

            public IEnumerable<Thing> EvenThings => from Row in this.model.Rows where Row.ID%2 == 0 select Thing.GetInstance(Row);
        }

        private sealed class ThingAdapter : ObservableObject
        {
            private static readonly Dictionary<Thing, ThingAdapter> instances = new Dictionary<Thing, ThingAdapter>();

            public static ThingAdapter GetInstance(Thing thing)
            {
                ThingAdapter TheThingAdapter;
                if (ThingAdapter.instances.ContainsKey(thing))
                {
                    TheThingAdapter = ThingAdapter.instances[thing];
                }
                else
                {
                    TheThingAdapter = new ThingAdapter(thing);
                    ThingAdapter.instances.Add(thing, TheThingAdapter);
                }
                return TheThingAdapter;
            }

            private readonly Thing thing;

            private ThingAdapter(Thing thing)
            {
                this.thing = thing;
            }

            public int ID => this.thing.ID;
        }

        private sealed class ThingContainerAdapter : ObservableObject
        {
            private readonly EnumerablePropertyAdapter<Thing, ThingAdapter> oddThingsPropertyAdapter;

            public ThingContainerAdapter(ThingContainer container)
            {
                this.oddThingsPropertyAdapter = this.CreatePropertyAdapter(
                    nameof(this.OddThings),
                    () => from Thing in container.AllThings
                        where container.EvenThings.Contains(Thing) == false
                        select Thing,
                    value => ThingAdapter.GetInstance(value));
            }

            public IEnumerable<ThingAdapter> OddThings => this.oddThingsPropertyAdapter.GetCollection();
        }

        #endregion

        [Test]
        public void Complex_Linq_expressions_shall_be_supported_()
        {
            Logging.DebugLogger.EnableLogging(typeof (object), LoggingLevel.Verbose);

            ModelWithRows Model = new ModelWithRows();
            RowThing Thing0 = new RowThing(0);
            RowThing Thing1 = new RowThing(1);
            RowThing Thing2 = new RowThing(2);
            Model.AddRow(Thing0);
            Model.AddRow(Thing1);
            Model.AddRow(Thing2);
            ThingContainer Container = new ThingContainer(Model);
            ThingContainerAdapter ContainerAdapter = new ThingContainerAdapter(Container);

            int OddCollectionChangedEventCount = 0;
            ((INotifyCollectionChanged) ContainerAdapter.OddThings).CollectionChanged += (sender, args) => OddCollectionChangedEventCount++;

            Assert.AreEqual(1, ContainerAdapter.OddThings.Count());
            Assert.AreEqual(1, ContainerAdapter.OddThings.First().ID);

            Thing0.ID = 3;
            Model.InvokeRowChanged();

            Assert.AreEqual(1, OddCollectionChangedEventCount);
            Assert.AreEqual(2, ContainerAdapter.OddThings.Count());
            Assert.AreEqual(3, ContainerAdapter.OddThings.First().ID);
            Assert.AreEqual(1, ContainerAdapter.OddThings.ElementAt(1).ID);
        }
    }
}
