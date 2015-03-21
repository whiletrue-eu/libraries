using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Framework
{
    public class ObservableCollectionFilter<SourceType, Type> : ObservableObject, IEnumerable<Type>, INotifyCollectionChanged
    {
        public delegate Type FilterItemDelegate(SourceType item);

        private readonly FilterItemDelegate filter;
        private readonly Dictionary<SourceType,Type> itemMappings = new Dictionary<SourceType, Type>();
        private readonly ObservableCollection<Type> innerList;

        public ObservableCollectionFilter(IEnumerable<SourceType> source, FilterItemDelegate filter)
        {
            source.DbC_Assure(value => value is INotifyCollectionChanged);

            this.filter = filter;
            this.innerList = new ObservableCollection<Type>();
            this.innerList.CollectionChanged += this.InnerListCollectionChanged;

            ((INotifyCollectionChanged)source).CollectionChanged += this.SourceCollectionChanged;
            foreach( SourceType Item in source)
            {
                this.Add(Item);
            }            
        }

        private void InnerListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged(this, e);
            /*if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.InvokePropertyChanged(()=>Count);
            }*/
        }

        private void Add(SourceType item)
        {
            Type FilteredItem = this.FilterItem(item);
            if (object.Equals(FilteredItem, default(Type)) == false)
            {
                this.itemMappings.Add(item, FilteredItem);
                this.innerList.Add(FilteredItem);
            }
        }

        protected virtual Type FilterItem(SourceType item)
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
                    this.Add((SourceType) e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count > 1)
                    {
                        throw new NotSupportedException("Cannot handle collection events with more than one item!");
                    }
                    if (this.itemMappings.ContainsKey((SourceType) e.OldItems[0]))
                    {
                        this.innerList.Remove(this.itemMappings[(SourceType) e.OldItems[0]]);
                        this.itemMappings.Remove((SourceType) e.OldItems[0]);
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

        public IEnumerator<Type> GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate{};
    }
}