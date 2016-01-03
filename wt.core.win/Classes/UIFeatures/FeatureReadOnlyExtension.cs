using System.Windows;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    /// <summary>
    /// Connects an <c>ReadOnly</c> property to the information provided by the associated <see cref="IUiFeatureManager"/>
    /// </summary>
    public sealed class FeatureReadOnlyExtension : UiFeatureBindingExtensionBase
    {
        /// <summary>
        /// Creates a wrapper for the ui readonly state from the permission manager, that allows binding it to the control
        /// </summary>
        protected override ValueWrapperBase CreateValueWrapper(DependencyObject target)
        {
            return new ReadonlyValueWrapper(target);
        }

        private sealed class ReadonlyValueWrapper : UiFeatureBindingExtensionBase.ValueWrapperBase
        {
            public ReadonlyValueWrapper(DependencyObject target)
                : base(target)
            {
            }

            public override object Value => (this.Manager == null || this.Manager.IsEnabled(this.Context))==false;
        }
    }
}