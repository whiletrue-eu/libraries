using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WhileTrue.Classes.Utilities;
using Xamarin.Forms;

namespace WhileTrue.Classes.Forms
{
    internal class CollectionWrapper : IEnumerable, INotifyCollectionChanged
    {
        private static readonly Dictionary<IEnumerable, CollectionWrapper> collectionWrappers = new Dictionary<IEnumerable, CollectionWrapper>();

        private readonly ObservableCollection<object> internalCollection = new ObservableCollection<object>();
        private readonly IEnumerable originalCollection;
        internal static List<object> RegisteredControls = new List<object>();

        private CollectionWrapper(IEnumerable collection)
        {
            ((INotifyCollectionChanged) collection).CollectionChanged += this.CollectionWrapper_CollectionChanged;
            this.originalCollection = collection;
            this.originalCollection.ForEach(item => this.internalCollection.Add(item));
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return this.internalCollection.GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { this.internalCollection.CollectionChanged += value; }
            remove { this.internalCollection.CollectionChanged -= value; }
        }

        #endregion

        public static CollectionWrapper GetCollectionWrapperInstance(IEnumerable collection/*, bool shareCollectionPerThread*/)
        {
            //if (shareCollectionPerThread)
            //{
                lock (CollectionWrapper.collectionWrappers)
                {
                    return CollectionWrapper.GetCollectionWrapperInstance(CollectionWrapper.collectionWrappers, collection);
                }
            //}
            //else
            //{
            //    return new CollectionWrapper(collection, Dispatcher.CurrentDispatcher);
            //}
        }

        private static CollectionWrapper GetCollectionWrapperInstance(IDictionary<IEnumerable, CollectionWrapper> collectionWrappers, IEnumerable collection)
        {
            lock (collectionWrappers)
            {
                if (collectionWrappers.ContainsKey(collection))
                {
                    return collectionWrappers[collection];
                }
                else
                {
                    CollectionWrapper Wrapper = new CollectionWrapper(collection);
                    collectionWrappers.Add(collection, Wrapper);
                    return Wrapper;
                }
            }
        }

        private void CollectionWrapper_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(()=>this.NotifyCollectionChanged(e));
        }

        private void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 1 ||
                e.OldItems != null && e.OldItems.Count > 1)
            {
                throw new InvalidOperationException("Collection Wrapper extension currently supports no batch add/remove/move");
            }
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex == this.internalCollection.Count)
                    {
                        this.internalCollection.Add(e.NewItems[0]);
                    }
                    else
                    {
                        this.internalCollection.Insert(e.NewStartingIndex, e.NewItems[0]);
                    }
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
                    //Dictionary<UIElement, Storyboard> Animations = new Dictionary<UIElement, Storyboard>();
                    object Item = e.OldItems[0];
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
                case NotifyCollectionChangedAction.Reset:
                    this.internalCollection.Clear();
                    this.originalCollection.ForEach(item => this.internalCollection.Add(item));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
    }
}