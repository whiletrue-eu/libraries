using WhileTrue.Classes.Components;

namespace WhileTrue.Modules.ModelInspector
{
    [Component]
    internal class ModelInspectorModel : IModelInspectorModel
    {
        private readonly ModelGroupCollection groups = new ModelGroupCollection();

        public IModelGroupCollection Groups => this.groups;
    }
}