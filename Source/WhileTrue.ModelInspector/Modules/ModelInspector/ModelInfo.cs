using System;
using System.Linq.Expressions;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Modules.ModelInspector
{
    internal class ModelInfo : ObservableObject, IModelInfo
    {
        private string name;
        private readonly bool nonClosable;
        private readonly ReadOnlyPropertyAdapter<ModelNodeBase> rootAdapter;
        private readonly ReadOnlyPropertyAdapter<object> rootObjectAdapter;

        public ModelInfo(Expression<Func<object>> rootObject, string name, bool nonClosable)
        {
            this.name = name;
            this.nonClosable = nonClosable;

            this.rootObjectAdapter = this.CreatePropertyAdapter(
                ()=>RootObject,
                rootObject
                );

            this.rootAdapter = this.CreatePropertyAdapter(
                ()=>Root,
                ()=>ModelNodeBase.GetNode(this.RootObject)
                );
        }

        public ModelInfo(object rootObject, string name, bool nonClosable) 
            : this(()=>rootObject,name,nonClosable)
        {
        }

        private object RootObject { get { return this.rootObjectAdapter.GetValue(); } }

        public IModelNodeBase Root
        {
            get
            {
                return this.rootAdapter.GetValue();
            }
        }

        public string Name
        {
            get { return this.name; }
            set { this.SetAndInvoke(()=>Name, ref this.name, value); }
        }

        public bool NonClosable
        {
            get { return this.nonClosable; }
        }
    }
}