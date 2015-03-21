using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace WhileTrue.Modules.ModelInspector
{
    internal class ModelComplexNode : ModelNodeBase, IModelComplexNode
    {
        private readonly INotifyPropertyChanged value;
        private Dictionary<string, PropertyNode> properties;

        internal ModelComplexNode(INotifyPropertyChanged value)
        {
            this.value = value;
        }

        private void UpdateProperties()
        {
            this.properties = new Dictionary<string, PropertyNode>();
            foreach (PropertyInfo PropertyInfo in this.value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                PropertyNode Node = PropertyNode.GetNode(this.value, PropertyInfo);
                this.properties.Add(PropertyInfo.Name, Node);
            }
        }

        public IEnumerable<IPropertyNode> Properties
        {
            get
            {
                this.UpdateProperties();
                return from KeyValuePair<string,PropertyNode> Property in this.properties orderby Property.Key select (IPropertyNode)Property.Value;
            }
        }

        public override Type Type => this.value.GetType();

        public override object Value => this.value;
    }
}