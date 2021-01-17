using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WhileTrue.Classes.Utilities;
using Xamarin.Forms;

namespace WhileTrue.Classes.Forms
{
    internal class CollectionWrapper : IEnumerable<object>, IList, INotifyCollectionChanged
    {
        internal static List<object> RegisteredControls = new List<object>();

        private readonly ObservableCollection<object> internalCollection = new ObservableCollection<object>();
        private readonly SemaphoreSlim collectionLock = new SemaphoreSlim(1, 1);

        private CollectionWrapper(IEnumerable collection)
        {
            this.collectionLock.Wait();
            try
            {
                ((INotifyCollectionChanged)collection).CollectionChanged += this.CollectionWrapper_CollectionChanged;
                lock (this.internalCollection)
                {
                    object[] TempArray = collection.Cast<object>().ToArray();
                    TempArray.ForEach(item => this.internalCollection.Add(item));
                }
            }
            finally
            {
                this.collectionLock.Release();
            }
        }

        #region IEnumerable Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return this.internalCollection.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return this.internalCollection.GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => this.internalCollection.CollectionChanged += value;
            remove => this.internalCollection.CollectionChanged -= value;
        }

        #endregion

        public static CollectionWrapper GetCollectionWrapperInstance(
            IEnumerable collection /*, bool shareCollectionPerThread*/)
        {
            CollectionWrapper Wrapper = new CollectionWrapper(collection);
            return Wrapper;
        }

        private void CollectionWrapper_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {   // In case of reset, we need the sender to reset the 'internalCollection' asynchronous
                object[] Sender = ((IEnumerable)sender).Cast<object>().ToArray();
                Device.BeginInvokeOnMainThread(() => this.NotifyCollectionChangedReset(Sender));
            }
            else
            {   // Else default event handling
                Device.BeginInvokeOnMainThread(() => this.NotifyCollectionChanged(e));
            }
        }

        private void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 1 ||
                e.OldItems != null && e.OldItems.Count > 1)
                throw new InvalidOperationException(
                    "Collection Wrapper extension currently supports no batch add/remove/move");

            this.collectionLock.Wait();
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Debug.Assert(e.NewItems != null, "e.NewItems != null");

                        if (e.NewStartingIndex == this.internalCollection.Count)
                            this.internalCollection.Add(e.NewItems[0]);
                        else
                            this.internalCollection.Insert(e.NewStartingIndex, e.NewItems[0]);
                        //foreach (ItemsControl ItemsControl in CollectionWrapper.RegisteredControls.Where(this.GetIsFadeAnimationEnabled))
                        //{
                        //    UIElement Element = ItemsControl.ItemContainerGenerator.ContainerFromItem(e.NewItems[0]) as UIElement;
                        //    if (Element != null)
                        //    {
                        //        Storyboard Storyboard = CrossThreadCollectionWrapper.GetFadeInAnimation(Element);
                        //        if (Storyboard != null)
                        //        {
                        //            Storyboard MyStoryboard = Storyboard.Clone();
                        //            MyStoryboard.Freeze();
                        //            MyStoryboard.Begin((FrameworkElement) Element);
                        //        }
                        //    }
                        //}
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        Debug.Assert(e.OldItems != null, "e.OldItems != null");
                        //Dictionary<UIElement, Storyboard> Animations = new Dictionary<UIElement, Storyboard>();
                        //var Item = e.OldItems[0];
                        //foreach (ItemsControl ItemsControl in CollectionWrapper.RegisteredControls.Where(this.GetIsFadeAnimationEnabled))
                        //{
                        //    UIElement Element = ItemsControl.ItemContainerGenerator.ContainerFromItem(Item) as UIElement;
                        //    if (Element != null)
                        //    {
                        //        Storyboard Storyboard = CrossThreadCollectionWrapper.GetFadeOutAnimation(Element);
                        //        if (Storyboard != null)
                        //        {
                        //            Storyboard MyStoryboard = Storyboard.Clone();
                        //            MyStoryboard.Completed += delegate
                        //                {
                        //                    Animations.Remove(Element);
                        //                    if (Animations.Count == 0)
                        //                    {
                        //                        this.internalCollection.Remove(Item);
                        //                    }
                        //                };
                        //            MyStoryboard.Freeze();
                        //            Animations.Add(Element, MyStoryboard);
                        //        }
                        //    }
                        //}
                        //if (Animations.Count > 0)
                        //{
                        //    Animations.ForEach(_ => _.Value.Begin((FrameworkElement)_.Key));
                        //}
                        //else
                        //{
                        this.internalCollection.Remove(e.OldItems[0]);
                        //}
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        throw new InvalidOperationException("Collection Wrapper extension currently supports no replace");

                    case NotifyCollectionChangedAction.Move:
                        this.internalCollection.Move(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    //case NotifyCollectionChangedAction.Reset:
                    //    this.internalCollection.Clear();
                    //    if (sender == null)
                    //    {
                    //        return;
                    //    }
                    //    int NumberOfTries = 5;
                    //    while (NumberOfTries > 0)
                    //    {
                    //        try
                    //        {
                    //            sender.ForEach(item => this.internalCollection.Add(item));
                    //            NumberOfTries = 0;
                    //        }
                    //        catch (InvalidOperationException)
                    //        {
                    //            //try to compensate 'enumeration could not execute - collection modified' exceptions that occur due to inadequate synchronization
                    //            NumberOfTries--;
                    //        }
                    //    }

                    //    break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            finally
            {
                this.collectionLock.Release();
            }
        }

        /// <summary>
        /// Handle the 'Reset' event of the collection. Need to be done separately, cause we need the sender as Array
        /// </summary>
        /// <param name="sender"></param>
        private void NotifyCollectionChangedReset(object[] sender)
        {
            this.collectionLock.Wait();
            try
            {
                this.internalCollection.Clear();
                if (sender == null)
                {
                    return;
                }
                // Retry counter
                int NumberOfTries = 5;

                while (NumberOfTries > 0)
                {
                    try
                    {   // Copy all items from sender into the internal collection
                        sender.ForEach(item => this.internalCollection.Add(item));
                        NumberOfTries = 0;
                    }
                    catch (InvalidOperationException)
                    {
                        //try to compensate 'enumeration could not execute - collection modified' exceptions that occur due to inadequate synchronization
                        NumberOfTries--;
                    }
                }
            }
            finally
            {
                this.collectionLock.Release();
            }
        }

        //private bool GetIsFadeAnimationEnabled(ItemsControl itemsControl)
        //{
        //    if (itemsControl.ItemsSource is ICollectionView)
        //    {
        //        return ((ICollectionView) itemsControl.ItemsSource).SourceCollection == this &&
        //               CrossThreadCollectionWrapper.GetEnableItemFadeAnimations(itemsControl);
        //    }
        //    else
        //    {
        //        return itemsControl.ItemsSource == this &&
        //               CrossThreadCollectionWrapper.GetEnableItemFadeAnimations(itemsControl);
        //    }
        //}
        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public int Count => this.internalCollection.Count;
        public bool IsSynchronized => false;
        public object SyncRoot => this;
        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object value)
        {
            return this.internalCollection.Contains(value);
        }

        public int IndexOf(object value)
        {
            return this.internalCollection.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public bool IsFixedSize => false;
        public bool IsReadOnly => true;
        public object this[int index] { get => this.internalCollection[index]; set => throw new NotSupportedException(); }
    }
}