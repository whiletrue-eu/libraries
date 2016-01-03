using System;
using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.ATR
{

    public abstract class AtrParametersBase:ObservableObject
    {
        protected readonly Atr Owner;
        private readonly Func<AtrInterfaceByteGroupToken, bool> groupTypeSelection;
        private readonly InterfaceByteGroupType groupType;

        public virtual bool IsApplicable
        {
            get
            {
                return this.Owner.TokenizedAtr.InterfaceByteGroups.Any(_=>_.Type == this.groupType);
            }
        }

        public virtual IEnumerable<ParameterByte> ParameterBytes
        {
            get
            {
                foreach (AtrInterfaceByteGroupToken InterfaceByteGroup in this.GetInterfaceGroups())
                {
                    yield return InterfaceByteGroup.Ta;
                    yield return InterfaceByteGroup.Tb;
                    yield return InterfaceByteGroup.Tc;
                }
            }
        }

        protected enum InterfaceByteType
        {
            Ta,
            Tb,
            Tc
        }

        protected AtrParametersBase(Atr owner, Func<AtrInterfaceByteGroupToken, bool> groupTypeSelection, InterfaceByteGroupType groupType)
        {
            this.Owner = owner;
            this.groupTypeSelection = groupTypeSelection;
            this.groupType = groupType;
        }


        protected IEnumerable<AtrInterfaceByteGroupToken> GetInterfaceGroups()
        {
            return this.Owner.TokenizedAtr.InterfaceByteGroups.Where(this.groupTypeSelection);
        }

        protected byte? GetInterfaceByte( int groupNo, InterfaceByteType type)
        {
            return AtrParametersBase.GetInterfaceByte(groupNo, type, this.GetInterfaceGroups().ToArray());
        }

        private static byte? GetInterfaceByte(int groupNo, InterfaceByteType type, AtrInterfaceByteGroupToken[] groups)
        {
            if (groups.Length > groupNo)
            {
                AtrInterfaceByteGroupToken InterfaceByteGroup = groups[groupNo];
                switch (type)
                {
                    case InterfaceByteType.Ta:
                        return InterfaceByteGroup.Ta;
                    case InterfaceByteType.Tb:
                        return InterfaceByteGroup.Tb;
                    case InterfaceByteType.Tc:
                        return InterfaceByteGroup.Tc;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type));
                }
            }
            else
            {
                return null;
            }
        }

        protected byte? GetInterfaceByte(InterfaceByteGroupType groupType, int groupNo, InterfaceByteType type)
        {
            return AtrParametersBase.GetInterfaceByte(groupNo, type, this.GetInterfaceGroups().Where(_=>_.Type == groupType).ToArray());
        }

        protected void SetInterfaceByte(int groupNo, InterfaceByteType type, byte? value)
        {
            this.EnsureGroupNumberExists(groupNo, _=>true);
            this.SetInterfaceByte(groupNo, type, this.GetInterfaceGroups().ToArray(), value);
        }

        protected void SetInterfaceByte(InterfaceByteGroupType groupType, int groupNo, InterfaceByteType type, byte? value)
        {
            this.EnsureGroupNumberExists(groupNo, _ => _.Type == groupType);
            this.SetInterfaceByte(groupNo, type, this.GetInterfaceGroups().Where(_ => _.Type == groupType).ToArray(), value);
        }

        private void SetInterfaceByte(int groupNo, InterfaceByteType type, AtrInterfaceByteGroupToken[] groups, byte? value)
        {
            if (groups.Length > groupNo)
            {
                AtrInterfaceByteGroupToken InterfaceByteGroup = groups[groupNo];
                switch (type)
                {
                    case InterfaceByteType.Ta:
                        InterfaceByteGroup.Ta=value;
                        break;
                    case InterfaceByteType.Tb:
                        InterfaceByteGroup.Tb = value;
                        break;
                    case InterfaceByteType.Tc:
                        InterfaceByteGroup.Tc = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type));
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(groupNo));
            }
            this.RemoveUnusedGroups();
        }

        private void EnsureGroupNumberExists(int groupNumber, Func<AtrInterfaceByteGroupToken, bool> filter)
        {
            if (this.Owner.TokenizedAtr.InterfaceByteGroups.Count == 0)
            {
                //First group is always global; should not matter, global is always there
                this.Owner.TokenizedAtr.InterfaceByteGroups.AppendGroup(null, InterfaceByteGroupType.Global);
            }

            if (this.Owner.TokenizedAtr.InterfaceByteGroups.Count == 1)
            {
                //Second group must be T=0, because if no group is there yet, T=0 is indicated by default
                this.Owner.TokenizedAtr.InterfaceByteGroups.ElementAt(0).AddGroup(InterfaceByteGroupType.T0);
            }
            while (this.GetInterfaceGroups().Where(filter).Count() <= groupNumber)
            {
                AtrInterfaceByteGroupToken LastDefinedExistingGroup = this.GetInterfaceGroups().LastOrDefault();
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (LastDefinedExistingGroup == null)
                {
                    //first group has to be created. append to last group with lower type
                    this.Owner.TokenizedAtr.InterfaceByteGroups.AppendGroup(this.Owner.TokenizedAtr.InterfaceByteGroups.TakeWhile(_ => _.Type < this.groupType).Last(), this.groupType);
                }
                else
                {
                    //groups already exist. append to last existing group
                    this.Owner.TokenizedAtr.InterfaceByteGroups.AppendGroup(LastDefinedExistingGroup, this.groupType);
                }
            }
        }

        protected void RemoveUnusedGroups()
        {
            //search in reversed order and cached so that we can alter the collection
            IGrouping<InterfaceByteGroupType, AtrInterfaceByteGroupToken>[] GroupsByType = this.Owner.TokenizedAtr.InterfaceByteGroups.Reverse().GroupBy(_=>_.Type).ToArray();
            foreach( IGrouping<InterfaceByteGroupType, AtrInterfaceByteGroupToken> Groups in GroupsByType)
            {
                if (Groups.Key != InterfaceByteGroupType.Global)
                {
                    AtrInterfaceByteGroupToken LastGroup = Groups.LastOrDefault();
                    //leave the first group even if its empty because it is indicating the protocol,
                    //except groups is T=15 / extended global as it is not indicating anything
                    //or if the last existing group (except global) is T=0
                    foreach (AtrInterfaceByteGroupToken Group in Groups.Where( _=>_ != LastGroup ||
                                                                                  // ReSharper disable once PossibleNullReferenceException
                                                                                       _.Type == InterfaceByteGroupType.GlobalExtended ||
                                                                                       (_.Type == InterfaceByteGroupType.T0 && this.Owner.TokenizedAtr.InterfaceByteGroups.Count==2))
                        )
                    {
                        if (Group.InterfaceBytesIndicator.TaExists == false &&
                            Group.InterfaceBytesIndicator.TbExists == false &&
                            Group.InterfaceBytesIndicator.TcExists == false )
                        {
                            this.Owner.TokenizedAtr.InterfaceByteGroups.Remove(Group);
                        }
                        else
                        {
                            //stop removing empty groups when one non-empty is found because otherwise the
                            //byte index of defined bytes is altered
                            break;
                        }
                    }
                }
                else
                {
                    //leave global as it is; it is never deleted
                }
            }
        }

        public virtual void NotifyAtrChanged()
        {
            this.InvokePropertyChanged(nameof(AtrParametersBase.ParameterBytes));
        }
    }
}