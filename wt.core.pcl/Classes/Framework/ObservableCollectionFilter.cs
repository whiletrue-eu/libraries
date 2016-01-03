using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Implements an adapter to an observable collection that can wrap each item from the source item type into another item type and 
    /// which can filter items based on any custom criteria
    /// </summary>
   [PublicAPI] public class ObservableCollectionFilter<TSourceType, TItemType> : ObservableObject, IEnumerable<TItemType>, INotifyCollectionChanged
    {

        private readonly Func<TSourceType, TItemType> filter;
        private readonly Dictionary<TSourceType,TItemType> itemMappings = new Dictionary<TSourceType, TItemType>();
        private readonly ObservableCollection<TItemType> innerList;

        /// <summary/>
        public ObservableCollectionFilter(IEnumerable<TSourceType> source, Func<TSourceType,TItemType> filter)
        {
            ((object)source).DbC_Assure(value => value is INotifyCollectionChanged);

            this.filter = filter;
            this.innerList = new ObservableCollection<TItemType>();
            this.innerList.CollectionChanged += this.InnerListCollectionChanged;

            ((INotifyCollectionChanged)source).CollectionChanged += this.SourceCollectionChanged;
            foreach( TSourceType Item in source)
            {
                this.Add(Item);
            }            
        }

        private void InnerListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged(this, e);
        }

        private void Add(TSourceType item)
        {
            TItemType FilteredItem = this.FilterItem(item);
            if (object.Equals(FilteredItem, default(TItemType)) == false)
            {
                this.itemMappings.Add(item, FilteredItem);
                this.innerList.Add(FilteredItem);
            }
        }

        private TItemType FilterItem(TSourceType item)
        {
            return this.filter(item);
        }

        void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count > 1)
                    {
                        throw new NotSupportedException("Cannot handle collection events with more than one item!");
                    }
                    this.Add((TSourceType) e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count > 1)
                    {
                        throw new NotSupportedException("Cannot handle collection events with more than one item!");
                    }
                    if (this.itemMappings.ContainsKey((TSourceType) e.OldItems[0]))
                    {
                        this.innerList.Remove(this.itemMappings[(TSourceType) e.OldItems[0]]);
                        this.itemMappings.Remove((TSourceType) e.OldItems[0]);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException("Item replace currently not supported!");
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException("Item move currently not supported!");
                case NotifyCollectionChangedAction.Reset:
                    this.itemMappings.Clear();
                    this.innerList.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TItemType> GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate{};
    }
}