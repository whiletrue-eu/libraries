using System.Windows;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    /// <summary>
    /// Connects an <c>IsEnabled</c> property to the information provided by the associated <see cref="IUIFeatureManager"/>
    /// </summary>
    public sealed class FeatureEnabledExtension : UIFeatureBindingExtensionBase
    {
        protected override ValueWrapperBase CreateValueWrapper(DependencyObject target)
        {
            return new EnabledValueWrapper(target);
        }

        private sealed class EnabledValueWrapper : UIFeatureBindingExtensionBase.ValueWrapperBase
        {
            public EnabledValueWrapper(DependencyObject target)
                : base(target)
            {
            }

            public override object Value
            {
                get { return this.manager == null || this.manager.IsEnabled(this.context); }
            }
        }
    }
}