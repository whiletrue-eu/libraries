using System.Windows;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    /// <summary>
    /// Connects an <c>ReadOnly</c> property to the information provided by the associated <see cref="IUIFeatureManager"/>
    /// </summary>
    public sealed class FeatureReadOnlyExtension : UIFeatureBindingExtensionBase
    {
        protected override ValueWrapperBase CreateValueWrapper(DependencyObject target)
        {
            return new ReadonlyValueWrapper(target);
        }

        private sealed class ReadonlyValueWrapper : UIFeatureBindingExtensionBase.ValueWrapperBase
        {
            public ReadonlyValueWrapper(DependencyObject target)
                : base(target)
            {
            }

            public override object Value
            {
                get
                {
                    if (this.manager != null)
                    {
                        return this.manager.IsEnabled(this.context) == false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}