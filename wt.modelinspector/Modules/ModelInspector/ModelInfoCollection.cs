using WhileTrue.Classes.Framework;

namespace WhileTrue.Modules.ModelInspector
{
    internal class ModelInfoCollection : ObservableReadOnlyCollection<IModelInfo>, IModelInfoCollection
    {
        public void Add(IModelInfo model)
        {
            this.InnerList.Add(model);
        }

        public void Insert(int index, IModelInfo model)
        {
            this.InnerList.Insert(index, model);
        }

        public void Move(IModelInfo model, int newIndex)
        {
            this.InnerList.Move(this.InnerList.IndexOf(model), newIndex);
        }

        public void Remove(IModelInfo model)
        {
            this.InnerList.Remove(model);
        }
    }
}