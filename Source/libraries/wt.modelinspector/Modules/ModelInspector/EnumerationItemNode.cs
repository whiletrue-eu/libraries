using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Modules.ModelInspector
{
    internal class EnumerationItemNode : ObservableObject, IEnumerationItemNode
    {
        private static readonly ObjectCache<ObjectCacheKey<object,int>, object, int, EnumerationItemNode> nodeCache = new ObjectCache<ObjectCacheKey<object,int>, object, int, EnumerationItemNode>((key, value, index) => EnumerationItemNode.CreateNode(value,index));

        internal static EnumerationItemNode GetNode(object value, int index)
        {
            return EnumerationItemNode.nodeCache.GetObject(new ObjectCacheKey<object,int>(value,index), value, index); //objectcachekey also supports null value
        }

        private static EnumerationItemNode CreateNode(object value, int index)
        {
            return new EnumerationItemNode(value,index);
        }

        private readonly ModelNodeBase value;

        private EnumerationItemNode(object value, int index)
        {
            this.value=ModelNodeBase.GetNode(value);
            this.Name = $"[{index}]";
        }

        public IModelNodeBase Value => this.value;

        public string Name { get; }
    }
}