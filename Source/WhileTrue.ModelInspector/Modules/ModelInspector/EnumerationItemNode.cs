using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Modules.ModelInspector
{
    internal class EnumerationItemNode : ObservableObject, IEnumerationItemNode
    {
        private static readonly ObjectCache<ObjectCacheKey<object,int>, object, int, EnumerationItemNode> nodeCache = new ObjectCache<ObjectCacheKey<object,int>, object, int, EnumerationItemNode>((key, value, index) => CreateNode(value,index));

        internal static EnumerationItemNode GetNode(object value, int index)
        {
            return nodeCache.GetObject(new ObjectCacheKey<object,int>(value,index), value, index); //objectcachekey also supports null value
        }

        private static EnumerationItemNode CreateNode(object value, int index)
        {
            return new EnumerationItemNode(value,index);
        }

        private readonly ModelNodeBase value;
        private readonly string name;

        private EnumerationItemNode(object value, int index)
        {
            this.value=ModelNodeBase.GetNode(value);
            this.name = string.Format("[{0}]", index);
        }

        public IModelNodeBase Value
        {
            get { return this.value; }
        }

        public string Name
        {
            get { return this.name; }
        }
    }
}