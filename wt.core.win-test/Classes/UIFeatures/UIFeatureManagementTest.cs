// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using NUnit.Framework;
using WhileTrue.Classes.Framework;
using WhileTrue.Components.UIFeatures;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    [TestFixture]
    public class UIFeatureManagementTest
    {
        private class Feature
        {
            public Feature(bool isVisible, bool isEnabled)
            {
                IsVisible = isVisible;
                IsEnabled = isEnabled;
            }

            public bool IsVisible { get; }

            public bool IsEnabled { get; }
        }


        private class TestFeatureSource : IUiFeatureManagerSource
        {
            private readonly Dictionary<string, Feature> rules;
            private bool? enabled;
            private bool? visible;

            public TestFeatureSource(Dictionary<string, Feature> rules)
            {
                this.rules = rules;
            }

            public TestFeatureSource(bool enabled, bool visible)
                : this(new Dictionary<string, Feature>())
            {
                this.enabled = enabled;
                this.visible = visible;
            }

            public bool Visible
            {
                private get
                {
                    if (visible.HasValue)
                        return visible.Value;
                    throw new InvalidOperationException();
                }
                set
                {
                    visible = value;
                    FeaturesChanged(this, EventArgs.Empty);
                }
            }

            public bool Enabled
            {
                private get
                {
                    if (enabled.HasValue)
                        return enabled.Value;
                    throw new InvalidOperationException();
                }
                set
                {
                    enabled = value;
                    FeaturesChanged(this, EventArgs.Empty);
                }
            }

            public bool IsVisible(string context)
            {
                bool State;
                if (rules.ContainsKey(context))
                    State = rules[context].IsVisible;
                else
                    try
                    {
                        State = Visible;
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException($"visible state cannot be found for {context}", e);
                    }

                Debug.WriteLine("visible state {1} returned for {0}", context, State);
                return State;
            }

            public bool IsEnabled(string context)
            {
                bool State;
                if (rules.ContainsKey(context))
                    State = rules[context].IsEnabled;
                else
                    try
                    {
                        State = Enabled;
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException($"enabled state cannot be found for {context}", e);
                    }

                Debug.WriteLine("visible state {1} returned for {0}", context, State);
                return State;
            }

            public event EventHandler<EventArgs> FeaturesChanged = delegate { };
        }

        [Test]
        public void Feature_Bindings_shall_query_the_right_bound_contextids()
        {
            var Rules = new Dictionary<string, Feature>
            {
                {"One/Two/BoundThree", new Feature(true, false)},
                {"One/Two/BoundTemplatedThree", new Feature(true, false)}
            };

            var TestSource = new TestFeatureSource(Rules);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            ControlTree.DataContext = new ContextValues("BoundThree");
            ControlTree.Three.SetBinding(UiFeatureManagement.ContextProperty, "Context");
            ControlTree.TemplatedThree.DataContext = new ContextValues("BoundTemplatedThree");
            ControlTree.TemplatedThree.SetBinding(UiFeatureManagement.ContextProperty, "Context");
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Visible, ControlTree.TemplatedThree.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);
        }

        [Test]
        public void Feature_Bindings_shall_query_the_right_contextids()
        {
            var Rules = new Dictionary<string, Feature>
            {
                {"One/Two/Three", new Feature(true, false)},
                {"One/Two/TemplatedThree", new Feature(true, false)},
                {"One/Two/OtherThree", new Feature(true, true)}
            };

            var TestSource = new TestFeatureSource(Rules);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.AreEqual(Visibility.Visible, ControlTree.TemplatedThree.Visibility);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);
            Assert.AreEqual(Visibility.Visible, ControlTree.OtherThree.Visibility);
            Assert.IsTrue(ControlTree.OtherThree.IsEnabled);
        }

        [Test]
        public void Feature_Bindings_shall_reflect_values_of_manager()
        {
            var TestSource = new TestFeatureSource(true, true);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.IsTrue(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.ReadonlyThree.IsReadOnly);

            TestSource.Enabled = false;

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);
            Assert.IsTrue(ControlTree.ReadonlyThree.IsReadOnly);

            TestSource.Visible = false;

            Assert.AreEqual(Visibility.Collapsed, ControlTree.Three.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);
            Assert.IsTrue(ControlTree.ReadonlyThree.IsReadOnly);
        }

        [Test]
        public void Feature_Bindings_shall_requery_if_bound_contextid_is_changed_at_the_ancestor()
        {
            var Rules = new Dictionary<string, Feature>
            {
                {"One/BoundTwo/Three", new Feature(false, false)},
                {"One/AlteredTwo/Three", new Feature(true, true)},
                {"One/BoundTwo/TemplatedThree", new Feature(false, false)},
                {"One/AlteredTwo/TemplatedThree", new Feature(true, true)}
            };
            var ContextValues = new ContextValues("BoundTwo");

            var TestSource = new TestFeatureSource(Rules);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            ControlTree.DataContext = ContextValues;
            ControlTree.Two.SetBinding(UiFeatureManagement.ContextProperty, "Context");
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Collapsed, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Collapsed, ControlTree.TemplatedThree.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);

            ContextValues.Context = "AlteredTwo";

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Visible, ControlTree.TemplatedThree.Visibility);
            Assert.IsTrue(ControlTree.Three.IsEnabled);
            Assert.IsTrue(ControlTree.TemplatedThree.IsEnabled);
        }

        [Test]
        public void Feature_Bindings_shall_requery_if_bound_contextid_is_changed_at_the_element()
        {
            var Rules = new Dictionary<string, Feature>
            {
                {"One/Two/BoundThree", new Feature(false, false)},
                {"One/Two/AlteredThree", new Feature(true, true)}
            };
            var ContextValues = new ContextValues("BoundThree");

            var TestSource = new TestFeatureSource(Rules);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            ControlTree.DataContext = ContextValues;
            ControlTree.Three.SetBinding(UiFeatureManagement.ContextProperty, "Context");
            ControlTree.TemplatedThree.DataContext = ContextValues;
            ControlTree.TemplatedThree.SetBinding(UiFeatureManagement.ContextProperty, "Context");
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Collapsed, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Collapsed, ControlTree.TemplatedThree.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);

            ContextValues.Context = "AlteredThree";

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Visible, ControlTree.TemplatedThree.Visibility);
            Assert.IsTrue(ControlTree.Three.IsEnabled);
            Assert.IsTrue(ControlTree.TemplatedThree.IsEnabled);
        }

        [Test]
        public void Feature_Bindings_shall_requery_if_contextid_is_changed_at_the_ancestor()
        {
            var Rules = new Dictionary<string, Feature>
            {
                {"One/Two/Three", new Feature(false, false)},
                {"One/AlteredTwo/Three", new Feature(true, true)},
                {"One/Two/TemplatedThree", new Feature(false, false)},
                {"One/AlteredTwo/TemplatedThree", new Feature(true, true)}
            };

            var TestSource = new TestFeatureSource(Rules);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Collapsed, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Collapsed, ControlTree.TemplatedThree.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);

            UiFeatureManagement.SetContext(ControlTree.Two, "AlteredTwo");

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Visible, ControlTree.TemplatedThree.Visibility);
            Assert.IsTrue(ControlTree.Three.IsEnabled);
            Assert.IsTrue(ControlTree.TemplatedThree.IsEnabled);
        }

        [Test]
        public void Feature_Bindings_shall_requery_if_contextid_is_changed_at_the_element()
        {
            var Rules = new Dictionary<string, Feature>
            {
                {"One/Two/Three", new Feature(false, false)},
                {"One/Two/AlteredThree", new Feature(true, true)},
                {"One/Two/TemplatedThree", new Feature(false, false)},
                {"One/Two/AlteredTemplatedThree", new Feature(true, true)}
            };

            var TestSource = new TestFeatureSource(Rules);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Collapsed, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Collapsed, ControlTree.TemplatedThree.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);

            UiFeatureManagement.SetContext(ControlTree.Three, "AlteredThree");
            UiFeatureManagement.SetContext(ControlTree.TemplatedThree, "AlteredTemplatedThree");

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Visible, ControlTree.TemplatedThree.Visibility);
            Assert.IsTrue(ControlTree.Three.IsEnabled);
        }

        [Test]
        public void Feature_Bindings_shall_requery_if_contextid_is_changed_at_the_root_ancestor()
        {
            var Rules = new Dictionary<string, Feature>
            {
                {"One/Two/Three", new Feature(false, false)},
                {"AlteredOne/Two/Three", new Feature(true, true)},
                {"One/Two/TemplatedThree", new Feature(false, false)},
                {"AlteredOne/Two/TemplatedThree", new Feature(true, true)}
            };

            var TestSource = new TestFeatureSource(Rules);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Collapsed, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Collapsed, ControlTree.TemplatedThree.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);

            UiFeatureManagement.SetContext(ControlTree.One, "AlteredOne");

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Visible, ControlTree.TemplatedThree.Visibility);
            Assert.IsTrue(ControlTree.Three.IsEnabled);
            Assert.IsTrue(ControlTree.TemplatedThree.IsEnabled);
        }

        [Test]
        public void
            Feature_Bindings_shall_requery_if_contextid_is_changed_at_the_root_ancestor_with_one_level_without_context()
        {
            var Rules = new Dictionary<string, Feature>
            {
                {"One/Three", new Feature(false, false)},
                {"AlteredOne/Three", new Feature(true, true)},
                {"One/TemplatedThree", new Feature(false, false)},
                {"AlteredOne/TemplatedThree", new Feature(true, true)}
            };

            var TestSource = new TestFeatureSource(Rules);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            ControlTree.Two.ClearValue(UiFeatureManagement.ContextProperty);
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Collapsed, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Collapsed, ControlTree.TemplatedThree.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);

            UiFeatureManagement.SetContext(ControlTree.One, "AlteredOne");

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Visible, ControlTree.TemplatedThree.Visibility);
            Assert.IsTrue(ControlTree.Three.IsEnabled);
            Assert.IsTrue(ControlTree.TemplatedThree.IsEnabled);
        }

        [Test]
        public void Feature_Bindings_shall_requery_if_manager_is_changed()
        {
            var Rules = new Dictionary<string, Feature>
            {
                {"One/Two/Three", new Feature(false, false)},
                {"One/Two/TemplatedThree", new Feature(false, false)}
            };

            var NewRules = new Dictionary<string, Feature>
            {
                {"One/Two/Three", new Feature(true, true)},
                {"One/Two/TemplatedThree", new Feature(true, true)}
            };

            var TestSource = new TestFeatureSource(Rules);
            var NewTestSource = new TestFeatureSource(NewRules);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});
            IUiFeatureManager NewFeatureManager = new UiFeatureManager(new[] {NewTestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Collapsed, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Collapsed, ControlTree.TemplatedThree.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);

            UiFeatureManagement.SetManager(ControlTree, NewFeatureManager);

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Visible, ControlTree.TemplatedThree.Visibility);
            Assert.IsTrue(ControlTree.Three.IsEnabled);
            Assert.IsTrue(ControlTree.TemplatedThree.IsEnabled);
        }

        [Test]
        public void Feature_Bindings_shall_requery_if_manager_sends_featurechanged_event()
        {
            var Rules = new Dictionary<string, Feature>
            {
                {"One/Two/Three", new Feature(false, false)},
                {"One/Two/TemplatedThree", new Feature(false, false)},
                {"One/Two/Hyperlink", new Feature(false, false)}
            };

            var NewRules = new Dictionary<string, Feature>
            {
                {"One/Two/Three", new Feature(true, true)},
                {"One/Two/TemplatedThree", new Feature(true, true)},
                {"One/Two/Hyperlink", new Feature(true, true)}
            };

            var TestSource = new TestFeatureSource(Rules);
            var NewTestSource = new TestFeatureSource(NewRules);
            IUiFeatureManager FeatureManager = new UiFeatureManager(new[] {TestSource});
            IUiFeatureManager NewFeatureManager = new UiFeatureManager(new[] {NewTestSource});

            var ControlTree = new UiFeatureManagementControlTree();
            UiFeatureManagement.SetManager(ControlTree, FeatureManager);

            Assert.AreEqual(Visibility.Collapsed, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Collapsed, ControlTree.TemplatedThree.Visibility);
            Assert.IsFalse(ControlTree.Three.IsEnabled);
            Assert.IsFalse(ControlTree.TemplatedThree.IsEnabled);
            Assert.IsFalse(ControlTree.Hyperlink.IsEnabled);

            UiFeatureManagement.SetManager(ControlTree, NewFeatureManager);

            Assert.AreEqual(Visibility.Visible, ControlTree.Three.Visibility);
            Assert.AreEqual(Visibility.Visible, ControlTree.TemplatedThree.Visibility);
            Assert.IsTrue(ControlTree.Three.IsEnabled);
            Assert.IsTrue(ControlTree.TemplatedThree.IsEnabled);
            Assert.IsTrue(ControlTree.Hyperlink.IsEnabled);
        }
    }

    public class ContextValues : ObservableObject
    {
        private string context;

        public ContextValues(string context)
        {
            this.context = context;
        }

        public string Context
        {
            get => context;
            set => SetAndInvoke(nameof(Context), ref context, value);
        }
    }
}