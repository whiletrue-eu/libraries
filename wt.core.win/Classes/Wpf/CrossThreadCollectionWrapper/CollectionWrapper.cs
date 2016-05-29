using System;
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
        private static readonly Dictionary<Dispatcher, Dictionary<IEnumerable, CollectionWrapper>> collectionWrappers = new Dictionary<Dispatcher, Dictionary<IEnumerable, CollectionWrapper>>();

        private readonly ObservableCollection<object> internalCollection = new ObservableCollection<object>();
        private readonly Dispatcher dispatcher;
        private readonly IEnumerable originalCollection;
        internal static List<ItemsControl> RegisteredControls = new List<ItemsControl>();

        private CollectionWrapper(IEnumerable collection, Dispatcher dispatcher)
        {
            ((INotifyCollectionChanged) collection).CollectionChanged += this.CollectionWrapper_CollectionChanged;
            this.dispatcher = dispatcher;
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

        public static CollectionWrapper GetCollectionWrapperInstance(IEnumerable collection, bool shareCollectionPerThread)
        {
            if (shareCollectionPerThread)
            {
                lock (CollectionWrapper.collectionWrappers)
                {
                    Dispatcher CurrentDispatcher = Dispatcher.CurrentDispatcher;
                    if (CollectionWrapper.collectionWrappers.ContainsKey(CurrentDispatcher))
                    {
                        return CollectionWrapper.GetCollectionWrapperInstance(CollectionWrapper.collectionWrappers[CurrentDispatcher], collection,
                                                            CurrentDispatcher);
                    }
                    else
                    {
                        Dictionary<IEnumerable, CollectionWrapper> CollectionWrappers =
                            new Dictionary<IEnumerable, CollectionWrapper>();
                        CollectionWrapper.collectionWrappers.Add(CurrentDispatcher, CollectionWrappers);
                        return CollectionWrapper.GetCollectionWrapperInstance(CollectionWrappers, collection, CurrentDispatcher);
                    }
                }
            }
            else
            {
                return new CollectionWrapper(collection, Dispatcher.CurrentDispatcher);
            }
        }

        private static CollectionWrapper GetCollectionWrapperInstance(IDictionary<IEnumerable, CollectionWrapper> collectionWrappers, IEnumerable collection, Dispatcher dispatcher)
        {
            lock (collectionWrappers)
            {
                if (collectionWrappers.ContainsKey(collection))
                {
                    return collectionWrappers[collection];
                }
                else
                {
                    CollectionWrapper Wrapper = new CollectionWrapper(collection, dispatcher);
                    collectionWrappers.Add(collection, Wrapper);
                    return Wrapper;
                }
            }
        }

        private void CollectionWrapper_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.dispatcher.BeginInvoke(DispatcherPriority.DataBind, new NotifyCollectionChangedDelegate(this.NotifyCollectionChanged), e);
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
                    foreach (ItemsControl ItemsControl in CollectionWrapper.RegisteredControls.Where(this.GetIsFadeAnimationEnabled))
                    {
                        UIElement Element = ItemsControl.ItemContainerGenerator.ContainerFromItem(e.NewItems[0]) as UIElement;
                        if (Element != null)
                        {
                            Storyboard Storyboard = CrossThreadCollectionWrapper.GetFadeInAnimation(Element);
                            if (Storyboard != null)
                            {
                                Storyboard MyStoryboard = Storyboard.Clone();
                                MyStoryboard.Freeze();
                                MyStoryboard.Begin((FrameworkElement) Element);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Dictionary<UIElement, Storyboard> Animations = new Dictionary<UIElement, Storyboard>();
                    object Item = e.OldItems[0];
                    foreach (ItemsControl ItemsControl in CollectionWrapper.RegisteredControls.Where(this.GetIsFadeAnimationEnabled))
                    {
                        UIElement Element = ItemsControl.ItemContainerGenerator.ContainerFromItem(Item) as UIElement;
                        if (Element != null)
                        {
                            Storyboard Storyboard = CrossThreadCollectionWrapper.GetFadeOutAnimation(Element);
                            if (Storyboard != null)
                            {
                                Storyboard MyStoryboard = Storyboard.Clone();
                                MyStoryboard.Completed += delegate
                                    {
                                        Animations.Remove(Element);
                                        if (Animations.Count == 0)
                                        {
                                            this.internalCollection.Remove(Item);
                                        }
                                    };
                                MyStoryboard.Freeze();
                                Animations.Add(Element, MyStoryboard);
                            }
                        }
                    }
                    if (Animations.Count > 0)
                    {
                        Animations.ForEach(_ => _.Value.Begin((FrameworkElement)_.Key));
                    }
                    else
                    {
                        this.internalCollection.Remove(e.OldItems[0]);
                    }
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

        private bool GetIsFadeAnimationEnabled(ItemsControl itemsControl)
        {
            if (itemsControl.ItemsSource is ICollectionView)
            {
                return ((ICollectionView) itemsControl.ItemsSource).SourceCollection == this &&
                       CrossThreadCollectionWrapper.GetEnableItemFadeAnimations(itemsControl);
            }
            else
            {
                return itemsControl.ItemsSource == this &&
                       CrossThreadCollectionWrapper.GetEnableItemFadeAnimations(itemsControl);
            }
        }

        #region Nested type: NotifyCollectionChangedDelegate

        private delegate void NotifyCollectionChangedDelegate(NotifyCollectionChangedEventArgs e);

        #endregion
    }
}