// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System;
using NUnit.Framework;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Components.UIFeatures
{
    [TestFixture]
    public class UIFeatureManagerTest
    {
        [Test]
        public void UIFeatureManager_shall_return_the_results_of_underlying_source()
        {
            TestFeatureSource TestFeatureSource = new TestFeatureSource(true,true);
            UiFeatureManager UIFeatureManager = new UiFeatureManager(new[] { TestFeatureSource });
            
            bool Enabled = UIFeatureManager.IsEnabled("ContextString");

            Assert.IsTrue(Enabled);
            Assert.AreEqual("ContextString", TestFeatureSource.EnabledContext);

            bool Visible = UIFeatureManager.IsVisible("OtherContextString");

            Assert.IsTrue(Visible);
            Assert.AreEqual("OtherContextString", TestFeatureSource.VisibleContext);
        }

        [Test]
        public void Invisible_features_shall_also_not_be_enabled()
        {
            TestFeatureSource TestFeatureSource = new TestFeatureSource(false, true);
            UiFeatureManager UIFeatureManager = new UiFeatureManager(new[] { TestFeatureSource });

            bool Enabled = UIFeatureManager.IsEnabled("ContextString");
            bool Visible = UIFeatureManager.IsVisible("OtherContextString");

            Assert.IsFalse(Enabled);
            Assert.IsFalse(Visible);
        }

        [Test]
        public void FeatureChanged_event_shall_be_forwarded()
        {
            TestFeatureSource TestFeatureSource = new TestFeatureSource(false, true);
            UiFeatureManager UIFeatureManager = new UiFeatureManager(new[] { TestFeatureSource });

            bool EventCalled = false;
            UIFeatureManager.FeaturesChanged += delegate { EventCalled = true; };

            TestFeatureSource.InvokeFeaturesChanged();

            Assert.IsTrue(EventCalled);
        }

        [Test]
        public void Feature_shall_be_visible_if_at_least_one_source_returns_visible()
        {
            TestFeatureSource TestFeatureSource1 = new TestFeatureSource(true, true);
            TestFeatureSource TestFeatureSource2 = new TestFeatureSource(false, true);
            UiFeatureManager UIFeatureManager = new UiFeatureManager(new[] { TestFeatureSource1, TestFeatureSource2 });

            bool Visible = UIFeatureManager.IsVisible("");

            Assert.IsTrue(Visible);
        }

        [Test]
        public void Feature_shall_be_invisible_if_all_sources_return_invisible()
        {
            TestFeatureSource TestFeatureSource1 = new TestFeatureSource(false, true);
            TestFeatureSource TestFeatureSource2 = new TestFeatureSource(false, true);
            UiFeatureManager UIFeatureManager = new UiFeatureManager(new[] { TestFeatureSource1, TestFeatureSource2 });

            bool Visible = UIFeatureManager.IsVisible("");

            Assert.IsFalse(Visible);
        }

        [Test]
        public void Feature_shall_be_enabled_if_all_sources_return_enabled()
        {
            TestFeatureSource TestFeatureSource1 = new TestFeatureSource(true, true);
            TestFeatureSource TestFeatureSource2 = new TestFeatureSource(true, true);
            UiFeatureManager UIFeatureManager = new UiFeatureManager(new[] { TestFeatureSource1, TestFeatureSource2 });

            bool Enabled = UIFeatureManager.IsEnabled("");

            Assert.IsTrue(Enabled);
        }

        [Test]
        public void Feature_shall_be_disabled_if_at_least_one_source_returns_disabled()
        {
            TestFeatureSource TestFeatureSource1 = new TestFeatureSource(true, false);
            TestFeatureSource TestFeatureSource2 = new TestFeatureSource(true, false);
            UiFeatureManager UIFeatureManager = new UiFeatureManager(new[] { TestFeatureSource1, TestFeatureSource2 });

            bool Enabled = UIFeatureManager.IsEnabled("");

            Assert.IsFalse(Enabled);
        }

        private class TestFeatureSource : IUiFeatureManagerSource
        {
            private readonly bool visible;
            private readonly bool enabled;

            public string EnabledContext { get; private set; }

            public string VisibleContext { get; private set; }

            public TestFeatureSource(bool visible, bool enabled)
            {
                this.visible = visible;
                this.enabled = enabled;
            }

            public bool IsVisible(string context)
            {
                this.VisibleContext = context;
                return this.visible;
            }

            public bool IsEnabled(string context)
            {
                this.EnabledContext = context;
                return this.enabled;
            }

            public void InvokeFeaturesChanged()
            {
                this.FeaturesChanged(this, EventArgs.Empty);
            }

            public event EventHandler<EventArgs> FeaturesChanged;
        }
    }
}