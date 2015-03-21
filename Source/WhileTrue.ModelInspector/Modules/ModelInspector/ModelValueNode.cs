using System;

namespace WhileTrue.Modules.ModelInspector
{
    internal class ModelValueNode : ModelNodeBase, IModelValueNode
    {
        private readonly object value;

        public ModelValueNode(object value)
        {
            this.value = value;
        }

        public override object Value
        {
            get
            {
                return this.value;
            }
        }

        public override Type Type
        {
            get { return this.value!=null?this.value.GetType():null; }
        }
    }
}