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
            IPropertyAdapterFactory<ModelValueNodeAdapter> Factory = GetPropertyAdapterFactory<ModelValueNodeAdapter>();

            typenameAdapter = Factory.Create(
                instance => instance.Typename,
                instance => instance.model.Type == null ? "<null>" : instance.model.Type.Name
                );
            valueAdapter = Factory.Create(
                instance => instance.Value,
                instance => instance.model.Value
                );            
        }

        public ModelValueNodeAdapter(IModelNodeAdapterParent parent, IModelValueNode model):base(parent)
        {
            this.model = model;
        }

        public override string Name
        {
            get { return ""; }
        }

        public override string Path
        {
            get { return ""; }
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
            get { return true; }
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
            get { return new ObservableReadOnlyCollection<ValidationMessage>(); }
        }


        public override IEnumerable<ModelNodeBaseAdapter> Children
        {
            get { return null; }
        }

        internal override IModelNodeBase Model
        {
            get { return this.model; }
        }

        public override bool SupportsPropertyChanged { get { return false; } }
    }
}