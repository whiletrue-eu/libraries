﻿using System.Windows;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    /// <summary>
    ///     Connects an <c>IsVisible</c> property to the information provided by the associated
    ///     <see cref="IUiFeatureManager" />
    /// </summary>
    public sealed class FeatureVisibleExtension : UiFeatureBindingExtensionBase
    {
        /// <summary>
        ///     Creates a wrapper object that allows binding against the value from the permission manager
        /// </summary>
        protected override ValueWrapperBase CreateValueWrapper(DependencyObject target)
        {
            return new VisibleValueWrapper(target);
        }

        private sealed class VisibleValueWrapper : ValueWrapperBase
        {
            public VisibleValueWrapper(DependencyObject target)
                : base(target)
            {
            }

            public override object Value => Manager == null || Manager.IsVisible(Context)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}