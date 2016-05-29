using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.DragNDrop;
using WhileTrue.Classes.Framework;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{
    internal class ModelGroupAdapter : ObservableObject, IDragDropSource
    {
        private readonly ModelInspectorWindowModel owner;
        private static readonly EnumerablePropertyAdapter<ModelGroupAdapter, IModelInfo, ModelInfoAdapter> modelsAdapter;
        private static readonly PropertyAdapter<ModelGroupAdapter, string> nameAdapter;
        private readonly DelegateCommand closeGroupCommand;

        static ModelGroupAdapter()
        {
            IPropertyAdapterFactory<ModelGroupAdapter> Factory = ObservableObject.GetPropertyAdapterFactory<ModelGroupAdapter>();
            ModelGroupAdapter.modelsAdapter = Factory.Create(
                nameof(ModelGroupAdapter.Models),
                instance => instance.Group.Models,
                (instance,model) => new ModelInfoAdapter(instance, model)
                );
            ModelGroupAdapter.nameAdapter = Factory.Create(
                nameof(ModelGroupAdapter.Name),
                instance => instance.Group.Name,
                (instance, value) => instance.Group.Name = value
                );        
        }

        public ModelGroupAdapter(ModelInspectorWindowModel owner, IModelGroup group)
        {
            this.owner = owner;
            this.Group = group;


            this.DragDropHandler = DragDropTarget.GetFactory()
                .AddTypeHandler<ModelNodeBaseAdapter>(
                    DragDropEffects.Copy,
                    DragDropEffect.Copy,
                    (value, effect, info) =>
                        {
                            int NewIndex = info.GetInfoOrDefault<DropIndex>(this.Group.Models.Count);
                            this.Group.Models.Insert(NewIndex, new ModelInfo(value.Model.Value, value.Name, false));
                        }
                ).AddTypeHandler<ModelInfoAdapter>(
                    DragDropEffects.Move|DragDropEffects.Copy,
                    DragDropEffect.Move,
                    (value, effect, info) =>
                        {
                            int NewIndex = info.GetInfoOrDefault<DropIndex>(this.Group.Models.Count);
                            this.Group.Models.Insert(NewIndex, new ModelInfo(value.Model.Root.Value, value.Model.Name, effect == DragDropEffect.Move ? value.Model.NonClosable : false));
                        }
                ).Create();

            this.closeGroupCommand = new DelegateCommand(this.CloseGroup, () => group.Models.All(model=>model.NonClosable==false));
        }

        private void CloseGroup()
        {
            this.owner.RemoveGroup(this);
        }


        public IModelGroup Group { get; }

        public string Name
        {
            get { return ModelGroupAdapter.nameAdapter.GetValue(this); }
            set { ModelGroupAdapter.nameAdapter.SetValue(this,value); }
        }

        public IEnumerable<ModelInfoAdapter> Models => ModelGroupAdapter.modelsAdapter.GetCollection(this);

        internal void RemoveModel(ModelInfoAdapter modelInfoAdapter)
        {
            this.Group.Models.Remove(modelInfoAdapter.Model);
            if (this.Group.Models.Count == 0)
            {
                this.owner.RemoveGroup(this);
            }
        }

        public DragDropTarget DragDropHandler { get; }

        public ICommand CloseGroupCommand => this.closeGroupCommand;

        public object DragData => this;

        public DragDropEffects DragEffects => DragDropEffects.Move|DragDropEffects.Copy;

        public void NotifyDropped(DragDropEffect dropEffect)
        {
            if (dropEffect == DragDropEffect.Move)
            {
                this.CloseGroup();
            }
        }
    }
}