// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System.Collections;
using System.Collections.Generic;
using System.Windows.Data;
using NUnit.Framework;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.SettingStorage;

namespace WhileTrue.Classes.Wpf.PersistentProperties
{
    [TestFixture]
    public class PersistentPropertyTest
    {
        public class Values : ObservableObject
        {
            private string value;

            public Values(string value)
            {
                this.value = value;
            }

            public string Value
            {
                get => value;
                set => SetAndInvoke(nameof(Value), ref this.value, value);
            }
        }

        [Test]
        public void PersistentProperties_shall_be_stored_when_changed_in_UI()
        {
            var ControlTree = new PersistentPropertyControlTree();
            var TestValueStore = (TestValueStore) ControlTree.FindResource("PersistentProperties");

            ControlTree.Three.Text = "Hello, world!";
            ControlTree.DataTemplate.Opacity = 1;

            Assert.AreEqual("Hello, world!", ControlTree.Three.Text);
            Assert.AreEqual("Hello, world!", TestValueStore["One.Two.Three.Text"]);

            Assert.AreEqual(1, ControlTree.DataTemplate.Opacity);
            Assert.AreEqual(1d, TestValueStore["One.Two.ThreeTemplated.Opacity"]);
        }

        [Test]
        public void PersistentProperties_shall_requery_its_value_if_bound_id_is_changed()
        {
            var ControlTree = new PersistentPropertyControlTree();
            var TestValueStore = (TestValueStore) ControlTree.FindResource("PersistentProperties");
            TestValueStore["One.Bound.Three.Text"] = "Hello, binding!";
            TestValueStore["One.Changed.Three.Text"] = "Hello, another binding!";
            TestValueStore["One.Bound.ThreeTemplated.Opacity"] = .1;
            TestValueStore["One.Changed.ThreeTemplated.Opacity"] = .9;

            var Values = new Values("Bound");
            var Binding = new Binding("Value");
            Binding.Mode = BindingMode.OneWay;
            Binding.Source = Values;
            ControlTree.Two.SetBinding(PersistentProperty.IdProperty, Binding);

            Assert.AreEqual("Hello, binding!", ControlTree.Three.Text);
            Assert.AreEqual(.1, ControlTree.DataTemplate.Opacity);

            Values.Value = "Changed";

            Assert.AreEqual("Hello, another binding!", ControlTree.Three.Text);
            Assert.AreEqual(.9, ControlTree.DataTemplate.Opacity);
        }

        [Test]
        public void PersistentProperties_shall_use_default_values_if_no_value_exist()
        {
            var ControlTree = new PersistentPropertyControlTree();

            Assert.AreEqual(0.9, ControlTree.One.Opacity);
            Assert.AreEqual(0.8, ControlTree.Two.Opacity);
            Assert.AreEqual("Hello world", ControlTree.Three.Text);
            Assert.AreEqual(0.7, ControlTree.DataTemplate.Opacity);
        }

        [Test]
        public void PersistentProperties_shall_work_also_with_bound_context_ids()
        {
            var ControlTree = new PersistentPropertyControlTree();
            var TestValueStore = (TestValueStore) ControlTree.FindResource("PersistentProperties");
            TestValueStore["One.Bound.Three.Text"] = "Hello, binding!";
            TestValueStore["One.Bound.ThreeTemplated.Opacity"] = 1;

            var Binding = new Binding("Value");
            Binding.Mode = BindingMode.OneWay;
            Binding.Source = new Values("Bound");
            ControlTree.Two.SetBinding(PersistentProperty.IdProperty, Binding);

            Assert.AreEqual("Hello, binding!", ControlTree.Three.Text);
            Assert.AreEqual(1, ControlTree.DataTemplate.Opacity);
        }

        [Test]
        public void PersistentProperties_with_same_name_shall_always_have_the_same_value()
        {
            var ControlTree = new PersistentPropertyControlTree();

            PersistentProperty.SetId(ControlTree.DataTemplate, "Three");
            ControlTree.Three.Text = "Hello, world!";

            Assert.AreEqual("Hello, world!", ControlTree.Three.Text);
            Assert.AreEqual("Hello, world!", ControlTree.Three2.Text);
            Assert.AreEqual("Hello, world!", ControlTree.DataTemplate.Text);
        }
    }

    public class TestValueStore : ITagValueSettingStore
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public object this[string key]
        {
            get => values[key];
            set => values[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}