using System.Windows;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    /// <summary>
    /// Connects an <c>IsVisible</c> property to the information provided by the associated <see cref="IUIFeatureManager"/>
    /// </summary>
    public sealed class FeatureVisibleExtension : UIFeatureBindingExtensionBase
    {
        protected override ValueWrapperBase CreateValueWrapper(DependencyObject target)
        {
            return new VisibleValueWrapper(target);
        }

        private sealed class VisibleValueWrapper : UIFeatureBindingExtensionBase.ValueWrapperBase
        {
            public VisibleValueWrapper(DependencyObject target)
                : base(target)
            {
            }

            public override object Value
            {
                get { return this.manager == null || this.manager.IsVisible(this.context) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
            }
        }
    }
}