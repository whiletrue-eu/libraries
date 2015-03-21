using System.Collections.Generic;
using WhileTrue.Classes.Framework;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{
    internal class ModelComplexNodeAdapter : ModelNodeBaseAdapter
    {
        private readonly IModelComplexNode model;
        private static readonly EnumerablePropertyAdapter<ModelComplexNodeAdapter, IPropertyNode, ModelNodeBaseAdapter> childrenAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelComplexNodeAdapter, string> typenameAdapter;

        static ModelComplexNodeAdapter()
        {
            IPropertyAdapterFactory<ModelComplexNodeAdapter> Factory = GetPropertyAdapterFactory<ModelComplexNodeAdapter>();
            typenameAdapter = Factory.Create(
                instance => instance.Typename,
                instance => instance.model.Type.Name
                );
            childrenAdapter = Factory.Create(
                instance => instance.Children,
                instance => instance.model.Properties,
                (instance, value) => ModelNodeBaseAdapter.GetAdapter(instance, value)
                );            
        }

        public ModelComplexNodeAdapter(IModelNodeAdapterParent parent, IModelComplexNode model):base(parent)
        {
            this.model = model;

        }

        public override string Name
        {
            get { return ""; }
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
            get { return null; }
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