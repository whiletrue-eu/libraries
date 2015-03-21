using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Channels;
using WhileTrue.Classes.Logging;

namespace WhileTrue.Classes.Framework
{
    public class EnumerablePropertyAdapter<SourcePropertyType, PropertyType> : PropertyAdapterBase<IEnumerable<SourcePropertyType>>
    {
        private readonly NotifyChangeExpression<Func<SourcePropertyType, PropertyType>> adapterCreation;
        private readonly ValueRetrievalStrategyBase valueRetriever;


        internal EnumerablePropertyAdapter(Expression<Func<IEnumerable<SourcePropertyType>>> getExpression, Expression<Func<SourcePropertyType, PropertyType>> adapterCreation, Action changedCallback, EventBindingMode eventBindingMode, ValueRetrievalMode valueRetrievalMode)
            : base(getExpression, changedCallback, eventBindingMode)
        {
            this.adapterCreation = new NotifyChangeExpression<Func<SourcePropertyType, PropertyType>>(adapterCreation, eventBindingMode);
            this.adapterCreation.Changed += AdapterCreationChanged;
            switch(valueRetrievalMode)
            {
                case ValueRetrievalMode.Immediately:
                    this.valueRetriever = new ImmediateValueRetrievalStrategy(this);
                    break;
                case ValueRetrievalMode.Lazy:
                    this.valueRetriever = new LazyValueRetrievalStrategy(this);
                    break;
                case ValueRetrievalMode.OnDemand:
                    this.valueRetriever = new OnDemandValueRetrievalStrategy(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("valueRetrievalMode");
            }
            this.valueRetriever.NotifyInitialized();
        }

        private void AdapterCreationChanged(object sender, EventArgs e)
        {
            this.valueRetriever.NotifyAdapterCreationChanged();
        }

        protected override void NotifyExpressionChanged(object sender, EventArgs e)
        {
            DebugLogger.WriteLine(this, LoggingLevel.Normal, () => string.Format("Event received on {0}: {1}", sender, DebugLogger.ToString(e)));

            this.valueRetriever.NotifyChanged();
        }

        public IEnumerable<PropertyType> GetCollection()
        {
            return this.valueRetriever.GetCollection();
        }

        private void UpdateCollectionItems(SourcePropertyType[] values,ObservableCollection<PropertyType> collection, Func<SourcePropertyType, PropertyType> adapterCreation, ref SourcePropertyType[] oldValues )
        {
            if (values.Length > 0)
            {
                List<SourcePropertyType> OldValues = new List<SourcePropertyType>(oldValues ?? new SourcePropertyType[0]);
                int Index = 0;
                for (; Index < values.Length; Index++)
                {
                    if (OldValues.Count > Index && object.Equals(OldValues[Index], values[Index]))
                    {
                        //Item is still the same -> nothing to do
                        DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Collection Update: index {0} stys unchanged", Index));
                    }
                    else
                    {
                        //Search for the item from the current item on
                        int SearchIndex = OldValues.IndexOf(values[Index], Index);
                        if (SearchIndex != -1)
                        {
                            //Found in the list -> move to front
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Collection Update: Move  index {0} to index {1}", SearchIndex, Index));

                            collection.Move(SearchIndex, Index);
                            OldValues.RemoveAt(SearchIndex);
                            OldValues.Insert(Index, values[Index]);
                        }
                        else
                        {
                            //Not found -> must be new
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Collection Update: Add new item at index {0}", Index));

                            collection.Insert(Index, adapterCreation.Invoke(values[Index]));
                            OldValues.Insert(Index, values[Index]);
                        }
                    }
                }
                //Now, the list should be as the new values. All values that are to be deleted
                //should be moved to the end. So we just delete
                while (collection.Count > Index)
                {
                    DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Collection Update: Removing item at index {0}", Index));

                    collection.RemoveAt(Index);
                }

                //TODO: It would be better to first delete items to avoid unneeded move operations

                oldValues = values;
            }
            else
            {
                collection.Clear();
                oldValues = new SourcePropertyType[0];
            }
        }

        private abstract class ValueRetrievalStrategyBase
        {
            public abstract IEnumerable<PropertyType> GetCollection();
            public abstract void NotifyChanged();
            public abstract void NotifyAdapterCreationChanged();
            /// <summary>
            /// apart from constructor to support circular dependency (=> stackoverflow) cases, as this needs the constructor to be run completely
            /// </summary>
            public abstract void NotifyInitialized();
        }
        private class ImmediateValueRetrievalStrategy : ValueRetrievalStrategyBase
        {
            private readonly EnumerablePropertyAdapter<SourcePropertyType, PropertyType> owner;
            private readonly ObservableCollection<PropertyType> collection = new ObservableCollection<PropertyType>();
            private SourcePropertyType[] oldValues;
            private Value<IEnumerable<PropertyType>> value;

            public ImmediateValueRetrievalStrategy(EnumerablePropertyAdapter<SourcePropertyType, PropertyType> owner)
            {
                this.owner = owner;
            }
            
            public override void NotifyInitialized()
            {
                this.value = this.owner.RetrieveValue(this.PostProcess);
            }

            private IEnumerable<PropertyType> PostProcess(IEnumerable<SourcePropertyType> value)
            {
                this.owner.UpdateCollectionItems(value.ToArray(), this.collection, this.owner.adapterCreation, ref this.oldValues);
                return this.collection;
            }

            public override IEnumerable<PropertyType> GetCollection()
            {
                return this.value.GetValue();
            }

            public override void NotifyChanged()
            {
                Value<IEnumerable<PropertyType>> Value = this.owner.RetrieveValue(this.PostProcess);
                if (Value.Equals(this.value) == false)
                {
                    this.value = Value;
                    this.owner.InvokeChanged();
                }
            }

            public override void NotifyAdapterCreationChanged()
            {
                //reset cache
                this.collection.Clear();
                this.oldValues = new SourcePropertyType[0];
                //recreate all entries
                Value<IEnumerable<PropertyType>> Value = this.owner.RetrieveValue(this.PostProcess);
                if (Value.Equals(this.value) == false)
                {
                    this.value = Value;
                    this.owner.InvokeChanged();
                }
            }
        }
        private class LazyValueRetrievalStrategy : ValueRetrievalStrategyBase
        {
            private readonly EnumerablePropertyAdapter<SourcePropertyType, PropertyType> owner;
            private readonly ObservableCollection<PropertyType> collection = new ObservableCollection<PropertyType>();
            private SourcePropertyType[] oldValues;
            private Value<IEnumerable<PropertyType>> value;

            public LazyValueRetrievalStrategy(EnumerablePropertyAdapter<SourcePropertyType, PropertyType> owner)
            {
                this.owner = owner;
            }

            public override void NotifyInitialized()
            {
            }

            private IEnumerable<PropertyType> PostProcess(IEnumerable<SourcePropertyType> value)
            {
                this.owner.UpdateCollectionItems(value.ToArray(), this.collection, this.owner.adapterCreation, ref this.oldValues);
                return this.collection;
            }

            public override IEnumerable<PropertyType> GetCollection()
            {
                if (this.value == null)
                {
                    this.value = this.owner.RetrieveValue(this.PostProcess);
                }
                return this.value.GetValue();
            }

            public override void NotifyChanged()
            {
                Value<IEnumerable<PropertyType>> Value = this.owner.RetrieveValue(this.PostProcess);
                if (Value.Equals(this.value) == false)
                {
                    this.value = Value;
                    this.owner.InvokeChanged();
                }
            }

            public override void NotifyAdapterCreationChanged()
            {
                //reset cache
                this.collection.Clear();
                this.oldValues = new SourcePropertyType[0];
                //recreate all entries
                Value<IEnumerable<PropertyType>> Value = this.owner.RetrieveValue(this.PostProcess);
                if (Value.Equals(this.value) == false)
                {
                    this.value = Value;
                    this.owner.InvokeChanged();
                }
            }
        }
        private class OnDemandValueRetrievalStrategy : ValueRetrievalStrategyBase
        {
            private readonly EnumerablePropertyAdapter<SourcePropertyType, PropertyType> owner;

            public OnDemandValueRetrievalStrategy(EnumerablePropertyAdapter<SourcePropertyType, PropertyType> owner)
            {
                this.owner = owner;
            }

            public override void NotifyInitialized()
            {
            }

            public override IEnumerable<PropertyType> GetCollection()
            {
                return this.owner.RetrieveValue(this.PostProcess).GetValue();
            }

            private IEnumerable<PropertyType> PostProcess(IEnumerable<SourcePropertyType> value)
            {
                ObservableCollection<PropertyType> Collection = new ObservableCollection<PropertyType>();
                //register to collection to receive events to be able to forward
                Collection.CollectionChanged+=Collection_CollectionChanged;
                SourcePropertyType[] OldValues = new SourcePropertyType[0];
                this.owner.UpdateCollectionItems(value.ToArray(), Collection, this.owner.adapterCreation, ref OldValues);
                return Collection;
            }

            private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                //event is forwarded here -> unregister for cleanup
                ((ObservableCollection<PropertyType>)sender).CollectionChanged -= Collection_CollectionChanged;
                this.owner.InvokeChanged();
            }

            public override void NotifyChanged()
            {
                this.owner.InvokeChanged();
            }

            public override void NotifyAdapterCreationChanged()
            {
                this.owner.InvokeChanged();
            }
        }
    }
    public class EnumerablePropertyAdapter<TSource, TSourceEnumerationItem, TTargetEnumerationItem> : PropertyAdapterBase<TSource, IEnumerable<TSourceEnumerationItem>, IEnumerable<TTargetEnumerationItem>> where TSource : ObservableObject
    {
        private readonly Func<TSource, TSourceEnumerationItem, ObservableExpressionFactory.EventSink, TTargetEnumerationItem> adapterCreation;

