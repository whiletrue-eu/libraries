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
            ObservableObject.IPropertyAdapterFactory<ModelComplexNodeAdapter> Factory = ObservableObject.GetPropertyAdapterFactory<ModelComplexNodeAdapter>();
            ModelComplexNodeAdapter.typenameAdapter = Factory.Create(
                nameof(ModelComplexNodeAdapter.Typename),
                instance => instance.model.Type.Name
                );
            ModelComplexNodeAdapter.childrenAdapter = Factory.Create(
                nameof(ModelComplexNodeAdapter.Children),
                instance => instance.model.Properties,
                (instance, value) => ModelNodeBaseAdapter.GetAdapter(instance, value)
                );            
        }

        public ModelComplexNodeAdapter(IModelNodeAdapterParent parent, IModelComplexNode model):base(parent)
        {
            this.model = model;

        }

        public override string Name => "";

        public override string Path => $"{(this.Parent == null ? "" : this.Parent.Path)}{this.Name}";

        public override string Typename => ModelComplexNodeAdapter.typenameAdapter.GetValue(this);

        public override object Value => null;

        public override bool HasValue => false;

        public override bool SupportsValidation => false;

        public override ValidationSeverity ValidationSeverity => ValidationSeverity.None;

        public override IEnumerable<ValidationMessage> ValidationResults => null;


        public override IEnumerable<ModelNodeBaseAdapter> Children => ModelComplexNodeAdapter.childrenAdapter.GetCollection(this);

        internal override IModelNodeBase Model => this.model;

        public override bool SupportsPropertyChanged => false;
    }
}