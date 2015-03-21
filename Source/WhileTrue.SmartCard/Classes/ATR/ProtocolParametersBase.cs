using System;
using System.Linq;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class ProtocolParametersBase : AtrParametersBase
    {
        private readonly ReadOnlyPropertyAdapter<bool> isOnlyIndicatedProtocolAdapter;
        public ProtocolType ProtocolType { get; private set; }

        internal ProtocolParametersBase(Atr owner, ProtocolType protocolType, Func<AtrInterfaceByteGroupToken, bool> groupTypeSelection ) 
            : base(owner, groupTypeSelection, (InterfaceByteGroupType) protocolType)
        {
            this.ProtocolType = protocolType;
            isOnlyIndicatedProtocolAdapter = this.CreatePropertyAdapter(
                () => IsOnlyIndicatedProtocol,
                () => this.owner.ProtocolParameters.Count() == 1
                );
        }

        public bool IsOnlyIndicatedProtocol 
        {
            get { return isOnlyIndicatedProtocolAdapter.GetValue(); }
        }

        public void RemoveIndication()
        {
            DbC.Assure(this.IsOnlyIndicatedProtocol==false,"Last and only protocol inidication cannot be removed");

            foreach (AtrInterfaceByteGroupToken Group in this.owner.TokenizedAtr.InterfaceByteGroups.ToArray())
            {
                if (Group.Type == (InterfaceByteGroupType) this.ProtocolType)
                {
                    if (Group.Number == 2)
                    {
                        //Special handling for group#2: instead of removal, type indication ahs to be changed to next protocol
                        this.owner.TokenizedAtr.InterfaceByteGroups.ElementAt(0).NextInterfaceBytesIndicator.GroupType = this.owner.TokenizedAtr.InterfaceByteGroups.Skip(2).First(_ => _.Type != (InterfaceByteGroupType) this.ProtocolType).Type;
                    }
                    else
                    {
                        this.owner.TokenizedAtr.InterfaceByteGroups.Remove(Group);
                    }
                }
            }
            this.RemoveUnusedGroups();
        }

        internal void AddIndication()
        {
            if (this.ProtocolType != ProtocolType.T0 && this.owner.TokenizedAtr.InterfaceByteGroups.ElementAt(0).NextInterfaceBytesIndicator == null)
            {
                //Special Handling: If T=0 is only indicated implicitely, add it's indication before another indication is set to preserve T=0
                this.owner.TokenizedAtr.InterfaceByteGroups.Last().AddGroup(InterfaceByteGroupType.T0);
            }
            this.owner.TokenizedAtr.InterfaceByteGroups.TakeWhile(_=>_.Type<(InterfaceByteGroupType) this.ProtocolType).Last().AddGroup((InterfaceByteGroupType) this.ProtocolType);
            this.RemoveUnusedGroups();
        }
    }
}