        internal EnumerablePropertyAdapter(Expression<Func<TSource,IEnumerable<TTargetEnumerationItem>>> propertyAccess, Expression<Func<TSource, IEnumerable<TSourceEnumerationItem>>> getExpression, Expression<Func<TSource, TSourceEnumerationItem, TTargetEnumerationItem>> adapterCreation)
            : base(propertyAccess,getExpression)
        {
            this.adapterCreation = ObservableExpressionFactory.Compile(adapterCreation);
        }

        private class ObservableCachedValueCollection : ObservableCollection<TTargetEnumerationItem>
        {
            private readonly EnumerablePropertyAdapter<TSource, TSourceEnumerationItem, TTargetEnumerationItem> adapter;
            private readonly TSource source;
            private readonly ObservableCollection<CachedValueCollectionItem<TTargetEnumerationItem>> items = new ObservableCollection<CachedValueCollectionItem<TTargetEnumerationItem>>();

            public ObservableCachedValueCollection(EnumerablePropertyAdapter<TSource, TSourceEnumerationItem, TTargetEnumerationItem> adapter, TSource source)
            {
                this.adapter = adapter;
                this.source = source;
            }

            public void Update(IEnumerable<TSourceEnumerationItem> values)
            {
                TSourceEnumerationItem[] Values = values != null ? values.ToArray() : new TSourceEnumerationItem[0];
                if (Values.Length>0)
                {

                    //First delete items to avoid unneeded move operations. save in arraqy to be able to modify the source collection
                    foreach (CachedValueCollectionItem<TTargetEnumerationItem> ItemToDelete in this.items.Where(item => Values.Any(value => Equals(value, item.SourceValue)) == false).ToArray())
                    {
                        this.Remove(ItemToDelete);
                    }

                    ObservableCollection<CachedValueCollectionItem<TTargetEnumerationItem>> OldValues = new ObservableCollection<CachedValueCollectionItem<TTargetEnumerationItem>>(this.items.ToArray());

                    //Move existing entries to new locations and eventually add new entries
                    for (int Index = 0; Index < Values.Length; Index++)
                    {
                        if (OldValues.Count > Index && object.Equals(OldValues[Index], Values[Index]))
                        {
                            //Item is still the same -> nothing to do
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Collection Update: index {0} stys unchanged", Index));
                        }
                        else
                        {
                            //Search for the item. first we have to find the item in the cache to get the index. Skip already sorted entries to allow handling of equal objects in the same list
                            int SearchIndex = OldValues.IndexOf(OldValues.Skip(Index).FirstOrDefault(_ => Equals(_.SourceValue, Values[Index])));
                            if (SearchIndex != -1)
                            {
                                //Found in the list 
                                if (Index != SearchIndex)
                                {
                                    //-> move to front
                                    DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Collection Update: Move  index {0} to index {1}", SearchIndex, Index));

                                    this.Move(SearchIndex, Index);
                                    OldValues.Move(SearchIndex, Index);
                                }
                                else
                                {
                                    //Position already matches -> do nothing
                                }
                            }
                            else
                            {
                                //Not found -> must be new
                                DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Collection Update: Add new item at index {0}", Index));

                                TSourceEnumerationItem SourceValue = Values[Index];
                                ObservableExpressionFactory.EventSink EventSink = new ObservableExpressionFactory.EventSink((sender, e) => this.AdapterCreationCallback(sender, e, SourceValue));
                                CachedValueCollectionItem<TTargetEnumerationItem> NewItem = new CachedValueCollectionItem<TTargetEnumerationItem>(SourceValue,
                                    this.adapter.adapterCreation(this.source, SourceValue, EventSink), EventSink);
                                this.Insert(Index, NewItem);
                                OldValues.Insert(Index, NewItem);
                            }
                        }
                    }
                }
                else
                {
                    this.ClearItems();
                }
            }

