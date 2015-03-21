using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class AtrInterfaceByteGroupTokenAdapter : AtrTokenAdapterBase
    {
        private static readonly ReadOnlyPropertyAdapter<AtrInterfaceByteGroupTokenAdapter, EnumerationAdapter<InterfaceByteGroupType>> typeAdatper;
        private static readonly PropertyAdapter<AtrInterfaceByteGroupTokenAdapter, string> taAdapter;
        private static readonly PropertyAdapter<AtrInterfaceByteGroupTokenAdapter, string> tbAdapter;
        private static readonly PropertyAdapter<AtrInterfaceByteGroupTokenAdapter, string> tcAdapter;
        private static readonly ReadOnlyPropertyAdapter<AtrInterfaceByteGroupTokenAdapter, EnumerationAdapter<InterfaceByteGroupType>> nextGroupTypeAdapter;
        private static readonly ReadOnlyPropertyAdapter<AtrInterfaceByteGroupTokenAdapter, string> nextBytesTypeAdapter;
        private static readonly ReadOnlyPropertyAdapter<AtrInterfaceByteGroupTokenAdapter, int> groupNumberAdatper;
        private static readonly EnumerablePropertyAdapter<AtrInterfaceByteGroupTokenAdapter, InterfaceByteGroupType, EnumerationAdapter<InterfaceByteGroupType>> possibleTypesToChangeToAdapter;
        private static readonly EnumerablePropertyAdapter<AtrInterfaceByteGroupTokenAdapter, InterfaceByteGroupType, EnumerationAdapter<InterfaceByteGroupType>> possibleTypesToAddNextGroupAdapter;

        static AtrInterfaceByteGroupTokenAdapter()
        {
            IPropertyAdapterFactory<AtrInterfaceByteGroupTokenAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrInterfaceByteGroupTokenAdapter>();
            groupNumberAdatper = PropertyFactory.Create(
                @this => @this.GroupNumber,
                @this => @this.atrInterfaceByteGroupToken.Number
                );
            typeAdatper = PropertyFactory.Create(
                @this => @this.Type,
                @this => EnumerationAdapter<InterfaceByteGroupType>.GetInstanceFor(@this.atrInterfaceByteGroupToken.Type)
                );
            taAdapter = PropertyFactory.Create(
                @this => @this.Ta,
                @this => @this.atrInterfaceByteGroupToken.Ta.HasValue ? @this.atrInterfaceByteGroupToken.Ta.Value.ToHexString() : null,
                (@this, value) => @this.atrInterfaceByteGroupToken.Ta = !string.IsNullOrEmpty(value)?byte.Parse(value.Substring(0,Math.Min(value.Length,2)), NumberStyles.HexNumber):(byte?)null
                );
            tbAdapter = PropertyFactory.Create(
                @this => @this.Tb,
                @this => @this.atrInterfaceByteGroupToken.Tb.HasValue ? @this.atrInterfaceByteGroupToken.Tb.Value.ToHexString() : null,
                (@this, value) => @this.atrInterfaceByteGroupToken.Tb = !string.IsNullOrEmpty(value) ? byte.Parse(value.Substring(0, Math.Min(value.Length, 2)), NumberStyles.HexNumber) : (byte?)null
                );
            tcAdapter = PropertyFactory.Create(
                @this => @this.Tc,
                @this => @this.atrInterfaceByteGroupToken.Tc.HasValue ? @this.atrInterfaceByteGroupToken.Tc.Value.ToHexString() : null,
                (@this, value) => @this.atrInterfaceByteGroupToken.Tc = !string.IsNullOrEmpty(value) ? byte.Parse(value.Substring(0, Math.Min(value.Length, 2)), NumberStyles.HexNumber) : (byte?)null
                );
            nextGroupTypeAdapter = PropertyFactory.Create(
                @this => @this.NextGroupType,
                @this => EnumerationAdapter<InterfaceByteGroupType>.GetInstanceFor(@this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator!=null?@this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.GroupType:(InterfaceByteGroupType?)null)
                );
            nextBytesTypeAdapter = PropertyFactory.Create(
                @this => @this.NextBytes,
                @this => @this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator!=null?ToString(@this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.TaExists, @this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.TbExists, @this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.TcExists, @this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.TdExists):"not set"
                );

            possibleTypesToChangeToAdapter = PropertyFactory.Create(
                @this=>@this.PossibleTypesToChangeTo,
                @this => ((InterfaceByteGroupType[])Enum.GetValues(typeof(InterfaceByteGroupType)))
                    .Where(_=>_!=InterfaceByteGroupType.Global)//gloabl cannot be set; the first group automatically is global
                    .Where(_ => @this.atrInterfaceByteGroupToken.NextGroup != null) //nothing cannot be set if no next group
                    .Where(_=> _ != @this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.GroupType) //we can skip what is already set
                    .OrderBy(_=>(int)_) //Sort by id
                    .SkipWhile(value => value < @this.atrInterfaceByteGroupToken.Type) //Next Group cannot be less than this group. Same is OK
                    .TakeWhile(value => @this.atrInterfaceByteGroupToken.NextGroup.NextInterfaceBytesIndicator == null || value <= @this.atrInterfaceByteGroupToken.NextGroup.NextInterfaceBytesIndicator.GroupType), //next group cannot be higher than next to next group. Same is OK
                (@this, item) => EnumerationAdapter<InterfaceByteGroupType>.GetInstanceFor(item)
                );
            possibleTypesToAddNextGroupAdapter = PropertyFactory.Create(
                @this => @this.PossibleTypesToAddNextGroup,
                @this => ((InterfaceByteGroupType[])Enum.GetValues(typeof(InterfaceByteGroupType)))
                    .Where(_ => _ != InterfaceByteGroupType.Global)//gloabl cannot be set; the first group automatically is global
                    .OrderBy(_ => (int)_) //Sort by id
                    .SkipWhile(value => value < @this.atrInterfaceByteGroupToken.Type) //Next Group cannot be less than this group. Same is OK
                    .TakeWhile(value => @this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator == null || value <= @this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.GroupType), //next group cannot be higher than next to next group. Same is OK
                (@this, item) => EnumerationAdapter<InterfaceByteGroupType>.GetInstanceFor(item)
                );
        }

        private readonly AtrInterfaceByteGroupToken atrInterfaceByteGroupToken;
        private readonly DelegateCommand clearTaCommand;
        private readonly DelegateCommand clearTbCommand;
        private readonly DelegateCommand clearTcCommand;
        private readonly DelegateCommand<EnumerationAdapter<InterfaceByteGroupType>> setNextGroupTypeCommand;
        private readonly DelegateCommand<EnumerationAdapter<InterfaceByteGroupType>> addNextGroupTypeCommand;
        private readonly DelegateCommand removeNextGroupCommand;

        public AtrInterfaceByteGroupTokenAdapter(AtrInterfaceByteGroupToken atrInterfaceByteGroupToken) : base(atrInterfaceByteGroupToken)
        {
            this.atrInterfaceByteGroupToken = atrInterfaceByteGroupToken;

            this.clearTaCommand = new DelegateCommand(this.ClearTa,()=>this.atrInterfaceByteGroupToken.Ta.HasValue);
            this.clearTbCommand = new DelegateCommand(this.ClearTb, () => this.atrInterfaceByteGroupToken.Tb.HasValue);
            this.clearTcCommand = new DelegateCommand(this.ClearTc, () => this.atrInterfaceByteGroupToken.Tc.HasValue);
            this.setNextGroupTypeCommand = new DelegateCommand<EnumerationAdapter<InterfaceByteGroupType>>(this.SetNextGroupType);
            this.addNextGroupTypeCommand = new DelegateCommand<EnumerationAdapter<InterfaceByteGroupType>>(this.AddNextGroupType);
            this.removeNextGroupCommand = new DelegateCommand(this.RemoveNextGroup, ()=>this.atrInterfaceByteGroupToken.NextGroup!=null);
        }

        private void RemoveNextGroup()
        {
            this.atrInterfaceByteGroupToken.RemoveNextGroup();
        }

        private void AddNextGroupType(EnumerationAdapter<InterfaceByteGroupType> type)
        {
            this.atrInterfaceByteGroupToken.AddGroup(type);
        }

        private void SetNextGroupType(EnumerationAdapter<InterfaceByteGroupType> type)
        {
            this.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.GroupType = type;
        }

        public ICommand SetNextGroupTypeCommand
        {
            get { return this.setNextGroupTypeCommand; }
        }

        public ICommand AddNextGroupTypeCommand
        {
            get { return this.addNextGroupTypeCommand; }
        }

        public ICommand RemoveNextGroupCommand
        {
            get { return this.removeNextGroupCommand; }
        }

        public ICommand ClearTaCommand
        {
            get { return this.clearTaCommand; }
        }

        private void ClearTa()
        {
            this.Ta = null;
        }

        public ICommand ClearTbCommand
        {
            get { return this.clearTbCommand; }
        }

        private void ClearTb()
        {
            this.Tb = null;
        }

        public ICommand ClearTcCommand
        {
            get { return this.clearTcCommand; }
        }

        private void ClearTc()
        {
            this.Tc = null;
        }

        public IEnumerable<EnumerationAdapter<InterfaceByteGroupType>> PossibleTypesToChangeTo
        {
            get
            {
                return possibleTypesToChangeToAdapter.GetCollection(this);
            }
        }

        public IEnumerable<EnumerationAdapter<InterfaceByteGroupType>> PossibleTypesToAddNextGroup
        {
            get
            {
                return possibleTypesToAddNextGroupAdapter.GetCollection(this);
            }
        }

        public string NextBytes
        {
            get { return nextBytesTypeAdapter.GetValue(this); }
        }

        public EnumerationAdapter<InterfaceByteGroupType> NextGroupType
        {
            get { return nextGroupTypeAdapter.GetValue(this); }
        }

        public EnumerationAdapter<InterfaceByteGroupType> Type
        {
            get { return typeAdatper.GetValue(this); }
        }

        public string Ta
        {
            get { return taAdapter.GetValue(this); }
            set { taAdapter.SetValue(this,value); }
        }

        public string Tb
        {
            get { return tbAdapter.GetValue(this); }
            set { tbAdapter.SetValue(this, value); }

        }

        public string Tc
        {
            get { return tcAdapter.GetValue(this); }
            set { tcAdapter.SetValue(this, value); }

        }

        public int GroupNumber
        {
            get { return groupNumberAdatper.GetValue(this); }
        }
    }
}