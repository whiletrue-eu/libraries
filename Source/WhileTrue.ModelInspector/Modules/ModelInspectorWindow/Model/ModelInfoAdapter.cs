using System.Windows;
using System.Windows.Input;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.DragNDrop;
using WhileTrue.Classes.Framework;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{

    internal class ModelInfoAdapter : ObservableObject, IModelNodeAdapterParent, IDragDropSource
    {
        private readonly IModelInfo model;
        private static readonly ReadOnlyPropertyAdapter<ModelInfoAdapter, string> typeNameAdapter;
        private static readonly ReadOnlyPropertyAdapter<ModelInfoAdapter, ModelNodeBaseAdapter> rootAdapter;
        private static readonly PropertyAdapter<ModelInfoAdapter, string> nameAdapter;
        private readonly ICommand closeModelCommand;
        private readonly ModelGroupAdapter owner;

        static ModelInfoAdapter()
        {
            IPropertyAdapterFactory<ModelInfoAdapter> Factory = GetPropertyAdapterFactory<ModelInfoAdapter>();

            typeNameAdapter = Factory.Create(
                instance => instance.TypeName,
                instance => instance.model.Root.Type != null ? instance.model.Root.Type.Name : "null"
                );
            rootAdapter = Factory.Create(
                instance => instance.Root,
                instance => ModelNodeBaseAdapter.GetAdapter(instance, instance.model.Root)
                );
            nameAdapter = Factory.Create(
                instance => instance.Name,
                instance => instance.model.Name,
                (instance, value) => instance.model.Name = value
                );
        }

        public ModelInfoAdapter(ModelGroupAdapter owner, IModelInfo model)
        {
            this.owner = owner;
            this.model = model;



            this.closeModelCommand = new DelegateCommand(this.CloseModel, ()=>model.NonClosable==false, EventBindingMode.Weak);
        }

        private void CloseModel()
        {
            this.owner.RemoveModel(this);
        }

        public string Name
        {
            get { return nameAdapter.GetValue(this); }
            set { nameAdapter.SetValue(this,value); }
        }

        public string TypeName
        {
            get
            {
                return typeNameAdapter.GetValue(this);
            }
        }

        public ModelNodeBaseAdapter Root
        {
            get { return rootAdapter.GetValue(this); }
        }

        public IModelNodeAdapterParent Parent
        {
            get { return null; }
        }

        public string Path
        {
            get { return this.Name; }
        }

        public IModelInfo Model
        {
            get {
                return model;
            }
        }

        public object DragData
        {
            get { return this; }
        }

        public DragDropEffects DragEffects
        {
            get { return DragDropEffects.Move|DragDropEffects.Copy; }
        }

        public void NotifyDropped(DragDropEffect dropEffect)
        {
            if (dropEffect == DragDropEffect.Move)
            {
                this.CloseModel();
            }
        }

        public ICommand CloseModelCommand
        {
            get
            {
                return this.closeModelCommand;
            }
        }

        public ModelGroupAdapter Owner
        {
            get { return this.owner; }
        }
    }
}