using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.DragNDrop;
using WhileTrue.Classes.Framework;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{
    internal class ModelGroupAdapter : ObservableObject, IDragDropSource
    {
        private readonly ModelInspectorWindowModel owner;
        private readonly IModelGroup group;
        private static readonly EnumerablePropertyAdapter<ModelGroupAdapter, IModelInfo, ModelInfoAdapter> modelsAdapter;
        private static readonly PropertyAdapter<ModelGroupAdapter, string> nameAdapter;
        private readonly DelegateCommand closeGroupCommand;
        private readonly DragDropTarget dragDropHandler;

        static ModelGroupAdapter()
        {
            IPropertyAdapterFactory<ModelGroupAdapter> Factory = GetPropertyAdapterFactory<ModelGroupAdapter>();
            modelsAdapter = Factory.Create(
                instance => instance.Models,
                instance => instance.group.Models,
                (instance,model) => new ModelInfoAdapter(instance, model)
                );
            nameAdapter = Factory.Create(
                instance => instance.Name,
                instance => instance.group.Name,
                (instance, value) => instance.group.Name = value
                );        
        }

        public ModelGroupAdapter(ModelInspectorWindowModel owner, IModelGroup group)
        {
            this.owner = owner;
            this.group = group;


            this.dragDropHandler = DragDropTarget.GetFactory()
                .AddTypeHandler<ModelNodeBaseAdapter>(
                    DragDropEffects.Copy,
                    DragDropEffect.Copy,
                    (value, effect, info) =>
                        {
                            int NewIndex = info.GetInfoOrDefault<DropIndex>(this.group.Models.Count);
                            this.group.Models.Insert(NewIndex, new ModelInfo(value.Model.Value, value.Name, false));
                        }
                ).AddTypeHandler<ModelInfoAdapter>(
                    DragDropEffects.Move|DragDropEffects.Copy,
                    DragDropEffect.Move,
                    (value, effect, info) =>
                        {
                            int NewIndex = info.GetInfoOrDefault<DropIndex>(this.group.Models.Count);
                            this.group.Models.Insert(NewIndex, new ModelInfo(value.Model.Root.Value, value.Model.Name, effect == DragDropEffect.Move ? value.Model.NonClosable : false));
                        }
                ).Create();

            this.closeGroupCommand = new DelegateCommand(this.CloseGroup, () => group.Models.All(model=>model.NonClosable==false), EventBindingMode.Weak);
        }

        private void CloseGroup()
        {
            this.owner.RemoveGroup(this);
        }


        public IModelGroup Group
        {
            get { return this.group; }
        }

        public string Name
        {
            get { return nameAdapter.GetValue(this); }
            set { nameAdapter.SetValue(this,value); }
        }

        public IEnumerable<ModelInfoAdapter> Models
        {
            get
            {
                return modelsAdapter.GetCollection(this);
            }
        }

        internal void RemoveModel(ModelInfoAdapter modelInfoAdapter)
        {
            this.group.Models.Remove(modelInfoAdapter.Model);
            if (this.group.Models.Count == 0)
            {
                this.owner.RemoveGroup(this);
            }
        }

        public DragDropTarget DragDropHandler
        {
            get { return this.dragDropHandler; }
        }

        public ICommand CloseGroupCommand
        {
            get
            {
                return this.closeGroupCommand;
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
                this.CloseGroup();
            }
        }
    }
}