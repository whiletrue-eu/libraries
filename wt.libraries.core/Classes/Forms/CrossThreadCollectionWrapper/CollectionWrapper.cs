﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using WhileTrue.Classes.Utilities;
using Xamarin.Forms;

namespace WhileTrue.Classes.Forms
{
    internal class CollectionWrapper : IEnumerable<object>, INotifyCollectionChanged
    {
        internal static List<object> RegisteredControls = new List<object>();

        private readonly ObservableCollection<object> internalCollection = new ObservableCollection<object>();
        private readonly SemaphoreSlim collectionLock = new SemaphoreSlim(1,1);
        private readonly IEnumerable originalCollection;

        private CollectionWrapper(IEnumerable collection)
        {
            this.collectionLock.Wait();
            try
            {
                ((INotifyCollectionChanged) collection).CollectionChanged += this.CollectionWrapper_CollectionChanged;
                this.originalCollection = collection;
                lock (this.internalCollection)
                {
                    this.originalCollection.ForEach(item => this.internalCollection.Add(item));
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
                var Wrapper = new CollectionWrapper(collection);
                return Wrapper;
        }

        private void CollectionWrapper_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var Sender = (sender as ICollection<object>)?.ToArray();
            Device.BeginInvokeOnMainThread(() => this.NotifyCollectionChanged(Sender, e));
        }

        private void NotifyCollectionChanged(object[] sender,  NotifyCollectionChangedEventArgs e)
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
                        //Dictionary<UIElement, Storyboard> Animations = new Dictionary<UIElement, Storyboard>();
                        var Item = e.OldItems[0];
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
                        if (sender == null)
                        {
                            return;
                        }
                        int NumberOfTries = 5;
                        while (NumberOfTries>0)
                        {
                            try
                            {
                                sender.ForEach(item => this.internalCollection.Add(item));
                                NumberOfTries = 0;
                            }
                            catch (InvalidOperationException)
                            {
                                //try to compensate 'enumeration could not execute - collection modified' exceptions that occur due to inadequate synchronization
                                NumberOfTries--;
                            }
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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
    }
}