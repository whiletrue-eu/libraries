using System;
using System.Collections.Generic;
using WhileTrue.Classes.Framework;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{
    internal class ModelEnumerableNodeAdapter : ModelNodeBaseAdapter
    {
        private readonly IModelEnumerableNode model;
        private static readonly EnumerablePropertyAdapter<ModelEnumerableNodeAdapter, IEnumerationItemNode, ModelNodeBaseAdapter> childrenAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelEnumerableNodeAdapter, string> typenameAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelEnumerableNodeAdapter, object> valueAdapter;

        static ModelEnumerableNodeAdapter()
        {
            IPropertyAdapterFactory<ModelEnumerableNodeAdapter> Factory = GetPropertyAdapterFactory<ModelEnumerableNodeAdapter>();
            typenameAdapter = Factory.Create(
                instance => instance.Typename,
                instance => instance.model.Type.Name
                );
            valueAdapter = Factory.Create(
                instance => instance.Value,
                instance => instance.model.Value
                );
            childrenAdapter = Factory.Create(
                instance => instance.Children,
                instance => instance.model.Items,
                (instance, value) => ModelNodeBaseAdapter.GetAdapter(instance, value)
                );            
        }

        public ModelEnumerableNodeAdapter(IModelNodeAdapterParent parent, IModelEnumerableNode model):base(parent)
        {
            this.model = model;

        }

        public override string Name
        {
            get { return ""; }
        }

        public override string Path
        {
            get { return this.Parent == null ? "" : this.Parent.Path; }
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
            get { return false; }
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
            get { return childrenAdapter.GetCollection(this); }
        }

        internal override IModelNodeBase Model
        {
            get { return this.model; }
        }

        public override bool SupportsPropertyChanged { get { return false; } }
    }
}