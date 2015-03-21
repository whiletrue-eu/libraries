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
            ObservableObject.IPropertyAdapterFactory<ModelEnumerableNodeAdapter> Factory = ObservableObject.GetPropertyAdapterFactory<ModelEnumerableNodeAdapter>();
            ModelEnumerableNodeAdapter.typenameAdapter = Factory.Create(
                nameof(ModelEnumerableNodeAdapter.Typename),
                instance => instance.model.Type.Name
                );
            ModelEnumerableNodeAdapter.valueAdapter = Factory.Create(
                nameof(ModelEnumerableNodeAdapter.Value),
                instance => instance.model.Value
                );
            ModelEnumerableNodeAdapter.childrenAdapter = Factory.Create(
                nameof(ModelEnumerableNodeAdapter.Children),
                instance => instance.model.Items,
                (instance, value) => ModelNodeBaseAdapter.GetAdapter(instance, value)
                );            
        }

        public ModelEnumerableNodeAdapter(IModelNodeAdapterParent parent, IModelEnumerableNode model):base(parent)
        {
            this.model = model;

        }

        public override string Name => "";

        public override string Path => this.Parent == null ? "" : this.Parent.Path;

        public override string Typename => ModelEnumerableNodeAdapter.typenameAdapter.GetValue(this);

        public override object Value => ModelEnumerableNodeAdapter.valueAdapter.GetValue(this);

        public override bool HasValue => false;

        public override bool SupportsValidation => false;

        public override ValidationSeverity ValidationSeverity => ValidationSeverity.None;

        public override IEnumerable<ValidationMessage> ValidationResults => null;

        public override IEnumerable<ModelNodeBaseAdapter> Children => ModelEnumerableNodeAdapter.childrenAdapter.GetCollection(this);

        internal override IModelNodeBase Model => this.model;

        public override bool SupportsPropertyChanged => false;
    }
}