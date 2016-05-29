using WhileTrue.Classes.Framework;

namespace WhileTrue.Modules.ModelInspector
{
    internal class ModelGroupCollection : ObservableReadOnlyCollection<IModelGroup>, IModelGroupCollection
    {
        public void Add(IModelGroup modelGroup)
        {
            this.InnerList.Add(modelGroup);
        }

        public void Insert(int index, IModelGroup modelGroup)
        {
            this.InnerList.Insert(index, modelGroup);
        }

        public void Move(IModelGroup modelGroup, int newIndex)
        {
            this.InnerList.Move(this.InnerList.IndexOf(modelGroup), newIndex);
        }

        public void Remove(IModelGroup modelGroup)
        {
            this.InnerList.Remove(modelGroup);
        }
    }
}