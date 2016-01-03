using System;
using System.Linq.Expressions;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Modules.ModelInspector
{
    internal class ModelInfo : ObservableObject, IModelInfo
    {
        private string name;
        private readonly ReadOnlyPropertyAdapter<ModelNodeBase> rootAdapter;
        private readonly ReadOnlyPropertyAdapter<object> rootObjectAdapter;

        public ModelInfo(Expression<Func<object>> rootObject, string name, bool nonClosable)
        {
            this.name = name;
            this.NonClosable = nonClosable;

            this.rootObjectAdapter = this.CreatePropertyAdapter(
                nameof(ModelInfo.RootObject),
                rootObject
                );

            this.rootAdapter = this.CreatePropertyAdapter(
                nameof(ModelInfo.Root),
                ()=>ModelNodeBase.GetNode(this.RootObject)
                );
        }

        public ModelInfo(object rootObject, string name, bool nonClosable) 
            : this(()=>rootObject,name,nonClosable)
        {
        }

        private object RootObject => this.rootObjectAdapter.GetValue();

        public IModelNodeBase Root => this.rootAdapter.GetValue();

        public string Name
        {
            get { return this.name; }
            set { this.SetAndInvoke(nameof(ModelInfo.Name), ref this.name, value); }
        }

        public bool NonClosable { get; }
    }
}