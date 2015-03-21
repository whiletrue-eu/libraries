using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace WhileTrue.Classes.Framework
{
    public class ObservableReadOnlyCollection<Type> : ObservableObject, IEnumerable<Type>, INotifyCollectionChanged
    {
        private readonly ObservableCollection<Type> innerList;

        public ObservableReadOnlyCollection()
        {
            this.innerList = new ObservableCollection<Type>();
            this.innerList.CollectionChanged += this.innerList_CollectionChanged;
        }

        protected ObservableCollection<Type> InnerList
        {
            get { return this.innerList; }
        }

        public int Count
        {
            get { return this.InnerList.Count; }
        }

        public virtual Type this[int index]
        {
            get { return this.InnerList[index]; }
        }

        #region IEnumerable<ComponentType> Members

        public IEnumerator<Type> GetEnumerator()
        {
            return this.InnerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InnerList.GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate{};

        #endregion

        private void innerList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged(this, e);
            if( e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Reset )
            {
                this.InvokePropertyChanged(()=>Count);
            }
        }

        public bool Contains(Type item)
        {
            return this.innerList.Contains(item);
        }
    }
}