            private void Insert(int index, CachedValueCollectionItem<TTargetEnumerationItem> newItem)
            {
                this.items.Insert(index,newItem);
                base.Insert(index, newItem.TargetValue);
            }

            private void Move(int oldIndex, int newIndex)
            {
                this.items.Move(oldIndex, newIndex);
                base.Move(oldIndex, newIndex);
            }

            private void Remove(CachedValueCollectionItem<TTargetEnumerationItem> itemToDelete)
            {
                this.items.Remove(itemToDelete);
                base.Remove(itemToDelete.TargetValue);
            }

            private new void ClearItems()
            {
                this.items.Clear();
                base.ClearItems();
            }

            private void AdapterCreationCallback(object eventSender, EventArgs eventArgs, TSourceEnumerationItem sourceValue)
            {
                try
                {
                    //Replace item with a newly created adapter
                    var OldItem = this.items.First(_ => Equals(_.SourceValue, sourceValue));

                    ObservableExpressionFactory.EventSink EventSink = new ObservableExpressionFactory.EventSink((sender, e) => this.AdapterCreationCallback(sender, e, sourceValue));
                    CachedValueCollectionItem<TTargetEnumerationItem> NewItem = new CachedValueCollectionItem<TTargetEnumerationItem>(sourceValue, this.adapter.adapterCreation(this.source, sourceValue, EventSink), EventSink);

                    this.Replace(OldItem, NewItem);
                }
                catch (Exception)
                {
                    // Something happend while converting and/or adding the item. Reset the colleciton and retry 'from scratch'
                    this.adapter.NotifyItemUpdateFailed( this.source,eventSender, eventArgs);
                }
            }

