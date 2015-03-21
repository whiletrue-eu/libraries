using System.Collections.Generic;
using WhileTrue.Classes.Framework;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{
    internal class ModelValueNodeAdapter : ModelNodeBaseAdapter
    {
        private readonly IModelValueNode model;
        private static readonly ReadOnlyPropertyAdapter<ModelValueNodeAdapter, object> valueAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelValueNodeAdapter, string> typenameAdapter;

        static ModelValueNodeAdapter()
        {
            IPropertyAdapterFactory<ModelValueNodeAdapter> Factory = ObservableObject.GetPropertyAdapterFactory<ModelValueNodeAdapter>();

            ModelValueNodeAdapter.typenameAdapter = Factory.Create(
                nameof(ModelValueNodeAdapter.Typename),
                instance => instance.model.Type == null ? "<null>" : instance.model.Type.Name
                );
            ModelValueNodeAdapter.valueAdapter = Factory.Create(
                nameof(ModelValueNodeAdapter.Value),
                instance => instance.model.Value
                );            
        }

        public ModelValueNodeAdapter(IModelNodeAdapterParent parent, IModelValueNode model):base(parent)
        {
            this.model = model;
        }

        public override string Name => "";

        public override string Path => "";

        public override string Typename => ModelValueNodeAdapter.typenameAdapter.GetValue(this);

        public override object Value => ModelValueNodeAdapter.valueAdapter.GetValue(this);

        public override bool HasValue => true;

        public override bool SupportsValidation => false;

        public override ValidationSeverity ValidationSeverity => ValidationSeverity.None;

        public override IEnumerable<ValidationMessage> ValidationResults => new ObservableReadOnlyCollection<ValidationMessage>();


        public override IEnumerable<ModelNodeBaseAdapter> Children => null;

        internal override IModelNodeBase Model => this.model;

        public override bool SupportsPropertyChanged => false;
    }
}