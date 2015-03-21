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
            IPropertyAdapterFactory<ModelPropertyNodeAdapter> Factory = ObservableObject.GetPropertyAdapterFactory<ModelPropertyNodeAdapter>();
            ModelPropertyNodeAdapter.internalValueAdapter = Factory.Create(
                nameof(ModelPropertyNodeAdapter.InternalValue),
                instance => ModelNodeBaseAdapter.GetAdapter(instance, instance.propertyNode.Value)
                );
            ModelPropertyNodeAdapter.modelAdapter = Factory.Create(
                nameof(ModelPropertyNodeAdapter.Model),
                instance => instance.InternalValue.Model
                );
            ModelPropertyNodeAdapter.nameAdapter = Factory.Create(
                nameof(ModelPropertyNodeAdapter.Name),
                instance => instance.propertyNode.Name
                );
            ModelPropertyNodeAdapter.typenameAdapter = Factory.Create(
                nameof(ModelPropertyNodeAdapter.Typename),
                instance => instance.InternalValue.Typename
                );
            ModelPropertyNodeAdapter.valueAdapter = Factory.Create(
                nameof(ModelPropertyNodeAdapter.Value),
                instance => instance.InternalValue.Value
                );
            ModelPropertyNodeAdapter.hasValueAdapter = Factory.Create(
                nameof(ModelPropertyNodeAdapter.HasValue),
                instance => instance.InternalValue.HasValue
                );
            ModelPropertyNodeAdapter.supportsValidationAdapter = Factory.Create(
                nameof(ModelPropertyNodeAdapter.SupportsValidation),
                instance => instance.propertyNode.SupportsValidation
                );
            ModelPropertyNodeAdapter.validationSeverityAdapter = Factory.Create(
                nameof(ModelPropertyNodeAdapter.ValidationSeverity),
                instance => instance.propertyNode.ValidationSeverity
                );
            ModelPropertyNodeAdapter.validationResultsAdapter = Factory.Create(
                nameof(ModelPropertyNodeAdapter.ValidationResults),
                instance => instance.propertyNode.ValidationResults,
                (instance,message)=>message
                );
            ModelPropertyNodeAdapter.childrenAdapter = Factory.Create(
                nameof(ModelPropertyNodeAdapter.Children),
                instance => instance.InternalValue.Children
                );      
        }

        public ModelPropertyNodeAdapter(IModelNodeAdapterParent parent, IPropertyNode propertyNode):base(parent)
        {
            this.propertyNode = propertyNode;
        }

        private ModelNodeBaseAdapter InternalValue => ModelPropertyNodeAdapter.internalValueAdapter.GetValue(this);

        public override string Name => ModelPropertyNodeAdapter.nameAdapter.GetValue(this);

        public override string Path => $"{(this.Parent == null ? "" : $"{this.Parent.Path}.")}{this.Name}";

        public override string Typename => ModelPropertyNodeAdapter.typenameAdapter.GetValue(this);

        public override object Value => ModelPropertyNodeAdapter.valueAdapter.GetValue(this);

        public override bool HasValue => ModelPropertyNodeAdapter.hasValueAdapter.GetValue(this);

        public override bool SupportsPropertyChanged => true;

        public override bool SupportsValidation => ModelPropertyNodeAdapter.supportsValidationAdapter.GetValue(this);

        public override ValidationSeverity ValidationSeverity => ModelPropertyNodeAdapter.validationSeverityAdapter.GetValue(this);

        public override IEnumerable<ValidationMessage> ValidationResults => ModelPropertyNodeAdapter.validationResultsAdapter.GetCollection(this);


        public override IEnumerable<ModelNodeBaseAdapter> Children => ModelPropertyNodeAdapter.childrenAdapter.GetValue(this);

        internal override IModelNodeBase Model => ModelPropertyNodeAdapter.modelAdapter.GetValue(this);
    }
}