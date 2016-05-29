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
    public  class PersistentPropertyExtension : MarkupExtension
    {
        public PersistentPropertyExtension()
        {
        }

        public PersistentPropertyExtension(string name, object defaultValue)
        {
            this.Name = name;
            this.DefaultValue = defaultValue;
        }

        [ConstructorArgument("name")]
        private string Name { get; }
        [ConstructorArgument("defaultValue")]
        private object DefaultValue { get; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget Target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if (Target.TargetObject is DependencyObject)
            {
                DependencyObject TargetObject = (DependencyObject) Target.TargetObject;

                ITagValueSettingStore PropertyStore = PersistentPropertyExtension.TryGetPropertyStoreFromResources(TargetObject);

                PropertyStoreAdapter StoreAdapter = PropertyStoreAdapter.GetInstanceFor(PropertyStore);

                SettingValue Value = new SettingValue(StoreAdapter, this.Name, this.DefaultValue, TargetObject);

                Binding SettingBinding = new Binding("Value");
                SettingBinding.Mode = BindingMode.TwoWay;
                SettingBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                SettingBinding.Source = Value;

                return SettingBinding.ProvideValue(serviceProvider);
            }
            else
            {
                return this;
            }
        }

        private static ITagValueSettingStore TryGetPropertyStoreFromResources(DependencyObject targetObject)
        {
            ITagValueSettingStore PropertyStore=null; 
            if( targetObject is FrameworkElement )
            {
                PropertyStore = ((FrameworkElement) targetObject).TryFindResource("PersistentProperties") as ITagValueSettingStore;
            }
            else if(targetObject is FrameworkContentElement)
            {
                PropertyStore = ((FrameworkContentElement)targetObject).TryFindResource("PersistentProperties") as ITagValueSettingStore;
            }
            return PropertyStore;
        }


        private class SettingValue : ObservableObject
        {
            private readonly PropertyStoreAdapter propertyStore;
            private readonly string name;
            private readonly object defaultValue;
            private PropertyAdapter value;
            private string context;

            public SettingValue( PropertyStoreAdapter propertyStore,  string name, object defaultValue, DependencyObject target)
            {
                this.propertyStore = propertyStore;
                this.name = name;
                this.defaultValue = defaultValue;
                this.context = PersistentProperty.GetIdPath(target);
                PersistentProperty.AddIDPathChangedEventHandler(target, this.ContextChanged);
                this.value = this.GetProperty();
                this.value.PropertyChanged += this.ValueChanged;
            }

            private void SetValue(PropertyAdapter value)
            {
                this.value.PropertyChanged -= this.ValueChanged;
                this.value = value;
                this.value.PropertyChanged += this.ValueChanged;
            }

            private void ValueChanged(object sender, PropertyChangedEventArgs e)
            {
                this.InvokePropertyChanged(nameof(SettingValue.Value));
            }

            // Propery is used in binding expression
            // ReSharper disable UnusedMemberInPrivateClass
            // ReSharper disable MemberCanBePrivate.Local
            // ReSharper disable UnusedMember.Local
            public object Value
            {
                get { return this.value.Value; }
                set
                {
                    this.value.Value = value;
                    this.InvokePropertyChanged(nameof(SettingValue.Value));
                }
            }
            // ReSharper restore UnusedMember.Local
            // ReSharper restore MemberCanBePrivate.Local
            // ReSharper restore UnusedMemberInPrivateClass

            private void ContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                this.context = PersistentProperty.GetIdPath((DependencyObject)sender);
                this.SetValue(this.GetProperty());
                this.InvokePropertyChanged(nameof(SettingValue.Value));
            }

            private PropertyAdapter GetProperty()
            {
                string PropertyName = this.GetPropertyName();
                return this.propertyStore.GetProperty(PropertyName, this.defaultValue);

            }

            private string GetPropertyName()
            {
                return $"{this.context ?? ""}{(this.context != null ? "." : "")}{this.name}";
            }
        }
    }
}
