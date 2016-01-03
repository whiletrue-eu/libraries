using System;
using System.Collections;
using System.ComponentModel;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Modules.ModelInspector
{
    internal abstract class ModelNodeBase : ObservableObject, IModelNodeBase
    {
        private static readonly ObjectCache<ObjectCacheKey<object>,object, ModelNodeBase> nodeCache = new ObjectCache<ObjectCacheKey<object>, object, ModelNodeBase>((key,value) => ModelNodeBase.CreateNode(value));

        internal static ModelNodeBase GetNode(object value)
        {
            return ModelNodeBase.nodeCache.GetObject(new ObjectCacheKey<object>(value),value); //objectcachekey also supports null value
        }

        private static ModelNodeBase CreateNode(object value)
        {
            if (value is string)
            {
                return new ModelValueNode(value);
            }
            if (value is IEnumerable)
            {
                return new ModelEnumerableNode((IEnumerable)value);
            }
            else if (value is INotifyPropertyChanged)
            {
                return new ModelComplexNode((INotifyPropertyChanged)value);
            }
            else
            {
                return new ModelValueNode(value);
            }
        }

        public abstract Type Type { get; }
        public abstract object Value { get; }
    }
}