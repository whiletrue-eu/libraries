using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;
using WhileTrue.Classes.Framework;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    /// <summary>
    ///     Base class for the binding extensions that allow to bind to values of the permission manager
    /// </summary>
    [PublicAPI]
    public abstract class UiFeatureBindingExtensionBase : MarkupExtension
    {
        /// <summary>
        ///     When implemented in a derived class, returns an object that is provided as the value of the target property for
        ///     this markup extension.
        /// </summary>
        /// <returns>
        ///     The object value to set on the property where the extension is applied.
        /// </returns>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var Target = (IProvideValueTarget) serviceProvider.GetService(typeof(IProvideValueTarget));

            if (Target.TargetObject is DependencyObject)
            {
                var Value = CreateValueWrapper((DependencyObject) Target.TargetObject);

                var VisibilityBinding = new Binding("Value") {Mode = BindingMode.OneWay, Source = Value};
                return VisibilityBinding.ProvideValue(serviceProvider);
            }

            return this;
        }

        /// <summary>
        ///     Creates a wrapper object that allows binding against the value from the permission manager
        /// </summary>
        protected abstract ValueWrapperBase CreateValueWrapper(DependencyObject target);

        /// <summary>
        ///     base class for the wrappr object
        /// </summary>
        protected abstract class ValueWrapperBase : ObservableObject
        {
            /// <summary>
            ///     Context string for the UI element
            /// </summary>
            protected string Context;

            /// <summary>
            ///     the permission manager
            /// </summary>
            protected IUiFeatureManager Manager;

            /// <summary />
            protected ValueWrapperBase(DependencyObject target)
            {
                UiFeatureManagement.AddContextPathChangedEventHandler(target, ContextChanged);
                UiFeatureManagement.AddManagerChangedEventHandler(target, ManagerChanged);

                Context = UiFeatureManagement.GetContextPath(target);

                UpdateManager(UiFeatureManagement.GetManager(target));
            }

            // ReSharper disable MemberCanBeProtected.Global
            /// <summary>
            ///     returns the value of the permission manager
            /// </summary>
            public abstract object Value { get; } // Propery is used in binding expression
            // ReSharper restore MemberCanBeProtected.Global

            private void UpdateManager(IUiFeatureManager manager)
            {
                if (Manager != null) Manager.FeaturesChanged -= FeaturesChanged;
                Manager = manager;
                if (Manager != null) Manager.FeaturesChanged += FeaturesChanged;
                InvokePropertyChanged(nameof(Value));
            }

            private void ManagerChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                UpdateManager((IUiFeatureManager) e.NewValue);
            }

            private void FeaturesChanged(object sender, EventArgs e)
            {
                InvokePropertyChanged(nameof(Value));
            }

            private void ContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                Context = (string) e.NewValue;
                InvokePropertyChanged(nameof(Value));
            }
        }
    }
}