using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Implements a readonly collection that implements <c>INotifyCollectionChanged</c>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicAPI]
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverriden.Global")]
    public class ObservableReadOnlyCollection<T> : ObservableObject, IEnumerable<T>, INotifyCollectionChanged
    {
        /// <summary/>
        public ObservableReadOnlyCollection()
        {
            this.InnerList = new ObservableCollection<T>();
            this.InnerList.CollectionChanged += this.innerList_CollectionChanged;
        }

        /// <summary>
        /// Innere list that stores the values
        /// </summary>
        protected ObservableCollection<T> InnerList { get; }

        /// <summary>
        /// Gets the number of elements actually contained in the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements actually contained in the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
        /// </returns>
        public int Count => this.InnerList.Count;

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.-or-<paramref name="index"/> is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>. </exception>
        public virtual T this[int index] => this.InnerList[index];

        #region IEnumerable<ComponentType> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return this.InnerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InnerList.GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate{};

        #endregion

        private void innerList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged(this, e);
            if( e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Reset )
            {
                this.InvokePropertyChanged(nameof(ObservableReadOnlyCollection<T>.Count));
            }
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.ObjectModel.Collection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.ObjectModel.Collection`1"/>. The value can be null for reference types.</param>
        public bool Contains(T item)
        {
            return this.InnerList.Contains(item);
        }
    }
}