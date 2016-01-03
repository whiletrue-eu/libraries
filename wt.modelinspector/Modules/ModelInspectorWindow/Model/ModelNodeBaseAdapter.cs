using System;
using System.Collections.Generic;
using System.Windows;
using WhileTrue.Classes.DragNDrop;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.Modules.ModelInspector;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{
    internal abstract class ModelNodeBaseAdapter: ObservableObject, IDragDropSource, IModelNodeAdapterParent
    {
        private static readonly ObjectCache<ObjectCacheKey<IModelNodeAdapterParent, INodeBase>, IModelNodeAdapterParent, INodeBase, ModelNodeBaseAdapter> cache = new ObjectCache<ObjectCacheKey<IModelNodeAdapterParent, INodeBase>, IModelNodeAdapterParent, INodeBase, ModelNodeBaseAdapter>(ModelNodeBaseAdapter.CreateAdapters);

        private static ModelNodeBaseAdapter CreateAdapters(ObjectCacheKey<IModelNodeAdapterParent, INodeBase> cacheKey, IModelNodeAdapterParent parent, INodeBase node)
        {
            if (node is IModelComplexNode)
            {
                return new ModelComplexNodeAdapter(parent, (IModelComplexNode)node);
            }
            else if (node is IModelValueNode)
            {
                return new ModelValueNodeAdapter(parent,(IModelValueNode)node);
            }
            else if (node is IModelEnumerableNode)
            {
                return new ModelEnumerableNodeAdapter(parent, (IModelEnumerableNode)node);
            }
            else if (node is IPropertyNode)
            {
                return new ModelPropertyNodeAdapter(parent,  (IPropertyNode)node);
            }
            else if (node is IEnumerationItemNode)
            {
                return new ModelEnumerationItemNodeAdapter(parent, (IEnumerationItemNode)node);
            }
            else
            {
                throw new ArgumentException();
            }
        }


        public static ModelNodeBaseAdapter GetAdapter(IModelNodeAdapterParent parent, INodeBase node)
        {
            return ModelNodeBaseAdapter.cache.GetObject(new ObjectCacheKey<IModelNodeAdapterParent, INodeBase>(parent, node), parent, node);
        }


        public abstract string Name { get;}
        public abstract string Path { get;}
        public abstract string Typename { get; }
        public abstract IEnumerable<ModelNodeBaseAdapter> Children { get; }

        public abstract object Value { get; }
        public abstract bool HasValue { get; }

        public abstract bool SupportsPropertyChanged { get; }

        public abstract bool SupportsValidation { get; }
        public abstract ValidationSeverity ValidationSeverity { get; }
        public abstract IEnumerable<ValidationMessage> ValidationResults { get; }

        protected ModelNodeBaseAdapter(IModelNodeAdapterParent parent)
        {
            this.Parent = parent;
        }

        public object DragData => this;

        public DragDropEffects DragEffects => DragDropEffects.Copy;

        internal abstract IModelNodeBase Model{ get; }

        public void NotifyDropped(DragDropEffect dropEffect)
        {
        }

        public IModelNodeAdapterParent Parent { get; }
    }

    internal interface IModelNodeAdapterParent
    {
        IModelNodeAdapterParent Parent { get; }
        string Path { get; }
    }
}