using System.Windows;
using System.Windows.Input;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.DragNDrop;
using WhileTrue.Classes.Framework;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{

    internal class ModelInfoAdapter : ObservableObject, IModelNodeAdapterParent, IDragDropSource
    {
        private static readonly ReadOnlyPropertyAdapter<ModelInfoAdapter, string> typeNameAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelInfoAdapter, ModelNodeBaseAdapter> rootAdapter;
        private static readonly PropertyAdapter<ModelInfoAdapter, string> nameAdapter;

        static ModelInfoAdapter()
        {
            IPropertyAdapterFactory<ModelInfoAdapter> Factory = ObservableObject.GetPropertyAdapterFactory<ModelInfoAdapter>();

            ModelInfoAdapter.typeNameAdapter = Factory.Create(
                nameof(ModelInfoAdapter.TypeName),
                instance => instance.Model.Root.Type != null ? instance.Model.Root.Type.Name : "null"
                );
            ModelInfoAdapter.rootAdapter = Factory.Create(
                nameof(ModelInfoAdapter.Root),
                instance => ModelNodeBaseAdapter.GetAdapter(instance, instance.Model.Root)
                );
            ModelInfoAdapter.nameAdapter = Factory.Create(
                nameof(ModelInfoAdapter.Name),
                instance => instance.Model.Name,
                (instance, value) => instance.Model.Name = value
                );
        }

        public ModelInfoAdapter(ModelGroupAdapter owner, IModelInfo model)
        {
            this.Owner = owner;
            this.Model = model;



            this.CloseModelCommand = new DelegateCommand(this.CloseModel, ()=>model.NonClosable==false);
        }

        private void CloseModel()
        {
            this.Owner.RemoveModel(this);
        }

        public string Name
        {
            get { return ModelInfoAdapter.nameAdapter.GetValue(this); }
            set { ModelInfoAdapter.nameAdapter.SetValue(this,value); }
        }

        public string TypeName => ModelInfoAdapter.typeNameAdapter.GetValue(this);

        public ModelNodeBaseAdapter Root => ModelInfoAdapter.rootAdapter.GetValue(this);

        public IModelNodeAdapterParent Parent => null;

        public string Path => this.Name;

        public IModelInfo Model { get; }

        public object DragData => this;

        public DragDropEffects DragEffects => DragDropEffects.Move|DragDropEffects.Copy;

        public void NotifyDropped(DragDropEffect dropEffect)
        {
            if (dropEffect == DragDropEffect.Move)
            {
                this.CloseModel();
            }
        }

        public ICommand CloseModelCommand { get; }

        public ModelGroupAdapter Owner { get; }
    }
}