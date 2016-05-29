using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Modules.ModelInspector
{
    internal class ModelEnumerableNode : ModelNodeBase, IModelEnumerableNode
    {
        private readonly IEnumerable value;
        private List<EnumerationItemNode> items;
        private Exception exception;

        public ModelEnumerableNode(IEnumerable value)
        {
            this.value = value;
            if (this.value is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged) this.value).CollectionChanged += WeakDelegate.Connect<ModelEnumerableNode, INotifyCollectionChanged, NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    this, (INotifyCollectionChanged) this.value, (target, sender, eventargs) => target.CollectionChanged(sender, eventargs), (source, handler) => source.CollectionChanged -= handler);
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyChanged();
        }

        private void UpdateItems()
        {
            if (this.items == null)
            {
                this.items = new List<EnumerationItemNode>();
                int Index = 0;
                object[] Items;
                try
                {
                    //lock in case that the source gets locked too on update. if not, an exception is thrown, but also a new changed event, so an update will happen
                    lock (this.value)
                    {
                        Items = ((IEnumerable<object>) this.value).ToArray();
                    }
                    this.exception = null;
                    foreach (object Value in Items)
                    {
                        this.items.Add(EnumerationItemNode.GetNode(Value, Index));
                        Index++;
                    }
                }
                catch (Exception Exception)
                {
                    this.exception = Exception;
                }
            }
        }

        public override Type Type => this.value.GetType();

        private void NotifyChanged()
        {
            this.items = null;
            this.InvokePropertyChanged(nameof(ModelEnumerableNode.Items));
        }

        public IEnumerable<IEnumerationItemNode> Items
        {
            get
            {
                this.UpdateItems();
                return from Item in this.items select (IEnumerationItemNode)Item;
            }
        }

        public override object Value => this.exception;
    }
}