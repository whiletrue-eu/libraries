using WhileTrue.Classes.Framework;

namespace WhileTrue.Modules.ModelInspector
{
    internal class ModelGroup : ObservableObject, IModelGroup
    {
        private readonly ModelInfoCollection models = new ModelInfoCollection();
        private string name;

         public ModelGroup(string name)
        {
            this.name=name;
        }
        
        public IModelInfoCollection Models => this.models;

        public string Name
        {
            get { return this.name; }
            set { this.SetAndInvoke(nameof(ModelGroup.Name), ref this.name, value); }
        }
    }
}