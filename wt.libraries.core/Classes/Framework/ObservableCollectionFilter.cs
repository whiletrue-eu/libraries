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
    ///     Implements an adapter to an observable collection that can wrap each item from the source item type into another
    ///     item type and
    ///     which can filter items based on any custom criteria
    /// </summary>
    [PublicAPI]
    public class ObservableCollectionFilter<TSourceType, TItemType> : ObservableObject, IEnumerable<TItemType>,
        INotifyCollectionChanged
    {
        private readonly Func<TSourceType, TItemType> filter;
        private readonly ObservableCollection<TItemType> innerList;
        private readonly Dictionary<TSourceType, TItemType> itemMappings = new Dictionary<TSourceType, TItemType>();

        /// <summary />
        public ObservableCollectionFilter(IEnumerable<TSourceType> source, Func<TSourceType, TItemType> filter)
        {
            ((object) source).DbC_Assure(value => value is INotifyCollectionChanged);

            this.filter = filter;
            innerList = new ObservableCollection<TItemType>();
            innerList.CollectionChanged += InnerListCollectionChanged;

            ((INotifyCollectionChanged) source).CollectionChanged += SourceCollectionChanged;

            var Index = 0;

            foreach (var Item in source)
            {
                Insert(Item, Index);
                Index++;
            }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TItemType> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };

        private void InnerListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged(this, e);
        }

        private void Insert(TSourceType item, int index)
        {
            var FilteredItem = FilterItem(item);
            if (Equals(FilteredItem, default(TItemType)) == false)
            {
                itemMappings.Add(item, FilteredItem);
                innerList.Insert(index, FilteredItem);
            }
        }

        private TItemType FilterItem(TSourceType item)
        {
            return filter(item);
        }

        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count > 1)
                        throw new NotSupportedException("Cannot handle collection events with more than one item!");
                    Insert((TSourceType) e.NewItems[0], e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count > 1)
                        throw new NotSupportedException("Cannot handle collection events with more than one item!");
                    if (itemMappings.ContainsKey((TSourceType) e.OldItems[0]))
                    {
                        innerList.Remove(itemMappings[(TSourceType) e.OldItems[0]]);
                        itemMappings.Remove((TSourceType) e.OldItems[0]);
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException("Item replace currently not supported!");
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException("Item move currently not supported!");
                case NotifyCollectionChangedAction.Reset:
                    itemMappings.Clear();
                    innerList.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}