﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Wpf
{
    internal class CollectionWrapper : IEnumerable, INotifyCollectionChanged
    {
        private static readonly Dictionary<Dispatcher, Dictionary<IEnumerable, CollectionWrapper>> collectionWrappers =
            new Dictionary<Dispatcher, Dictionary<IEnumerable, CollectionWrapper>>();

        internal static List<ItemsControl> RegisteredControls = new List<ItemsControl>();
        private readonly Dispatcher dispatcher;

        private readonly ObservableCollection<object> internalCollection = new ObservableCollection<object>();
        private readonly IEnumerable originalCollection;

        private CollectionWrapper(IEnumerable collection, Dispatcher dispatcher)
        {
            ((INotifyCollectionChanged) collection).CollectionChanged += CollectionWrapper_CollectionChanged;
            this.dispatcher = dispatcher;
            originalCollection = collection;
            originalCollection.ForEach(item => internalCollection.Add(item));
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return internalCollection.GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => internalCollection.CollectionChanged += value;
            remove => internalCollection.CollectionChanged -= value;
        }

        #endregion

        public static CollectionWrapper GetCollectionWrapperInstance(IEnumerable collection,
            bool shareCollectionPerThread)
        {
            if (shareCollectionPerThread)
                lock (collectionWrappers)
                {
                    var CurrentDispatcher = Dispatcher.CurrentDispatcher;
                    if (collectionWrappers.ContainsKey(CurrentDispatcher))
                        return GetCollectionWrapperInstance(collectionWrappers[CurrentDispatcher], collection,
                            CurrentDispatcher);

                    var CollectionWrappers =
                        new Dictionary<IEnumerable, CollectionWrapper>();
                    collectionWrappers.Add(CurrentDispatcher, CollectionWrappers);
                    return GetCollectionWrapperInstance(CollectionWrappers, collection, CurrentDispatcher);
                }

            return new CollectionWrapper(collection, Dispatcher.CurrentDispatcher);
        }

        private static CollectionWrapper GetCollectionWrapperInstance(
            IDictionary<IEnumerable, CollectionWrapper> collectionWrappers, IEnumerable collection,
            Dispatcher dispatcher)
        {
            lock (collectionWrappers)
            {
                if (collectionWrappers.ContainsKey(collection)) return collectionWrappers[collection];

                var Wrapper = new CollectionWrapper(collection, dispatcher);
                collectionWrappers.Add(collection, Wrapper);
                return Wrapper;
            }
        }

        private void CollectionWrapper_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            dispatcher.BeginInvoke(DispatcherPriority.DataBind,
                new NotifyCollectionChangedDelegate(NotifyCollectionChanged), e);
        }

        private void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 1 ||
                e.OldItems != null && e.OldItems.Count > 1)
                throw new InvalidOperationException(
                    "Collection Wrapper extension currently supports no batch add/remove/move");
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex == internalCollection.Count)
                        internalCollection.Add(e.NewItems[0]);
                    else
                        internalCollection.Insert(e.NewStartingIndex, e.NewItems[0]);
                    foreach (var ItemsControl in RegisteredControls.Where(GetIsFadeAnimationEnabled))
                    {
                        var Element = ItemsControl.ItemContainerGenerator.ContainerFromItem(e.NewItems[0]) as UIElement;
                        if (Element != null)
                        {
                            var Storyboard = CrossThreadCollectionWrapper.GetFadeInAnimation(Element);
                            if (Storyboard != null)
                            {
                                var MyStoryboard = Storyboard.Clone();
                                MyStoryboard.Freeze();
                                MyStoryboard.Begin((FrameworkElement) Element);
                            }
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    var Animations = new Dictionary<UIElement, Storyboard>();
                    var Item = e.OldItems[0];
                    foreach (var ItemsControl in RegisteredControls.Where(GetIsFadeAnimationEnabled))
                    {
                        var Element = ItemsControl.ItemContainerGenerator.ContainerFromItem(Item) as UIElement;
                        if (Element != null)
                        {
                            var Storyboard = CrossThreadCollectionWrapper.GetFadeOutAnimation(Element);
                            if (Storyboard != null)
                            {
                                var MyStoryboard = Storyboard.Clone();
                                MyStoryboard.Completed += delegate
                                {
                                    Animations.Remove(Element);
                                    if (Animations.Count == 0) internalCollection.Remove(Item);
                                };
                                MyStoryboard.Freeze();
                                Animations.Add(Element, MyStoryboard);
                            }
                        }
                    }

                    if (Animations.Count > 0)
                        Animations.ForEach(_ => _.Value.Begin((FrameworkElement) _.Key));
                    else
                        internalCollection.Remove(e.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new InvalidOperationException("Collection Wrapper extension currently supports no replace");
                case NotifyCollectionChangedAction.Move:
                    internalCollection.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    internalCollection.Clear();
                    originalCollection.ForEach(item => internalCollection.Add(item));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool GetIsFadeAnimationEnabled(ItemsControl itemsControl)
        {
            if (itemsControl.ItemsSource is ICollectionView)
                return ((ICollectionView) itemsControl.ItemsSource).SourceCollection == this &&
                       CrossThreadCollectionWrapper.GetEnableItemFadeAnimations(itemsControl);
            return itemsControl.ItemsSource == this &&
                   CrossThreadCollectionWrapper.GetEnableItemFadeAnimations(itemsControl);
        }

        #region Nested type: NotifyCollectionChangedDelegate

        private delegate void NotifyCollectionChangedDelegate(NotifyCollectionChangedEventArgs e);

        #endregion
    }
}