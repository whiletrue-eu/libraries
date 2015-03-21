using System.Collections.Generic;
using WhileTrue.Classes.Framework;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{
    internal class ModelPropertyNodeAdapter : ModelNodeBaseAdapter
    {
        private readonly IPropertyNode propertyNode;
        private static readonly ReadOnlyPropertyAdapter<ModelPropertyNodeAdapter, ModelNodeBaseAdapter> internalValueAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelPropertyNodeAdapter, string> nameAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelPropertyNodeAdapter, string> typenameAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelPropertyNodeAdapter, object> valueAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelPropertyNodeAdapter, IEnumerable<ModelNodeBaseAdapter>> childrenAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelPropertyNodeAdapter, IModelNodeBase> modelAdapter;
        private static readonly EnumerablePropertyAdapter<ModelPropertyNodeAdapter, ValidationMessage, ValidationMessage> validationResultsAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelPropertyNodeAdapter, bool> hasValueAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelPropertyNodeAdapter, ValidationSeverity> validationSeverityAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelPropertyNodeAdapter, bool> supportsValidationAdapter;

        static ModelPropertyNodeAdapter()
        {
            IPropertyAdapterFactory<ModelPropertyNodeAdapter> Factory = GetPropertyAdapterFactory<ModelPropertyNodeAdapter>();
            internalValueAdapter = Factory.Create(
                instance => instance.InternalValue,
                instance => ModelNodeBaseAdapter.GetAdapter(instance, instance.propertyNode.Value)
                );
            modelAdapter = Factory.Create(
                instance => instance.Model,
                instance => instance.InternalValue.Model
                );
            nameAdapter = Factory.Create(
                instance => instance.Name,
                instance => instance.propertyNode.Name
                );
            typenameAdapter = Factory.Create(
                instance => instance.Typename,
                instance => instance.InternalValue.Typename
                );
            valueAdapter = Factory.Create(
                instance => instance.Value,
                instance => instance.InternalValue.Value
                );
            hasValueAdapter = Factory.Create(
                instance => instance.HasValue,
                instance => instance.InternalValue.HasValue
                );
            supportsValidationAdapter = Factory.Create(
                instance => instance.SupportsValidation,
                instance => instance.propertyNode.SupportsValidation
                );
            validationSeverityAdapter = Factory.Create(
                instance => instance.ValidationSeverity,
                instance => instance.propertyNode.ValidationSeverity
                );
            validationResultsAdapter = Factory.Create(
                instance => instance.ValidationResults,
                instance => instance.propertyNode.ValidationResults,
                (instance,message)=>message
                );
            childrenAdapter = Factory.Create(
                instance => instance.Children,
                instance => instance.InternalValue.Children
                );      
        }

        public ModelPropertyNodeAdapter(IModelNodeAdapterParent parent, IPropertyNode propertyNode):base(parent)
        {
            this.propertyNode = propertyNode;
        }

        private ModelNodeBaseAdapter InternalValue
        {
            get { return internalValueAdapter.GetValue(this); }
        }

        public override string Name
        {
            get { return nameAdapter.GetValue(this); }
        }

        public override string Path
        {
            get { return string.Format("{0}{1}", (this.Parent == null ? "" : string.Format("{0}.", this.Parent.Path)), this.Name); }
        }

        public override string Typename
        {
            get { return typenameAdapter.GetValue(this); }
        }

        public override object Value
        {
            get { return valueAdapter.GetValue(this); }
        }

        public override bool HasValue
        {
            get { return hasValueAdapter.GetValue(this); }
        }

        public override bool SupportsPropertyChanged
        {
            get { return true; }
        }

        public override bool SupportsValidation
        {
            get { return supportsValidationAdapter.GetValue(this); }
        }

        public override ValidationSeverity ValidationSeverity
        {
            get { return validationSeverityAdapter.GetValue(this); }
        }

        public override IEnumerable<ValidationMessage> ValidationResults
        {
            get { return validationResultsAdapter.GetCollection(this); }
        }


        public override IEnumerable<ModelNodeBaseAdapter> Children
        {
            get { return childrenAdapter.GetValue(this); }
        }

        internal override IModelNodeBase Model
        {
            get { return modelAdapter.GetValue(this); }
        }
    }
}