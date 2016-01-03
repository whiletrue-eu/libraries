using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace WhileTrue.Classes.ATR.Tokenized
{
    public class AtrInterfaceByteGroupTokenCollection : INotifyCollectionChanged, IEnumerable<AtrInterfaceByteGroupToken>
    {
        private readonly TokenizedAtr owner;
        private readonly ObservableCollection<AtrInterfaceByteGroupToken> collection = new ObservableCollection<AtrInterfaceByteGroupToken>(); 

        public AtrInterfaceByteGroupTokenCollection(TokenizedAtr owner)
        {
            this.owner = owner;
            ((INotifyCollectionChanged) this.collection).CollectionChanged += this.OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged(this, e);
        }

        public void AppendGroup(AtrInterfaceByteGroupToken atrInterfaceByteGroupToken, InterfaceByteGroupType type)
        {
            AtrInterfaceByteGroupToken NewGroup;
            int InsertIndex;
            if (atrInterfaceByteGroupToken != null)
            {
                NewGroup = new AtrInterfaceByteGroupToken(this.owner, null, new NextInterfaceBytesIndicator(type));
                NewGroup.NextGroup = atrInterfaceByteGroupToken.NextGroup;
                atrInterfaceByteGroupToken.NextGroup = NewGroup;
                atrInterfaceByteGroupToken.InterfaceBytesIndicator.TdExists = true;
                InsertIndex = this.collection.IndexOf(atrInterfaceByteGroupToken) + 1;
            }
            else
            {
                NewGroup = new AtrInterfaceByteGroupToken(this.owner, null, new NextInterfaceBytesIndicator(type));
                this.owner.Preamble.NextInterfaceBytesIndicator.TdExists = true;
                InsertIndex = 0;
            }
            this.collection.Insert(InsertIndex, NewGroup);
            this.owner.NotifyChanged();
        }

        public void AppendGroup(AtrInterfaceByteGroupToken atrInterfaceByteGroupToken)
        {
            AtrInterfaceByteGroupToken PreviousGroup = this.collection.LastOrDefault();
            if (PreviousGroup != null)
            {
                PreviousGroup.NextGroup = atrInterfaceByteGroupToken;
            }
            this.collection.Add(atrInterfaceByteGroupToken);
            //no notification; method is only used in constructor. This is not nice; better cleanup and do initialisation with array instead. nobody listens anyway to changes
        }

        public void Remove(AtrInterfaceByteGroupToken group)
        {
            AtrInterfaceByteGroupToken PreviousGroup = this.collection.TakeWhile(_ => _ != group).LastOrDefault();
            if (PreviousGroup != null)
            {
                PreviousGroup.NextGroup = group.NextGroup;
                PreviousGroup.InterfaceBytesIndicator.TdExists = group.NextGroup != null;
                this.collection.Remove(group);
                this.owner.NotifyChanged();
            }
            else
            {
                //group to remove is global group -> don't delete, even if no bytes are left. otherwise, other groups can not be created anymore
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged=delegate{};
        public int Count => this.collection.Count;

        public IEnumerator<AtrInterfaceByteGroupToken> GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}