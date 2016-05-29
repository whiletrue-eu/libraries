using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
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
            ObservableObject.IPropertyAdapterFactory<AtrInterfaceByteGroupTokenAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrInterfaceByteGroupTokenAdapter>();
            AtrInterfaceByteGroupTokenAdapter.groupNumberAdatper = PropertyFactory.Create(
                nameof(AtrInterfaceByteGroupTokenAdapter.GroupNumber),
                instance => instance.atrInterfaceByteGroupToken.Number
                );
            AtrInterfaceByteGroupTokenAdapter.typeAdatper = PropertyFactory.Create(
                nameof(AtrInterfaceByteGroupTokenAdapter.Type),
                instance => EnumerationAdapter<InterfaceByteGroupType>.GetInstanceFor(instance.atrInterfaceByteGroupToken.Type)
                );
            AtrInterfaceByteGroupTokenAdapter.taAdapter = PropertyFactory.Create(
                nameof(AtrInterfaceByteGroupTokenAdapter.Ta),
                instance => instance.atrInterfaceByteGroupToken.Ta.HasValue ? instance.atrInterfaceByteGroupToken.Ta.Value.ToHexString() : null,
                (instance, value) => instance.atrInterfaceByteGroupToken.Ta = !string.IsNullOrEmpty(value)?byte.Parse(value.Substring(0,Math.Min(value.Length,2)), NumberStyles.HexNumber):(byte?)null
                );
            AtrInterfaceByteGroupTokenAdapter.tbAdapter = PropertyFactory.Create(
                nameof(AtrInterfaceByteGroupTokenAdapter.Tb),
                instance => instance.atrInterfaceByteGroupToken.Tb.HasValue ? instance.atrInterfaceByteGroupToken.Tb.Value.ToHexString() : null,
                (instance, value) => instance.atrInterfaceByteGroupToken.Tb = !string.IsNullOrEmpty(value) ? byte.Parse(value.Substring(0, Math.Min(value.Length, 2)), NumberStyles.HexNumber) : (byte?)null
                );
            AtrInterfaceByteGroupTokenAdapter.tcAdapter = PropertyFactory.Create(
                nameof(AtrInterfaceByteGroupTokenAdapter.Tc),
                instance => instance.atrInterfaceByteGroupToken.Tc.HasValue ? instance.atrInterfaceByteGroupToken.Tc.Value.ToHexString() : null,
                (instance, value) => instance.atrInterfaceByteGroupToken.Tc = !string.IsNullOrEmpty(value) ? byte.Parse(value.Substring(0, Math.Min(value.Length, 2)), NumberStyles.HexNumber) : (byte?)null
                );
            AtrInterfaceByteGroupTokenAdapter.nextGroupTypeAdapter = PropertyFactory.Create(
                nameof(AtrInterfaceByteGroupTokenAdapter.NextGroupType),
                instance => EnumerationAdapter<InterfaceByteGroupType>.GetInstanceFor(instance.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator!=null?instance.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.GroupType:(InterfaceByteGroupType?)null)
                );
            AtrInterfaceByteGroupTokenAdapter.nextBytesTypeAdapter = PropertyFactory.Create(
                nameof(AtrInterfaceByteGroupTokenAdapter.NextBytes),
                instance => instance.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator!=null?AtrTokenAdapterBase.ToString(instance.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.TaExists, instance.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.TbExists, instance.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.TcExists, instance.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.TdExists):"not set"
                );

            AtrInterfaceByteGroupTokenAdapter.possibleTypesToChangeToAdapter = PropertyFactory.Create(
                nameof(AtrInterfaceByteGroupTokenAdapter.PossibleTypesToChangeTo),
                instance => ((InterfaceByteGroupType[])Enum.GetValues(typeof(InterfaceByteGroupType)))
                    .Where(_=>_!=InterfaceByteGroupType.Global)//gloabl cannot be set; the first group automatically is global
                    .Where(_ => instance.atrInterfaceByteGroupToken.NextGroup != null) //nothing cannot be set if no next group
                    .Where(_=> _ != instance.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.GroupType) //we can skip what is already set
                    .OrderBy(_=>(int)_) //Sort by id
                    .SkipWhile(value => value < instance.atrInterfaceByteGroupToken.Type) //Next Group cannot be less than this group. Same is OK
                    .TakeWhile(value => instance.atrInterfaceByteGroupToken.NextGroup.NextInterfaceBytesIndicator == null || value <= instance.atrInterfaceByteGroupToken.NextGroup.NextInterfaceBytesIndicator.GroupType), //next group cannot be higher than next to next group. Same is OK
                (instance, item) => EnumerationAdapter<InterfaceByteGroupType>.GetInstanceFor(item)
                );
            AtrInterfaceByteGroupTokenAdapter.possibleTypesToAddNextGroupAdapter = PropertyFactory.Create(
                nameof(AtrInterfaceByteGroupTokenAdapter.PossibleTypesToAddNextGroup),
                instance => ((InterfaceByteGroupType[])Enum.GetValues(typeof(InterfaceByteGroupType)))
                    .Where(_ => _ != InterfaceByteGroupType.Global)//gloabl cannot be set; the first group automatically is global
                    .OrderBy(_ => (int)_) //Sort by id
                    .SkipWhile(value => value < instance.atrInterfaceByteGroupToken.Type) //Next Group cannot be less than this group. Same is OK
                    .TakeWhile(value => instance.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator == null || value <= instance.atrInterfaceByteGroupToken.NextInterfaceBytesIndicator.GroupType), //next group cannot be higher than next to next group. Same is OK
                (instance, item) => EnumerationAdapter<InterfaceByteGroupType>.GetInstanceFor(item)
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

        public ICommand SetNextGroupTypeCommand => this.setNextGroupTypeCommand;

        public ICommand AddNextGroupTypeCommand => this.addNextGroupTypeCommand;

        public ICommand RemoveNextGroupCommand => this.removeNextGroupCommand;

        public ICommand ClearTaCommand => this.clearTaCommand;

        private void ClearTa()
        {
            this.Ta = null;
        }

        public ICommand ClearTbCommand => this.clearTbCommand;

        private void ClearTb()
        {
            this.Tb = null;
        }

        public ICommand ClearTcCommand => this.clearTcCommand;

        private void ClearTc()
        {
            this.Tc = null;
        }

        public IEnumerable<EnumerationAdapter<InterfaceByteGroupType>> PossibleTypesToChangeTo => AtrInterfaceByteGroupTokenAdapter.possibleTypesToChangeToAdapter.GetCollection(this);

        public IEnumerable<EnumerationAdapter<InterfaceByteGroupType>> PossibleTypesToAddNextGroup => AtrInterfaceByteGroupTokenAdapter.possibleTypesToAddNextGroupAdapter.GetCollection(this);

        public string NextBytes => AtrInterfaceByteGroupTokenAdapter.nextBytesTypeAdapter.GetValue(this);

        public EnumerationAdapter<InterfaceByteGroupType> NextGroupType => AtrInterfaceByteGroupTokenAdapter.nextGroupTypeAdapter.GetValue(this);

        public EnumerationAdapter<InterfaceByteGroupType> Type => AtrInterfaceByteGroupTokenAdapter.typeAdatper.GetValue(this);

        public string Ta
        {
            get { return AtrInterfaceByteGroupTokenAdapter.taAdapter.GetValue(this); }
            set { AtrInterfaceByteGroupTokenAdapter.taAdapter.SetValue(this,value); }
        }

        public string Tb
        {
            get { return AtrInterfaceByteGroupTokenAdapter.tbAdapter.GetValue(this); }
            set { AtrInterfaceByteGroupTokenAdapter.tbAdapter.SetValue(this, value); }

        }

        public string Tc
        {
            get { return AtrInterfaceByteGroupTokenAdapter.tcAdapter.GetValue(this); }
            set { AtrInterfaceByteGroupTokenAdapter.tcAdapter.SetValue(this, value); }

        }

        public int GroupNumber => AtrInterfaceByteGroupTokenAdapter.groupNumberAdatper.GetValue(this);
    }
}