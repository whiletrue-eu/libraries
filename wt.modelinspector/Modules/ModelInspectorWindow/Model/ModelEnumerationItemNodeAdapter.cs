using System.Collections.Generic;
using WhileTrue.Classes.Framework;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{
    internal class ModelEnumerationItemNodeAdapter : ModelNodeBaseAdapter
    {
        private readonly IEnumerationItemNode enumerationItem;
        private static readonly ReadOnlyPropertyAdapter<ModelEnumerationItemNodeAdapter, ModelNodeBaseAdapter> internalValueAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelEnumerationItemNodeAdapter, string> nameAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelEnumerationItemNodeAdapter, string> typenameAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelEnumerationItemNodeAdapter, object> valueAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelEnumerationItemNodeAdapter, IEnumerable<ModelNodeBaseAdapter>> childrenAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelEnumerationItemNodeAdapter, IModelNodeBase> modelAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelEnumerationItemNodeAdapter, bool> hasValueAdapter;

        static ModelEnumerationItemNodeAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<ModelEnumerationItemNodeAdapter> Factory = ObservableObject.GetPropertyAdapterFactory<ModelEnumerationItemNodeAdapter>();
            ModelEnumerationItemNodeAdapter.internalValueAdapter = Factory.Create(
                nameof(ModelEnumerationItemNodeAdapter.InternalValue),
                instance=>ModelNodeBaseAdapter.GetAdapter(instance, instance.enumerationItem.Value)
                );
            ModelEnumerationItemNodeAdapter.modelAdapter = Factory.Create(
                nameof(ModelEnumerationItemNodeAdapter.Model),
                instance => instance.InternalValue.Model
                );
            ModelEnumerationItemNodeAdapter.nameAdapter = Factory.Create(
                nameof(ModelEnumerationItemNodeAdapter.Name),
                instance => instance.enumerationItem.Name
                );
            ModelEnumerationItemNodeAdapter.typenameAdapter = Factory.Create(
                nameof(ModelEnumerationItemNodeAdapter.Typename),
                instance => instance.InternalValue.Typename
                );
            ModelEnumerationItemNodeAdapter.valueAdapter = Factory.Create(
                nameof(ModelEnumerationItemNodeAdapter.Value),
                instance => instance.InternalValue.Value
                );
            ModelEnumerationItemNodeAdapter.hasValueAdapter = Factory.Create(
                nameof(ModelEnumerationItemNodeAdapter.HasValue),
                instance => instance.InternalValue.HasValue
                );
            ModelEnumerationItemNodeAdapter.childrenAdapter = Factory.Create(
                nameof(ModelEnumerationItemNodeAdapter.Children),
                instance => instance.InternalValue.Children
                );        
        }

        public ModelEnumerationItemNodeAdapter(IModelNodeAdapterParent parent, IEnumerationItemNode enumerationItem):base(parent)
        {
            this.enumerationItem = enumerationItem;
        }

        private ModelNodeBaseAdapter InternalValue => ModelEnumerationItemNodeAdapter.internalValueAdapter.GetValue(this);

        public override string Name => ModelEnumerationItemNodeAdapter.nameAdapter.GetValue(this);

        public override string Path => $"{(this.Parent == null ? "" : this.Parent.Path)}{this.Name}";

        public override string Typename => ModelEnumerationItemNodeAdapter.typenameAdapter.GetValue(this);

        public override object Value => ModelEnumerationItemNodeAdapter.valueAdapter.GetValue(this);

        public override bool HasValue => ModelEnumerationItemNodeAdapter.hasValueAdapter.GetValue(this);

        public override bool SupportsValidation => false;

        public override ValidationSeverity ValidationSeverity => ValidationSeverity.None;

        public override IEnumerable<ValidationMessage> ValidationResults => null;

        public override IEnumerable<ModelNodeBaseAdapter> Children => ModelEnumerationItemNodeAdapter.childrenAdapter.GetValue(this);

        internal override IModelNodeBase Model => ModelEnumerationItemNodeAdapter.modelAdapter.GetValue(this);

        public override bool SupportsPropertyChanged => false;
    }
}