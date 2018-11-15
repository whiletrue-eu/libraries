#pragma warning disable 1591
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.SettingStorage;

namespace WhileTrue.Classes.Wpf
{
    public class PersistentPropertyExtension : MarkupExtension
    {
        public PersistentPropertyExtension()
        {
        }

        public PersistentPropertyExtension(string name, object defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        [ConstructorArgument("name")] private string Name { get; }

        [ConstructorArgument("defaultValue")] private object DefaultValue { get; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var Target = (IProvideValueTarget) serviceProvider.GetService(typeof(IProvideValueTarget));

            if (Target.TargetObject is DependencyObject)
            {
                var TargetObject = (DependencyObject) Target.TargetObject;

                var PropertyStore = TryGetPropertyStoreFromResources(TargetObject);

                var StoreAdapter = PropertyStoreAdapter.GetInstanceFor(PropertyStore);

                var Value = new SettingValue(StoreAdapter, Name, DefaultValue, TargetObject);

                var SettingBinding = new Binding("Value");
                SettingBinding.Mode = BindingMode.TwoWay;
                SettingBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                SettingBinding.Source = Value;

                return SettingBinding.ProvideValue(serviceProvider);
            }

            return this;
        }

        private static ITagValueSettingStore TryGetPropertyStoreFromResources(DependencyObject targetObject)
        {
            ITagValueSettingStore PropertyStore = null;
            if (targetObject is FrameworkElement)
                PropertyStore =
                    ((FrameworkElement) targetObject).TryFindResource("PersistentProperties") as ITagValueSettingStore;
            else if (targetObject is FrameworkContentElement)
                PropertyStore =
                    ((FrameworkContentElement) targetObject).TryFindResource("PersistentProperties") as
                    ITagValueSettingStore;
            return PropertyStore;
        }


        private class SettingValue : ObservableObject
        {
            private readonly object defaultValue;
            private readonly string name;
            private readonly PropertyStoreAdapter propertyStore;
            private string context;
            private PropertyAdapter value;

            public SettingValue(PropertyStoreAdapter propertyStore, string name, object defaultValue,
                DependencyObject target)
            {
                this.propertyStore = propertyStore;
                this.name = name;
                this.defaultValue = defaultValue;
                context = PersistentProperty.GetIdPath(target);
                PersistentProperty.AddIDPathChangedEventHandler(target, ContextChanged);
                value = GetProperty();
                value.PropertyChanged += ValueChanged;
            }

            // Propery is used in binding expression
            // ReSharper disable UnusedMemberInPrivateClass
            // ReSharper disable MemberCanBePrivate.Local
            // ReSharper disable UnusedMember.Local
            public object Value
            {
                get => value.Value;
                set
                {
                    this.value.Value = value;
                    InvokePropertyChanged(nameof(Value));
                }
            }
            // ReSharper restore UnusedMember.Local
            // ReSharper restore MemberCanBePrivate.Local
            // ReSharper restore UnusedMemberInPrivateClass

            private void SetValue(PropertyAdapter value)
            {
                this.value.PropertyChanged -= ValueChanged;
                this.value = value;
                this.value.PropertyChanged += ValueChanged;
            }

            private void ValueChanged(object sender, PropertyChangedEventArgs e)
            {
                InvokePropertyChanged(nameof(Value));
            }

            private void ContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                context = PersistentProperty.GetIdPath((DependencyObject) sender);
                SetValue(GetProperty());
                InvokePropertyChanged(nameof(Value));
            }

            private PropertyAdapter GetProperty()
            {
                var PropertyName = GetPropertyName();
                return propertyStore.GetProperty(PropertyName, defaultValue);
            }

            private string GetPropertyName()
            {
                return $"{context ?? ""}{(context != null ? "." : "")}{name}";
            }
        }
    }
}