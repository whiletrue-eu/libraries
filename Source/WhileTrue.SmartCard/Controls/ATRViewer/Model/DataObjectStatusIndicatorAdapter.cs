using System;
using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class DataObjectStatusIndicatorAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTLVDataObjectStatusIndicator value;
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

            includedInTlvAdapter = PropertyFactory.Create(
                @this => @this.IncludedInTlv,
                @this => @this.value.IncludedInTlv
                );

            lifeCycleAdapter = PropertyFactory.Create(
                @this => @this.LifeCycle,
                @this => @this.value.LifeCycle != null ? @this.value.LifeCycle.Value.ToString("X2") : null,
                (@this, value) => Helper.SetAsHexByteValue(value, _ => @this.value.LifeCycle = _)
                );

            isLifeCycleDefinedAdapter = PropertyFactory.Create(
                @this => @this.IsLifeCycleDefined,
                @this => @this.value.LifeCycle != null
                );

            canUndefineLifeCycleAdapter = PropertyFactory.Create(
                @this => @this.CanUndefineLifeCycle,
                @this => @this.value.CanUndefineLifeCycle
                );
            lifeCycleInformationAdapter = PropertyFactory.Create(
                @this => @this.LifeCycleInformation,
                @this => EnumerationAdapter<KnownLifeCycle>.GetInstanceFor(@this.value.LifeCycleInformation),
                (@this, value) => @this.value.LifeCycleInformation = value
                );

            statusWordIndicationAdapter = PropertyFactory.Create(
                @this => @this.StatusWordIndication,
                @this => EnumerationAdapter<StatusWordIndication>.GetInstanceFor(@this.value.StatusWordIndication),
                (@this,value) => @this.value.StatusWordIndication=value
                );

            statusWordAdapter = PropertyFactory.Create(
                @this => @this.StatusWord,
                @this => @this.value.StatusWord != null ? @this.value.StatusWord.Value.ToString("X4") : null,
                (@this,value) => Helper.SetAsHexUShortValue(value,_=>@this.value.StatusWord=_)
                );

            statusWordCodingAdapter = PropertyFactory.Create(
                @this => @this.StatusWordCoding,
                @this => EnumerationAdapter<StatusWordCoding>.GetInstanceFor(@this.value.IncludedInTlv?Classes.ATR.StatusWordCoding.WithinTlvData : Classes.ATR.StatusWordCoding.FollowingTlvData)
                );

            isStatusWordDefinedAdapter = PropertyFactory.Create(
                @this => @this.IsStatusWordDefined,
                @this => @this.value.StatusWordIndication != null
                );

            canUndefineStatusWordAdapter = PropertyFactory.Create(
                @this => @this.CanUndefineStatusWord,
                @this => @this.value.CanUndefineStatusWordIndication
                );
        }

        public string StatusWord 
        {
            get { return statusWordAdapter.GetValue(this); }
            set
            {
                if (value != StatusWordIndication.Description)
                {
                    statusWordAdapter.SetValue(this, value);
                }//workaround for combobox seting the selected enumeration value as string when user selects something. We ignore that to display th underlying ushort value
            }
        }

        public bool IsStatusWordDefined
        {
            get { return isStatusWordDefinedAdapter.GetValue(this); }
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

        public bool CanUndefineStatusWord
        {
            get { return canUndefineStatusWordAdapter.GetValue(this); }
        }

        public string LifeCycle
        {
            get { return lifeCycleAdapter.GetValue(this); }
            set
            {
                if (value != LifeCycleInformation.Description)
                {
                    lifeCycleAdapter.SetValue(this, value);
                }//workaround for combobox seting the selected enumeration value as string when user selects something. We ignore that to display th underlying ushort value
            }
        }

        public bool IsLifeCycleDefined
        {
            get { return isLifeCycleDefinedAdapter.GetValue(this); }
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

        public bool CanUndefineLifeCycle
        {
            get { return canUndefineLifeCycleAdapter.GetValue(this); }
        }

        public EnumerationAdapter<StatusWordIndication> StatusWordIndication
        {
            get { return statusWordIndicationAdapter.GetValue(this); }
            set { statusWordIndicationAdapter.SetValue(this, value); }
        }

        public IEnumerable<EnumerationAdapter<StatusWordIndication>> StatusWordIndicationValues
        {
            get { return EnumerationAdapter<StatusWordIndication>.Items.Where(_=>_.Value != Classes.ATR.StatusWordIndication.RFU); }
        }

        public EnumerationAdapter<KnownLifeCycle> LifeCycleInformation
        {
            get { return lifeCycleInformationAdapter.GetValue(this); }
            set { lifeCycleInformationAdapter.SetValue(this, value); }
        }

        public IEnumerable<EnumerationAdapter<KnownLifeCycle>> KnownLifeCycleValues
        {
            get { return EnumerationAdapter<KnownLifeCycle>.Items.Where(_ => _.Value != Classes.ATR.KnownLifeCycle.RFU); }
        }

        public EnumerationAdapter<StatusWordCoding> StatusWordCoding
        {
            get { return statusWordCodingAdapter.GetValue(this); }
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

        public IEnumerable<EnumerationAdapter<StatusWordCoding>> StatusWordCodingValues
        {
            get { return EnumerationAdapter<StatusWordCoding>.Items; }
        }

        public bool IncludedInTlv { get { return includedInTlvAdapter.GetValue(this); } }

        public DataObjectStatusIndicatorAdapter(CompactTLVDataObjectStatusIndicator value)
            : base(value)
        {
            this.value = value;
        }
    }
}