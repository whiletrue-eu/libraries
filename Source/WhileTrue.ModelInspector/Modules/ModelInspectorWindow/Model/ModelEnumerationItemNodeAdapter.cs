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
            IPropertyAdapterFactory<ModelEnumerationItemNodeAdapter> Factory = GetPropertyAdapterFactory<ModelEnumerationItemNodeAdapter>();
            internalValueAdapter = Factory.Create(
                instance=>instance.InternalValue,
                instance=>ModelNodeBaseAdapter.GetAdapter(instance, instance.enumerationItem.Value)
                );
            modelAdapter = Factory.Create(
                instance => instance.Model,
                instance => instance.InternalValue.Model
                );
            nameAdapter = Factory.Create(
                instance => instance.Name,
                instance => instance.enumerationItem.Name
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
            childrenAdapter = Factory.Create(
                instance => instance.Children,
                instance => instance.InternalValue.Children
                );        
        }

        public ModelEnumerationItemNodeAdapter(IModelNodeAdapterParent parent, IEnumerationItemNode enumerationItem):base(parent)
        {
            this.enumerationItem = enumerationItem;
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
            get { return string.Format("{0}{1}", this.Parent == null ? "" : this.Parent.Path, this.Name); }
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

        public override bool SupportsValidation
        {
            get { return false; }
        }

        public override ValidationSeverity ValidationSeverity
        {
            get { return ValidationSeverity.None; }
        }

        public override IEnumerable<ValidationMessage> ValidationResults
        {
            get { return null; }
        }

        public override IEnumerable<ModelNodeBaseAdapter> Children
        {
            get { return childrenAdapter.GetValue(this); }
        }

        internal override IModelNodeBase Model
        {
            get { return modelAdapter.GetValue(this); }
        }

        public override bool SupportsPropertyChanged { get { return false; } }
    }
}