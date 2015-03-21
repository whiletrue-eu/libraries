using System.Collections.Generic;
using System.Windows;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.DragNDrop;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{
    [Component]
    internal class ModelInspectorWindowModel : ObservableObject, IModelInspectorWindowModel
    {
        private readonly IModelInspectorModel model;
        private readonly DragDropTarget dragDropHandler;
        private readonly EnumerablePropertyAdapter<IModelGroup, ModelGroupAdapter> groupsAdapter;

        public ModelInspectorWindowModel(IModelInspectorModel model)
        {
            this.model = model;
            this.groupsAdapter = this.CreatePropertyAdapter(
                nameof(ModelInspectorWindowModel.Groups),
                () => model.Groups,
                group => new ModelGroupAdapter(this, group)
                );

            this.dragDropHandler = DragDropTarget.GetFactory()
                .AddTypeHandler<ModelNodeBaseAdapter>(
                    DragDropEffects.Copy,
                    DragDropEffect.Copy,
                    (value, effect, info) =>
                        {
                            int Count = this.model.Groups.Count;
                            int NewIndex = info.GetInfoOrDefault<DropIndex>(Count);
                            IModelGroup NewGroup = new ModelGroup(value.Name);
                            NewGroup.Models.Add(new ModelInfo(value.Model.Value, value.Name, false));
                            this.model.Groups.Insert(NewIndex, NewGroup);
                        }
                ).AddTypeHandler<ModelInfoAdapter>(
                    DragDropEffects.Move|DragDropEffects.Copy,
                    DragDropEffect.Move,
                    (value, effect, info) =>
                        {
                            int NewIndex = info.GetInfoOrDefault<DropIndex>(this.model.Groups.Count);
                            IModelGroup NewGroup = new ModelGroup(value.Name);
                            NewGroup.Models.Add(new ModelInfo(value.Model.Root.Value, value.Model.Name, effect==DragDropEffect.Move?value.Model.NonClosable:false));
                            this.model.Groups.Insert(NewIndex, NewGroup);
                        }
                ).AddTypeHandler<ModelGroupAdapter>(
                    DragDropEffects.Move|DragDropEffects.Copy,
                    DragDropEffect.Move,
                    (value, effect, info) =>
                    {
                        int NewIndex = info.GetInfoOrDefault<DropIndex>(this.model.Groups.Count);
                        ModelGroup Group = new ModelGroup(value.Group.Name);
                        value.Group.Models.ForEach(modelInfo => Group.Models.Add(new ModelInfo(modelInfo.Root.Value, modelInfo.Name, effect==DragDropEffect.Move?modelInfo.NonClosable:false)));
                        this.model.Groups.Insert(NewIndex, Group );
                    }
                ).Create();

            this.ZoomModel = new ZoomModel();
#if DEBUG
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                (System.Action) delegate
                                    {
                                        ModelGroup Group = new ModelGroup("Model Inspector");
                                        Group.Models.Add(new ModelInfo(this, "Model Inspector", true));
                                        model.Groups.Add(Group);
                                    });
#endif
        }

        public IEnumerable<ModelGroupAdapter> Groups => this.groupsAdapter.GetCollection();

        public IDragDropTarget DragDropHandler => this.dragDropHandler;

        internal void RemoveGroup(ModelGroupAdapter modelGroupAdapter)
        {
            this.model.Groups.Remove(modelGroupAdapter.Group);
        }

        public ZoomModel ZoomModel { get; }
    }
}