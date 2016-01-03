using System.Windows;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    /// <summary>
    /// Connects an <c>IsEnabled</c> property to the information provided by the associated <see cref="IUiFeatureManager"/>
    /// </summary>
    public sealed class FeatureEnabledExtension : UiFeatureBindingExtensionBase
    {
        /// <summary>
        /// Creates a wrapper for the ui enabled state from the permission manager, that allows binding it to the control
        /// </summary>
        protected override ValueWrapperBase CreateValueWrapper(DependencyObject target)
        {
            return new EnabledValueWrapper(target);
        }

        private sealed class EnabledValueWrapper : UiFeatureBindingExtensionBase.ValueWrapperBase
        {
            public EnabledValueWrapper(DependencyObject target)
                : base(target)
            {
            }

            public override object Value => this.Manager == null || this.Manager.IsEnabled(this.Context);
        }
    }
}