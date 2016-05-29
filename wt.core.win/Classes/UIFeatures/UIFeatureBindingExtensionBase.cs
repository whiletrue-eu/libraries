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
    /// Base class for the binding extensions that allow to bind to values of the permission manager
    /// </summary>
    [PublicAPI]
    public abstract class UiFeatureBindingExtensionBase : MarkupExtension 
    {
        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension. 
        /// </summary>
        /// <returns>
        /// The object value to set on the property where the extension is applied. 
        /// </returns>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget Target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if (Target.TargetObject is DependencyObject)
            {
                ValueWrapperBase Value = this.CreateValueWrapper((DependencyObject) Target.TargetObject);

                Binding VisibilityBinding = new Binding("Value") {Mode = BindingMode.OneWay, Source = Value};
                return VisibilityBinding.ProvideValue(serviceProvider);
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Creates a wrapper object that allows binding against the value from the permission manager
        /// </summary>
        protected abstract ValueWrapperBase CreateValueWrapper(DependencyObject target);

        /// <summary>
        /// base class for the wrappr object
        /// </summary>
        protected abstract class ValueWrapperBase : ObservableObject
        {
            /// <summary>
            /// Context string for the UI element
            /// </summary>
            protected string Context;
            /// <summary>
            /// the permission manager
            /// </summary>
            protected IUiFeatureManager Manager;

            /// <summary/>
            protected ValueWrapperBase(DependencyObject target)
            {
                UiFeatureManagement.AddContextPathChangedEventHandler(target, this.ContextChanged);
                UiFeatureManagement.AddManagerChangedEventHandler(target, this.ManagerChanged);

                this.Context = UiFeatureManagement.GetContextPath(target);

                this.UpdateManager(UiFeatureManagement.GetManager(target));
            }

            private void UpdateManager(IUiFeatureManager manager)
            {
                if (this.Manager != null)
                {
                    this.Manager.FeaturesChanged -= this.FeaturesChanged;
                }
                this.Manager = manager;
                if (this.Manager != null)
                {
                    this.Manager.FeaturesChanged += this.FeaturesChanged;
                }
                this.InvokePropertyChanged(nameof(this.Value));
            }

            private void ManagerChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                this.UpdateManager((IUiFeatureManager)e.NewValue);
            }

            private void FeaturesChanged(object sender, EventArgs e)
            {
                this.InvokePropertyChanged(nameof(this.Value));
            }

            private void ContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                this.Context = (string)e.NewValue;
                this.InvokePropertyChanged(nameof(ValueWrapperBase.Value));
            }

            // ReSharper disable MemberCanBeProtected.Global
            /// <summary>
            /// returns the value of the permission manager
            /// </summary>
            public abstract object Value { get; }   // Propery is used in binding expression
            // ReSharper restore MemberCanBeProtected.Global
        }

    }
}