using System;
using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class DataObjectStatusIndicatorAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTlvDataObjectStatusIndicator value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectStatusIndicatorAdapter, bool> includedInTlvAdapter;
        private static readonly PropertyAdapter<DataObjectStatusIndicatorAdapter, string> lifeCycleAdapter;
        private static readonly PropertyAdapter<DataObjectStatusIndicatorAdapter, EnumerationAdapter<StatusWordIndication>> statusWordIndicationAdapter;
        private static readonly PropertyAdapter<DataObjectStatusIndicatorAdapter, EnumerationAdapter<KnownLifeCycle>> lifeCycleInformationAdapter;
        private static readonly PropertyAdapter<DataObjectStatusIndicatorAdapter, string> statusWordAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectStatusIndicatorAdapter, EnumerationAdapter<StatusWordCoding>> statusWordCodingAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectStatusIndicatorAdapter, bool> isLifeCycleDefinedAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectStatusIndicatorAdapter, bool> isStatusWordDefinedAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectStatusIndicatorAdapter, bool> canUndefineLifeCycleAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectStatusIndicatorAdapter, bool> canUndefineStatusWordAdapter;

        static DataObjectStatusIndicatorAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectStatusIndicatorAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectStatusIndicatorAdapter>();

            DataObjectStatusIndicatorAdapter.includedInTlvAdapter = PropertyFactory.Create(
                nameof(DataObjectStatusIndicatorAdapter.IncludedInTlv),
                instance => instance.value.IncludedInTlv
                );

            DataObjectStatusIndicatorAdapter.lifeCycleAdapter = PropertyFactory.Create(
                nameof(DataObjectStatusIndicatorAdapter.LifeCycle),
                instance => instance.value.LifeCycle != null ? instance.value.LifeCycle.Value.ToString("X2") : null,
                (instance, value) => Helper.SetAsHexByteValue(value, _ => instance.value.LifeCycle = _)
                );

            DataObjectStatusIndicatorAdapter.isLifeCycleDefinedAdapter = PropertyFactory.Create(
                nameof(DataObjectStatusIndicatorAdapter.IsLifeCycleDefined),
                instance => instance.value.LifeCycle != null
                );

            DataObjectStatusIndicatorAdapter.canUndefineLifeCycleAdapter = PropertyFactory.Create(
                nameof(DataObjectStatusIndicatorAdapter.CanUndefineLifeCycle),
                instance => instance.value.CanUndefineLifeCycle
                );
            DataObjectStatusIndicatorAdapter.lifeCycleInformationAdapter = PropertyFactory.Create(
                nameof(DataObjectStatusIndicatorAdapter.LifeCycleInformation),
                instance => EnumerationAdapter<KnownLifeCycle>.GetInstanceFor(instance.value.LifeCycleInformation),
                (instance, value) => instance.value.LifeCycleInformation = value
                );

            DataObjectStatusIndicatorAdapter.statusWordIndicationAdapter = PropertyFactory.Create(
                nameof(DataObjectStatusIndicatorAdapter.StatusWordIndication),
                instance => EnumerationAdapter<StatusWordIndication>.GetInstanceFor(instance.value.StatusWordIndication),
                (instance,value) => instance.value.StatusWordIndication=value
                );

            DataObjectStatusIndicatorAdapter.statusWordAdapter = PropertyFactory.Create(
                nameof(DataObjectStatusIndicatorAdapter.StatusWord),
                instance => instance.value.StatusWord != null ? instance.value.StatusWord.Value.ToString("X4") : null,
                (instance,value) => Helper.SetAsHexUShortValue(value,_=>instance.value.StatusWord=_)
                );

            DataObjectStatusIndicatorAdapter.statusWordCodingAdapter = PropertyFactory.Create(
                nameof(DataObjectStatusIndicatorAdapter.StatusWordCoding),
                instance => EnumerationAdapter<StatusWordCoding>.GetInstanceFor(instance.value.IncludedInTlv?Classes.ATR.StatusWordCoding.WithinTlvData : Classes.ATR.StatusWordCoding.FollowingTlvData)
                );

            DataObjectStatusIndicatorAdapter.isStatusWordDefinedAdapter = PropertyFactory.Create(
                nameof(DataObjectStatusIndicatorAdapter.IsStatusWordDefined),
                instance => instance.value.StatusWordIndication != null
                );

            DataObjectStatusIndicatorAdapter.canUndefineStatusWordAdapter = PropertyFactory.Create(
                nameof(DataObjectStatusIndicatorAdapter.CanUndefineStatusWord),
                instance => instance.value.CanUndefineStatusWordIndication
                );
        }

        public string StatusWord 
        {
            get { return DataObjectStatusIndicatorAdapter.statusWordAdapter.GetValue(this); }
            set
            {
                if (value != this.StatusWordIndication.Description)
                {
                    DataObjectStatusIndicatorAdapter.statusWordAdapter.SetValue(this, value);
                }//workaround for combobox seting the selected enumeration value as string when user selects something. We ignore that to display th underlying ushort value
            }
        }

        public bool IsStatusWordDefined
        {
            get { return DataObjectStatusIndicatorAdapter.isStatusWordDefinedAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.value.StatusWordIndication = Classes.ATR.StatusWordIndication.NormalProcessing;
                }
                else
                {
                    this.value.StatusWordIndication = null;
                }

            }
        }

        public bool CanUndefineStatusWord => DataObjectStatusIndicatorAdapter.canUndefineStatusWordAdapter.GetValue(this);

        public string LifeCycle
        {
            get { return DataObjectStatusIndicatorAdapter.lifeCycleAdapter.GetValue(this); }
            set
            {
                if (value != this.LifeCycleInformation.Description)
                {
                    DataObjectStatusIndicatorAdapter.lifeCycleAdapter.SetValue(this, value);
                }//workaround for combobox seting the selected enumeration value as string when user selects something. We ignore that to display th underlying ushort value
            }
        }

        public bool IsLifeCycleDefined
        {
            get { return DataObjectStatusIndicatorAdapter.isLifeCycleDefinedAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.value.LifeCycle = 0x00;
                }
                else
                {
                    this.value.LifeCycle = null;
                }

            }
        }

        public bool CanUndefineLifeCycle => DataObjectStatusIndicatorAdapter.canUndefineLifeCycleAdapter.GetValue(this);

        public EnumerationAdapter<StatusWordIndication> StatusWordIndication
        {
            get { return DataObjectStatusIndicatorAdapter.statusWordIndicationAdapter.GetValue(this); }
            set { DataObjectStatusIndicatorAdapter.statusWordIndicationAdapter.SetValue(this, value); }
        }

        public IEnumerable<EnumerationAdapter<StatusWordIndication>> StatusWordIndicationValues
        {
            get { return EnumerationAdapter<StatusWordIndication>.Items.Where(_=>_.Value != Classes.ATR.StatusWordIndication.Rfu); }
        }

        public EnumerationAdapter<KnownLifeCycle> LifeCycleInformation
        {
            get { return DataObjectStatusIndicatorAdapter.lifeCycleInformationAdapter.GetValue(this); }
            set { DataObjectStatusIndicatorAdapter.lifeCycleInformationAdapter.SetValue(this, value); }
        }

        public IEnumerable<EnumerationAdapter<KnownLifeCycle>> KnownLifeCycleValues
        {
            get { return EnumerationAdapter<KnownLifeCycle>.Items.Where(_ => _.Value != Classes.ATR.KnownLifeCycle.Rfu); }
        }

        public EnumerationAdapter<StatusWordCoding> StatusWordCoding
        {
            get { return DataObjectStatusIndicatorAdapter.statusWordCodingAdapter.GetValue(this); }
            set
            {
                switch(value.Value)
                {
                    case Classes.ATR.StatusWordCoding.WithinTlvData:
                        this.value.IncludedInTlv = true;
                        break;
                    case Classes.ATR.StatusWordCoding.FollowingTlvData:
                        this.value.IncludedInTlv = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public IEnumerable<EnumerationAdapter<StatusWordCoding>> StatusWordCodingValues => EnumerationAdapter<StatusWordCoding>.Items;

        public bool IncludedInTlv => DataObjectStatusIndicatorAdapter.includedInTlvAdapter.GetValue(this);

        public DataObjectStatusIndicatorAdapter(CompactTlvDataObjectStatusIndicator value)
            : base(value)
        {
            this.value = value;
        }
    }
}