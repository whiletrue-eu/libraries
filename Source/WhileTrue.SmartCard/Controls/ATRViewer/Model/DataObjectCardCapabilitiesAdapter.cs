using System;
using System.Collections.Generic;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class DataObjectCardCapabilitiesAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTLVDataObjectCardCapabilities value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, byte?> maximumNumberOfLogicalChannelsAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, EnumerationAdapter<LogicalChannelAssignment>> logicalChannelAssignmentAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, bool?> supportsExtendedLcAndLeAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, int?> dataUnitSizeAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, EnumerationAdapter<WriteFunctionsBehaviour>> writeFunctionsBehaviourAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsRecordIDAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsRecordNumberAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsShortFileIDAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsImplicitDfSelectionAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsDfSelectionByFileIDAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsDfSelectionByPathAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsDfSelectionByPartialNameAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsDfSelectionByFullNameAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> isExtendedLengthAndLogicalChannelsIndicatedAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> isWriteFunctionsBehaviourAndDataUnitSizeIndicatedAdapter;

        static DataObjectCardCapabilitiesAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectCardCapabilitiesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectCardCapabilitiesAdapter>();
            maximumNumberOfLogicalChannelsAdapter = PropertyFactory.Create(
                @this => @this.MaximumNumberOfLogicalChannels,
                @this => @this.value.MaximumNumberOfLogicalChannels
                );
            logicalChannelAssignmentAdapter = PropertyFactory.Create(
                @this => @this.LogicalChannelAssignment,
                @this => EnumerationAdapter<LogicalChannelAssignment>.GetInstanceFor(@this.value.LogicalChannelAssignment)
                );
            supportsExtendedLcAndLeAdapter = PropertyFactory.Create(
                @this => @this.SupportsExtendedLcAndLe,
                @this => @this.value.SupportsExtendedLcAndLe
                );
            dataUnitSizeAdapter = PropertyFactory.Create(
                @this => @this.DataUnitSize,
                @this => @this.value.DataUnitSize
                );
            writeFunctionsBehaviourAdapter = PropertyFactory.Create(
                @this => @this.WriteFunctionsBehaviour,
                @this => EnumerationAdapter<WriteFunctionsBehaviour>.GetInstanceFor(@this.value.WriteFunctionsBehaviour)
                );
            supportsRecordIDAdapter = PropertyFactory.Create(
                @this => @this.SupportsRecordID,
                @this => @this.value.SupportsRecordID,
                (@this,value) => @this.value.SupportsRecordID=value
                );
            supportsRecordNumberAdapter = PropertyFactory.Create(
                @this => @this.SupportsRecordNumber,
                @this => @this.value.SupportsRecordNumber,
                (@this,value) => @this.value.SupportsRecordNumber=value
                );
            supportsShortFileIDAdapter = PropertyFactory.Create(
                @this => @this.SupportsShortFileID,
                @this => @this.value.SupportsShortFileID,
                (@this,value) => @this.value.SupportsShortFileID=value
                );
            supportsImplicitDfSelectionAdapter = PropertyFactory.Create(
                @this => @this.SupportsImplicitDFSelection,
                @this => @this.value.SupportsImplicitDFSelection,
                (@this,value) => @this.value.SupportsImplicitDFSelection=value
                );
            supportsDfSelectionByFileIDAdapter = PropertyFactory.Create(
                @this => @this.SupportsDFSelectionByFileID,
                @this => @this.value.SupportsDFSelectionByFileID,
                (@this,value) => @this.value.SupportsDFSelectionByFileID=value
                );
            supportsDfSelectionByPathAdapter = PropertyFactory.Create(
                @this => @this.SupportsDFSelectionByPath,
                @this => @this.value.SupportsDFSelectionByPath,
                (@this,value) => @this.value.SupportsDFSelectionByPath=value
                );
            supportsDfSelectionByPartialNameAdapter = PropertyFactory.Create(
                @this => @this.SupportsDFSelectionByPartialName,
                @this => @this.value.SupportsDFSelectionByPartialName,
                (@this,value) => @this.value.SupportsDFSelectionByPartialName=value
                );
            supportsDfSelectionByFullNameAdapter = PropertyFactory.Create(
                @this => @this.SupportsDFSelectionByFullName,
                @this => @this.value.SupportsDFSelectionByFullName,
                (@this,value) => @this.value.SupportsDFSelectionByFullName=value
                );
            isExtendedLengthAndLogicalChannelsIndicatedAdapter = PropertyFactory.Create(
                @this => @this.IsExtendedLengthAndLogicalChannelsIndicated,
                @this => @this.value.SupportsExtendedLcAndLe != null
                );
            isWriteFunctionsBehaviourAndDataUnitSizeIndicatedAdapter = PropertyFactory.Create(
            @this => @this.IsWriteFunctionsBehaviourAndDataUnitSizeIndicated,
            @this => @this.value.WriteFunctionsBehaviour != null
            );
        }

        public DataObjectCardCapabilitiesAdapter(CompactTLVDataObjectCardCapabilities value)
            : base(value)
        {
            this.value = value;
        }

        public bool IsExtendedLengthAndLogicalChannelsIndicated
        {
            get { return isExtendedLengthAndLogicalChannelsIndicatedAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.value.SetExtendedLengthAndLogicalChannels(false, Classes.ATR.LogicalChannelAssignment.AssignedByCard, 1);
                }
                else
                {
                    this.value.SetExtendedLengthAndLogicalChannelsToNotIndicated();
                }
            }
        }

        public byte? MaximumNumberOfLogicalChannels
        {
            get { return maximumNumberOfLogicalChannelsAdapter.GetValue(this); }
            set { this.value.SetExtendedLengthAndLogicalChannels(this.SupportsExtendedLcAndLe.Value, this.LogicalChannelAssignment, value.Value ); }
        }

        public EnumerationAdapter<LogicalChannelAssignment> LogicalChannelAssignment
        {
            get { return logicalChannelAssignmentAdapter.GetValue(this); }
            set { this.value.SetExtendedLengthAndLogicalChannels(this.SupportsExtendedLcAndLe.Value, value.Value, this.MaximumNumberOfLogicalChannels.Value); }
        }

        public IEnumerable<EnumerationAdapter<LogicalChannelAssignment>> LogicalChannelAssignmentValues
        {
            get { return EnumerationAdapter<LogicalChannelAssignment>.Items; }
        }

        public bool? SupportsExtendedLcAndLe
        {
            get { return supportsExtendedLcAndLeAdapter.GetValue(this); }
            set { this.value.SetExtendedLengthAndLogicalChannels(value.Value, this.LogicalChannelAssignment, this.MaximumNumberOfLogicalChannels.Value); }
        }

        public bool IsWriteFunctionsBehaviourAndDataUnitSizeIndicated
        {
            get { return isWriteFunctionsBehaviourAndDataUnitSizeIndicatedAdapter.GetValue(this); }
            set
            {
                if (value)
                {
                    this.value.SetWriteFunctionsBehaviourAndDataUnitSize(Classes.ATR.WriteFunctionsBehaviour.OneTimeWrite, 1);
                }
                else
                {
                    this.value.SetExtendedLengthAndLogicalChannelsToNotIndicated();
                    this.value.SetWriteFunctionsBehaviourAndDataUnitSizeToNotIndicated();
                }
            }
        }

        public int? DataUnitSize
        {
            get { return dataUnitSizeAdapter.GetValue(this); }
            set { this.value.SetWriteFunctionsBehaviourAndDataUnitSize(this.WriteFunctionsBehaviour.Value, value.Value); }
        }
        public IEnumerable<int> DataUnitSizeValues
        { get { return new[] {1, 2, 4, 8, 16, 32, 64, 128}; } }

        public EnumerationAdapter<WriteFunctionsBehaviour> WriteFunctionsBehaviour
        {
            get { return writeFunctionsBehaviourAdapter.GetValue(this); }
            set { this.value.SetWriteFunctionsBehaviourAndDataUnitSize(value.Value, this.DataUnitSize.Value); }
        }
        public IEnumerable<EnumerationAdapter<WriteFunctionsBehaviour>> WriteFunctionsBehaviourValues
        { get { return EnumerationAdapter<WriteFunctionsBehaviour>.Items; } }

        public bool SupportsRecordID
        {
            get { return supportsRecordIDAdapter.GetValue(this); }
            set { supportsRecordIDAdapter.SetValue(this,value); }
        }

        public bool SupportsRecordNumber
        {
            get { return supportsRecordNumberAdapter.GetValue(this); }
            set { supportsRecordNumberAdapter.SetValue(this,value); }
        }

        public bool SupportsShortFileID
        {
            get { return supportsShortFileIDAdapter.GetValue(this); }
            set { supportsShortFileIDAdapter.SetValue(this,value); }
        }

        public bool SupportsImplicitDFSelection
        {
            get { return supportsImplicitDfSelectionAdapter.GetValue(this); }
            set { supportsImplicitDfSelectionAdapter.SetValue(this,value); }
        }

        public bool SupportsDFSelectionByFileID
        {
            get { return supportsDfSelectionByFileIDAdapter.GetValue(this); }
            set { supportsDfSelectionByFileIDAdapter.SetValue(this,value); }
        }

        public bool SupportsDFSelectionByPath
        {
            get { return supportsDfSelectionByPathAdapter.GetValue(this); }
            set { supportsDfSelectionByPathAdapter.SetValue(this,value); }
        }

        public bool SupportsDFSelectionByPartialName
        {
            get { return supportsDfSelectionByPartialNameAdapter.GetValue(this); }
            set { supportsDfSelectionByPartialNameAdapter.SetValue(this,value); }
        }

        public bool SupportsDFSelectionByFullName
        {
            get { return supportsDfSelectionByFullNameAdapter.GetValue(this); }
            set { supportsDfSelectionByFullNameAdapter.SetValue(this,value); }
        }
    }
}