            private void Replace(CachedValueCollectionItem<TTargetEnumerationItem> oldItem, CachedValueCollectionItem<TTargetEnumerationItem> newItem)
            {
                int Index = this.items.IndexOf(oldItem);
                this.Insert(Index, newItem);
                this.Remove(oldItem);
            }
        }


        private class CachedValueCollectionItem<T>
        {
            // ReSharper disable once NotAccessedField.Local - only to avoid having it garbage collected
            private readonly ObservableExpressionFactory.EventSink eventSink;

            public CachedValueCollectionItem(TSourceEnumerationItem sourceValue, TTargetEnumerationItem targetValue, ObservableExpressionFactory.EventSink eventSink)
            {
                this.eventSink = eventSink;
                this.SourceValue = sourceValue;
                this.TargetValue = targetValue;
            }

            public TTargetEnumerationItem TargetValue { get; private set; }
            public TSourceEnumerationItem SourceValue { get; private set; }
        }

        public IEnumerable<TTargetEnumerationItem> GetCollection(TSource source)
        {
            ObservableObject.PropertyValueCache PropertyValues = source.GetPropertyValueCache();
            if (PropertyValues.HasValue(this))
            {
                return PropertyValues.GetValue(this).GetValue();
            }
            else
            {
                ObservableCachedValueCollection Collection = new ObservableCachedValueCollection(this, source);
                ObservableObject.CachedValue<IEnumerable<TSourceEnumerationItem>> Value = this.RetrieveValue(source, (sender, e) => this.UpdateCollection(source, Collection, sender, e));
                try
                {
                    Collection.Update(Value.GetValue());
                    return PropertyValues.SetValue(this, new ObservableObject.CachedValue<IEnumerable<TTargetEnumerationItem>>(Collection,Value.EventSink)).GetValue();
                }
                catch (Exception Exception)
                {
                    return PropertyValues.SetValue(this, new ObservableObject.CachedValue<IEnumerable<TTargetEnumerationItem>>(Exception, Value.EventSink)).GetValue();
                }
            }
        }


        private void UpdateCollection(TSource source, ObservableCachedValueCollection collection, object eventSender, EventArgs eventArgs)
        {
            ObservableObject.PropertyValueCache PropertyValues = source.GetPropertyValueCache();
            ObservableObject.CachedValue<IEnumerable<TSourceEnumerationItem>> Value = this.RetrieveValue(source, (sender, e) => this.UpdateCollection(source, collection, sender, e));
            try
            {
                collection.Update(Value.GetValue());
                PropertyValues.SetValue(this, new ObservableObject.CachedValue<IEnumerable<TTargetEnumerationItem>>(collection, Value.EventSink)).GetValue();
            }
            catch (Exception Exception)
            {
                PropertyValues.SetValue(this, new ObservableObject.CachedValue<IEnumerable<TTargetEnumerationItem>>(Exception, Value.EventSink)).GetValue();
                source.NotifyPropertyChanged(this.propertyName, eventSender, eventArgs);
            }
        }

        private void NotifyItemUpdateFailed(TSource source, object eventSender, EventArgs eventArgs)
        {
            source.GetPropertyValueCache().ClearValue(this);
            source.NotifyPropertyChanged(this.propertyName, eventSender, eventArgs);
        }

    }
}