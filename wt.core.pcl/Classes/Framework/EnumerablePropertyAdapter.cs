using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using WhileTrue.Classes.Logging;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Property adapter for enumerable properties
    /// </summary>
    public sealed class EnumerablePropertyAdapter<TSourcePropertyType, TPropertyType> : PropertyAdapterBase<IEnumerable<TSourcePropertyType>>
    {
        private readonly NotifyChangeExpression<Func<TSourcePropertyType, TPropertyType>> adapterCreation;
        private readonly ObservableCollection<TPropertyType> collection = new ObservableCollection<TPropertyType>();
        private TSourcePropertyType[] oldValues;
        private Value<IEnumerable<TPropertyType>> value;


        internal EnumerablePropertyAdapter(Expression<Func<IEnumerable<TSourcePropertyType>>> getExpression, Expression<Func<TSourcePropertyType, TPropertyType>> adapterCreation, Action changedCallback)
            : base(getExpression, changedCallback)
        {
            this.adapterCreation = new NotifyChangeExpression<Func<TSourcePropertyType, TPropertyType>>(adapterCreation);
            this.adapterCreation.Changed += this.AdapterCreationChanged;
        }

        private void AdapterCreationChanged(object sender, EventArgs e)
        {
            //reset cache
            this.collection.Clear();
            this.oldValues = new TSourcePropertyType[0];
            //recreate all entries
            Value<IEnumerable<TPropertyType>> Value = this.RetrieveValue(this.PostProcess);
            if (Value.Equals(this.value) == false)
            {
                this.value = Value;
                this.InvokeChanged();
            }
        }

        /// <summary>
        /// Notifies inherited classes that some parts of the instances used during expression evaluation changed
        /// </summary>
        protected override void NotifyExpressionChanged(object sender, EventArgs e)
        {
            DebugLogger.WriteLine(this, LoggingLevel.Normal, () => $"Event received on {sender}: {DebugLogger.ToString(e)}");

            Value<IEnumerable<TPropertyType>> Value = this.RetrieveValue(this.PostProcess);
            if (Value.Equals(this.value) == false)
            {
                this.value = Value;
                this.InvokeChanged();
            }
        }

        /// <summary>
        /// Retrieves the underlying wrapped collection as an ObservableCollection
        /// </summary>
        public IEnumerable<TPropertyType> GetCollection()
        {
            if (this.value == null)
            {
                this.value = this.RetrieveValue(this.PostProcess);
            }
            return this.value.GetValue();
        }

        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        private void UpdateCollectionItems(TSourcePropertyType[] values,ObservableCollection<TPropertyType> collection, Func<TSourcePropertyType, TPropertyType> adapterCreation, ref TSourcePropertyType[] oldValues )
        {
            if (values.Length > 0)
            {
                List<TSourcePropertyType> OldValues = new List<TSourcePropertyType>(oldValues ?? new TSourcePropertyType[0]);
                int Index = 0;
                for (; Index < values.Length; Index++)
                {
                    if (OldValues.Count > Index && object.Equals(OldValues[Index], values[Index]))
                    {
                        //Item is still the same -> nothing to do
                        DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => $"Collection Update: index {Index} stys unchanged");
                    }
                    else
                    {
                        //Search for the item from the current item on
                        int SearchIndex = OldValues.IndexOf(values[Index], Index);
                        if (SearchIndex != -1)
                        {
                            //Found in the list -> move to front
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => $"Collection Update: Move  index {SearchIndex} to index {Index}");

                            collection.Move(SearchIndex, Index);
                            OldValues.RemoveAt(SearchIndex);
                            OldValues.Insert(Index, values[Index]);
                        }
                        else
                        {
                            //Not found -> must be new
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => $"Collection Update: Add new item at index {Index}");

                            collection.Insert(Index, adapterCreation.Invoke(values[Index]));
                            OldValues.Insert(Index, values[Index]);
                        }
                    }
                }
                //Now, the list should be as the new values. All values that are to be deleted
                //should be moved to the end. So we just delete
                while (collection.Count > Index)
                {
                    DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => $"Collection Update: Removing item at index {Index}");

                    collection.RemoveAt(Index);
                }

                //TODO: It would be better to first delete items to avoid unneeded move operations

                oldValues = values;
            }
            else
            {
                collection.Clear();
                oldValues = new TSourcePropertyType[0];
            }


        }
        private IEnumerable<TPropertyType> PostProcess(IEnumerable<TSourcePropertyType> value)
        {
            this.UpdateCollectionItems(value?.ToArray() ?? new TSourcePropertyType[0], this.collection, this.adapterCreation, ref this.oldValues);
            return this.collection;
        }
    }

    /// <summary>
    /// Property adapter for enumerable properties
    /// </summary>
    public class EnumerablePropertyAdapter<TSource, TSourceEnumerationItem, TTargetEnumerationItem> : PropertyAdapterBase<TSource, IEnumerable<TSourceEnumerationItem>> where TSource : ObservableObject
    {
        private readonly Func<TSource, TSourceEnumerationItem, ObservableExpressionFactory.EventSink, TTargetEnumerationItem> adapterCreation;

        internal EnumerablePropertyAdapter(string propertyName, Expression<Func<TSource, IEnumerable<TSourceEnumerationItem>>> getExpression, Expression<Func<TSource, TSourceEnumerationItem, TTargetEnumerationItem>> adapterCreation)
            : base(propertyName,getExpression)
        {
            this.adapterCreation = ObservableExpressionFactory.Compile(adapterCreation);
        }

        private class ObservableCachedValueCollection : ObservableCollection<TTargetEnumerationItem>
        {
            private readonly EnumerablePropertyAdapter<TSource, TSourceEnumerationItem, TTargetEnumerationItem> adapter;
            private readonly TSource source;
            private readonly ObservableCollection<CachedValueCollectionItem> items = new ObservableCollection<CachedValueCollectionItem>();

            public ObservableCachedValueCollection(EnumerablePropertyAdapter<TSource, TSourceEnumerationItem, TTargetEnumerationItem> adapter, TSource source)
            {
                this.adapter = adapter;
                this.source = source;
            }

            [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
            public void Update(IEnumerable<TSourceEnumerationItem> values)
            {
                TSourceEnumerationItem[] Values = values != null ? values.ToArray() : new TSourceEnumerationItem[0];
                if (Values.Length>0)
                {

                    //First delete items to avoid unneeded move operations. save in arraqy to be able to modify the source collection
                    foreach (CachedValueCollectionItem ItemToDelete in this.items.Where(item => Values.Any(value => object.Equals(value, item.SourceValue)) == false).ToArray())
                    {
                        this.Remove(ItemToDelete);
                    }

                    ObservableCollection<CachedValueCollectionItem> OldValues = new ObservableCollection<CachedValueCollectionItem>(this.items.ToArray());

                    //Move existing entries to new locations and eventually add new entries
                    for (int Index = 0; Index < Values.Length; Index++)
                    {
                        if (OldValues.Count > Index && object.Equals(OldValues[Index], Values[Index]))
                        {
                            //Item is still the same -> nothing to do
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => $"Collection Update: index {Index} stays unchanged");
                        }
                        else
                        {
                            //Search for the item. first we have to find the item in the cache to get the index. Skip already sorted entries to allow handling of equal objects in the same list
                            int SearchIndex = OldValues.IndexOf(OldValues.Skip(Index).FirstOrDefault(_ => object.Equals(_.SourceValue, Values[Index])));
                            if (SearchIndex != -1)
                            {
                                //Found in the list 
                                if (Index != SearchIndex)
                                {
                                    //-> move to front
                                    DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => $"Collection Update: Move  index {SearchIndex} to index {Index}");

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
                                DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => $"Collection Update: Add new item at index {Index}");

                                TSourceEnumerationItem SourceValue = Values[Index];
                                ObservableExpressionFactory.EventSink EventSink = new ObservableExpressionFactory.EventSink((sender, e) => this.AdapterCreationCallback(SourceValue));
                                CachedValueCollectionItem NewItem = new CachedValueCollectionItem(SourceValue,
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

            private void Insert(int index, CachedValueCollectionItem newItem)
            {
                this.items.Insert(index,newItem);
                base.Insert(index, newItem.TargetValue);
            }

            private new void Move(int oldIndex, int newIndex)
            {
                this.items.Move(oldIndex, newIndex);
                base.Move(oldIndex, newIndex);
            }

            private void Remove(CachedValueCollectionItem itemToDelete)
            {
                this.items.Remove(itemToDelete);
                base.Remove(itemToDelete.TargetValue);
            }

            private new void ClearItems()
            {
                this.items.Clear();
                base.ClearItems();
            }

            private void AdapterCreationCallback(TSourceEnumerationItem sourceValue)
            {
                try
                {
                    //Replace item with a newly created adapter
                    var OldItem = this.items.First(_ => object.Equals(_.SourceValue, sourceValue));

                    ObservableExpressionFactory.EventSink EventSink = new ObservableExpressionFactory.EventSink((sender, e) => this.AdapterCreationCallback(sourceValue));
                    CachedValueCollectionItem NewItem = new CachedValueCollectionItem(sourceValue, this.adapter.adapterCreation(this.source, sourceValue, EventSink), EventSink);

                    this.Replace(OldItem, NewItem);
                }
                catch (Exception)
                {
                    // Something happend while converting and/or adding the item. Reset the colleciton and retry 'from scratch'
                    this.adapter.NotifyItemUpdateFailed( this.source);
                }
            }

            private void Replace(CachedValueCollectionItem oldItem, CachedValueCollectionItem newItem)
            {
                int Index = this.items.IndexOf(oldItem);
                this.Insert(Index, newItem);
                this.Remove(oldItem);
            }
        }


        private class CachedValueCollectionItem
        {
            // ReSharper disable once NotAccessedField.Local - only to avoid having it garbage collected
            private readonly ObservableExpressionFactory.EventSink eventSink;

            public CachedValueCollectionItem(TSourceEnumerationItem sourceValue, TTargetEnumerationItem targetValue, ObservableExpressionFactory.EventSink eventSink)
            {
                this.eventSink = eventSink;
                this.SourceValue = sourceValue;
                this.TargetValue = targetValue;
            }

            public TTargetEnumerationItem TargetValue { get; }
            public TSourceEnumerationItem SourceValue { get; }
        }

        /// <summary>
        /// Retrieves the underlying wrapped collection of teh specified instance as an ObservableCollection
        /// </summary>
        public IEnumerable<TTargetEnumerationItem> GetCollection(TSource source)
        {
            ObservableObject.PropertyValueCache PropertyValues = source.GetPropertyValueCache();
            lock (PropertyValues)
            {
                if (PropertyValues.HasValue(this))
                {
                    return PropertyValues.GetValue<TSource,IEnumerable<TSourceEnumerationItem>, IEnumerable<TTargetEnumerationItem>>(this).GetValue();
                }
                else
                {
                    ObservableCachedValueCollection Collection = new ObservableCachedValueCollection(this, source);
                    ObservableObject.CachedValue<IEnumerable<TSourceEnumerationItem>> Value = this.RetrieveValue(source, (sender, e) => this.UpdateCollection(source, Collection));
                    try
                    {
                        Collection.Update(Value.GetValue());
                        return PropertyValues.SetValue(this, new ObservableObject.CachedValue<IEnumerable<TTargetEnumerationItem>>(Collection, Value.EventSink)).GetValue();
                    }
                    catch (Exception Exception)
                    {
                        return PropertyValues.SetValue(this, new ObservableObject.CachedValue<IEnumerable<TTargetEnumerationItem>>(Exception, Value.EventSink)).GetValue();
                    }
                }
            }
        }


        private void UpdateCollection(TSource source, ObservableCachedValueCollection collection)
        {
            ObservableObject.PropertyValueCache PropertyValues = source.GetPropertyValueCache();
            lock (PropertyValues)
            {
                ObservableObject.CachedValue<IEnumerable<TSourceEnumerationItem>> Value = this.RetrieveValue(source, (sender, e) => this.UpdateCollection(source, collection));
                try
                {
                    collection.Update(Value.GetValue());
                    PropertyValues.SetValue(this, new ObservableObject.CachedValue<IEnumerable<TTargetEnumerationItem>>(collection, Value.EventSink)).GetValue();
                }
                catch (Exception Exception)
                {
                    PropertyValues.SetValue(this, new ObservableObject.CachedValue<IEnumerable<TTargetEnumerationItem>>(Exception, Value.EventSink)).GetValue();
                    source.NotifyPropertyChanged(this.PropertyName);
                }
            }
        }

        private void NotifyItemUpdateFailed(TSource source)
        {
            ObservableObject.PropertyValueCache PropertyValues = source.GetPropertyValueCache();
            lock (PropertyValues)
            {
                PropertyValues.ClearValue(this);
            }
            source.NotifyPropertyChanged(this.PropertyName);
        }

    }
}