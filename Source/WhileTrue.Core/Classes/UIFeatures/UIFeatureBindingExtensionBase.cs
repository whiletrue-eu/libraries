using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using WhileTrue.Classes.Framework;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    public abstract class UIFeatureBindingExtensionBase : MarkupExtension 
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget Target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if (Target.TargetObject is DependencyObject)
            {
                ValueWrapperBase Value = CreateValueWrapper((DependencyObject) Target.TargetObject);

                Binding VisibilityBinding = new Binding("Value") {Mode = BindingMode.OneWay, Source = Value};
                return VisibilityBinding.ProvideValue(serviceProvider);
            }
            else
            {
                return this;
            }
        }

        protected abstract ValueWrapperBase CreateValueWrapper(DependencyObject target);

        protected abstract class ValueWrapperBase : ObservableObject
        {
            protected string context;
            protected IUIFeatureManager manager;

            protected ValueWrapperBase(DependencyObject target)
            {
                UIFeatureManagement.AddContextPathChangedEventHandler(target, ContextChanged);
                UIFeatureManagement.AddManagerChangedEventHandler(target, ManagerChanged);

                this.context = UIFeatureManagement.GetContextPath(target);

                this.UpdateManager(UIFeatureManagement.GetManager(target));
            }

            private void UpdateManager(IUIFeatureManager manager)
            {
                if (this.manager != null)
                {
                    this.manager.FeaturesChanged -= FeaturesChanged;
                }
                this.manager = manager;
                if (this.manager != null)
                {
                    this.manager.FeaturesChanged += FeaturesChanged;
                }
                this.InvokePropertyChanged(() => Value);
            }

            private void ManagerChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                this.UpdateManager((IUIFeatureManager)e.NewValue);
            }

            private void FeaturesChanged(object sender, EventArgs e)
            {
                this.InvokePropertyChanged(() => Value);
            }

            private void ContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                this.context = (string)e.NewValue;
                this.InvokePropertyChanged(() => Value);
            }

            // ReSharper disable UnusedMember.Global
            // ReSharper disable MemberCanBeProtected.Global
            public abstract object Value { get; }   // Propery is used in binding expression
            // ReSharper restore MemberCanBeProtected.Global
            // ReSharper restore UnusedMember.Global
        }

    }
}