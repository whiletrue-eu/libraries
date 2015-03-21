using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Modules.ModelInspector
{
    internal class PropertyNode : ObservableObject, IPropertyNode
    {
        private static readonly ObjectCache<ObjectCacheKey<INotifyPropertyChanged, PropertyInfo>, INotifyPropertyChanged, PropertyInfo, PropertyNode> nodeCache = new ObjectCache<ObjectCacheKey<INotifyPropertyChanged, PropertyInfo>, INotifyPropertyChanged, PropertyInfo, PropertyNode>((key, value, index) => PropertyNode.CreateNode(value, index));

        internal static PropertyNode GetNode(INotifyPropertyChanged value, PropertyInfo info)
        {
            return PropertyNode.nodeCache.GetObject(new ObjectCacheKey<INotifyPropertyChanged,PropertyInfo>(value,info), value, info); //objectcachekey also supports null value
        }

        private static PropertyNode CreateNode(INotifyPropertyChanged value, PropertyInfo info)
        {
            return new PropertyNode(value, info);
        }
        
        
        private readonly INotifyPropertyChanged owner;
        private readonly PropertyInfo propertyInfo;
        private ModelNodeBase value;
        private readonly ReadOnlyPropertyAdapter<ValidationSeverity> validationSeverityAdapter;

        private PropertyNode(INotifyPropertyChanged owner, PropertyInfo propertyInfo)
        {
            this.owner = owner;
            this.propertyInfo = propertyInfo;
            this.owner.PropertyChanged += WeakDelegate.Connect<PropertyNode,INotifyPropertyChanged,PropertyChangedEventHandler,PropertyChangedEventArgs>(
                this, this.owner, (target, sender, eventargs) => target.OnPropertyChanged(sender, eventargs), (source, handler) => source.PropertyChanged -= handler);
            if (this.owner is IObjectValidation)
            {
                ((IObjectValidation)this.owner).ValidationChanged += WeakDelegate.Connect<PropertyNode, IObjectValidation, ValidationEventArgs>(
                    this, (IObjectValidation) this.owner, (target, sender, eventargs) => target.OnValidationChanged(eventargs), (source, handler) => source.ValidationChanged -= handler);
            }
            this.UpdateNode();

            this.validationSeverityAdapter = this.CreatePropertyAdapter(
                nameof(this.ValidationSeverity),
                ()=>this.ValidationResults.DefaultIfEmpty(new ValidationMessage(ValidationSeverity.None, "")).Max(message=>message.Severity)
                );
        }

        private void OnValidationChanged(ValidationEventArgs eventargs)
        {
            if (eventargs.PropertyName == this.propertyInfo.Name)
            {
                this.InvokePropertyChanged(nameof(this.ValidationResults));
            }

        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs eventargs)
        {
            if (eventargs.PropertyName == this.propertyInfo.Name)
            {
                this.UpdateNode();
            }
        }

        private void UpdateNode()
        {
            if (this.propertyInfo.GetIndexParameters().Any())
            {
                //Ignore indexer for the moment
                this.SetAndInvoke(nameof(this.Value),ref this.value, null);
            }
            else
            {
                object Value;
                try
                {
                    Value = this.propertyInfo.GetValue(this.owner, null);
                }
                catch (Exception Exception)
                {
                    Value = Exception;
                }
                this.SetAndInvoke(nameof(this.Value),ref this.value, ModelNodeBase.GetNode(Value));
            }
        }

        public IModelNodeBase Value => this.value;

        public string Name => this.propertyInfo.Name;

        public bool SupportsValidation => this.owner is IObjectValidation;

        public ValidationSeverity ValidationSeverity => this.validationSeverityAdapter.GetValue();

        public IEnumerable<ValidationMessage> ValidationResults
        {
            get
            {
                if (this.owner is IObjectValidation)
                {
                    return ((IObjectValidation) this.owner).GetValidationMessages(this.propertyInfo.Name);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}