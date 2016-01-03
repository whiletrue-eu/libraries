using System.Collections.Generic;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class DataObjectCardCapabilitiesAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTlvDataObjectCardCapabilities value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, byte?> maximumNumberOfLogicalChannelsAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, EnumerationAdapter<LogicalChannelAssignment>> logicalChannelAssignmentAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, bool?> supportsExtendedLcAndLeAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, int?> dataUnitSizeAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, EnumerationAdapter<WriteFunctionsBehaviour>> writeFunctionsBehaviourAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsRecordIdAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsRecordNumberAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsShortFileIdAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsImplicitDfSelectionAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsDfSelectionByFileIdAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsDfSelectionByPathAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsDfSelectionByPartialNameAdapter;
        private static readonly PropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> supportsDfSelectionByFullNameAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> isExtendedLengthAndLogicalChannelsIndicatedAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardCapabilitiesAdapter, bool> isWriteFunctionsBehaviourAndDataUnitSizeIndicatedAdapter;

        static DataObjectCardCapabilitiesAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectCardCapabilitiesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectCardCapabilitiesAdapter>();
            DataObjectCardCapabilitiesAdapter.maximumNumberOfLogicalChannelsAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.MaximumNumberOfLogicalChannels),
                instance => instance.value.MaximumNumberOfLogicalChannels
                );
            DataObjectCardCapabilitiesAdapter.logicalChannelAssignmentAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.LogicalChannelAssignment),
                instance => EnumerationAdapter<LogicalChannelAssignment>.GetInstanceFor(instance.value.LogicalChannelAssignment)
                );
            DataObjectCardCapabilitiesAdapter.supportsExtendedLcAndLeAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.SupportsExtendedLcAndLe),
                instance => instance.value.SupportsExtendedLcAndLe
                );
            DataObjectCardCapabilitiesAdapter.dataUnitSizeAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.DataUnitSize),
                instance => instance.value.DataUnitSize
                );
            DataObjectCardCapabilitiesAdapter.writeFunctionsBehaviourAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.WriteFunctionsBehaviour),
                instance => EnumerationAdapter<WriteFunctionsBehaviour>.GetInstanceFor(instance.value.WriteFunctionsBehaviour)
                );
            DataObjectCardCapabilitiesAdapter.supportsRecordIdAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.SupportsRecordId),
                instance => instance.value.SupportsRecordId,
                (instance,value) => instance.value.SupportsRecordId=value
                );
            DataObjectCardCapabilitiesAdapter.supportsRecordNumberAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.SupportsRecordNumber),
                instance => instance.value.SupportsRecordNumber,
                (instance,value) => instance.value.SupportsRecordNumber=value
                );
            DataObjectCardCapabilitiesAdapter.supportsShortFileIdAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.SupportsShortFileId),
                instance => instance.value.SupportsShortFileId,
                (instance,value) => instance.value.SupportsShortFileId=value
                );
            DataObjectCardCapabilitiesAdapter.supportsImplicitDfSelectionAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.SupportsImplicitDfSelection),
                instance => instance.value.SupportsImplicitDfSelection,
                (instance,value) => instance.value.SupportsImplicitDfSelection=value
                );
            DataObjectCardCapabilitiesAdapter.supportsDfSelectionByFileIdAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.SupportsDfSelectionByFileId),
                instance => instance.value.SupportsDfSelectionByFileId,
                (instance,value) => instance.value.SupportsDfSelectionByFileId=value
                );
            DataObjectCardCapabilitiesAdapter.supportsDfSelectionByPathAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.SupportsDfSelectionByPath),
                instance => instance.value.SupportsDfSelectionByPath,
                (instance,value) => instance.value.SupportsDfSelectionByPath=value
                );
            DataObjectCardCapabilitiesAdapter.supportsDfSelectionByPartialNameAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.SupportsDfSelectionByPartialName),
                instance => instance.value.SupportsDfSelectionByPartialName,
                (instance,value) => instance.value.SupportsDfSelectionByPartialName=value
                );
            DataObjectCardCapabilitiesAdapter.supportsDfSelectionByFullNameAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.SupportsDfSelectionByFullName),
                instance => instance.value.SupportsDfSelectionByFullName,
                (instance,value) => instance.value.SupportsDfSelectionByFullName=value
                );
            DataObjectCardCapabilitiesAdapter.isExtendedLengthAndLogicalChannelsIndicatedAdapter = PropertyFactory.Create(
                nameof(DataObjectCardCapabilitiesAdapter.IsExtendedLengthAndLogicalChannelsIndicated),
                instance => instance.value.SupportsExtendedLcAndLe != null
                );
            DataObjectCardCapabilitiesAdapter.isWriteFunctionsBehaviourAndDataUnitSizeIndicatedAdapter = PropertyFactory.Create(
            nameof(DataObjectCardCapabilitiesAdapter.IsWriteFunctionsBehaviourAndDataUnitSizeIndicated),
            instance => instance.value.WriteFunctionsBehaviour != null
            );
        }

        public DataObjectCardCapabilitiesAdapter(CompactTlvDataObjectCardCapabilities value)
            : base(value)
        {
            this.value = value;
        }

        public bool IsExtendedLengthAndLogicalChannelsIndicated
        {
            get { return DataObjectCardCapabilitiesAdapter.isExtendedLengthAndLogicalChannelsIndicatedAdapter.GetValue(this); }
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
            get { return DataObjectCardCapabilitiesAdapter.maximumNumberOfLogicalChannelsAdapter.GetValue(this); }
            set { this.value.SetExtendedLengthAndLogicalChannels(this.SupportsExtendedLcAndLe.Value, this.LogicalChannelAssignment, value.Value ); }
        }

        public EnumerationAdapter<LogicalChannelAssignment> LogicalChannelAssignment
        {
            get { return DataObjectCardCapabilitiesAdapter.logicalChannelAssignmentAdapter.GetValue(this); }
            set { this.value.SetExtendedLengthAndLogicalChannels(this.SupportsExtendedLcAndLe.Value, value.Value, this.MaximumNumberOfLogicalChannels.Value); }
        }

        public IEnumerable<EnumerationAdapter<LogicalChannelAssignment>> LogicalChannelAssignmentValues => EnumerationAdapter<LogicalChannelAssignment>.Items;

        public bool? SupportsExtendedLcAndLe
        {
            get { return DataObjectCardCapabilitiesAdapter.supportsExtendedLcAndLeAdapter.GetValue(this); }
            set { this.value.SetExtendedLengthAndLogicalChannels(value.Value, this.LogicalChannelAssignment, this.MaximumNumberOfLogicalChannels.Value); }
        }

        public bool IsWriteFunctionsBehaviourAndDataUnitSizeIndicated
        {
            get { return DataObjectCardCapabilitiesAdapter.isWriteFunctionsBehaviourAndDataUnitSizeIndicatedAdapter.GetValue(this); }
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
            get { return DataObjectCardCapabilitiesAdapter.dataUnitSizeAdapter.GetValue(this); }
            set { this.value.SetWriteFunctionsBehaviourAndDataUnitSize(this.WriteFunctionsBehaviour.Value, value.Value); }
        }
        public IEnumerable<int> DataUnitSizeValues => new[] {1, 2, 4, 8, 16, 32, 64, 128};

        public EnumerationAdapter<WriteFunctionsBehaviour> WriteFunctionsBehaviour
        {
            get { return DataObjectCardCapabilitiesAdapter.writeFunctionsBehaviourAdapter.GetValue(this); }
            set { this.value.SetWriteFunctionsBehaviourAndDataUnitSize(value.Value, this.DataUnitSize.Value); }
        }
        public IEnumerable<EnumerationAdapter<WriteFunctionsBehaviour>> WriteFunctionsBehaviourValues => EnumerationAdapter<WriteFunctionsBehaviour>.Items;

        public bool SupportsRecordId
        {
            get { return DataObjectCardCapabilitiesAdapter.supportsRecordIdAdapter.GetValue(this); }
            set { DataObjectCardCapabilitiesAdapter.supportsRecordIdAdapter.SetValue(this,value); }
        }

        public bool SupportsRecordNumber
        {
            get { return DataObjectCardCapabilitiesAdapter.supportsRecordNumberAdapter.GetValue(this); }
            set { DataObjectCardCapabilitiesAdapter.supportsRecordNumberAdapter.SetValue(this,value); }
        }

        public bool SupportsShortFileId
        {
            get { return DataObjectCardCapabilitiesAdapter.supportsShortFileIdAdapter.GetValue(this); }
            set { DataObjectCardCapabilitiesAdapter.supportsShortFileIdAdapter.SetValue(this,value); }
        }

        public bool SupportsImplicitDfSelection
        {
            get { return DataObjectCardCapabilitiesAdapter.supportsImplicitDfSelectionAdapter.GetValue(this); }
            set { DataObjectCardCapabilitiesAdapter.supportsImplicitDfSelectionAdapter.SetValue(this,value); }
        }

        public bool SupportsDfSelectionByFileId
        {
            get { return DataObjectCardCapabilitiesAdapter.supportsDfSelectionByFileIdAdapter.GetValue(this); }
            set { DataObjectCardCapabilitiesAdapter.supportsDfSelectionByFileIdAdapter.SetValue(this,value); }
        }

        public bool SupportsDfSelectionByPath
        {
            get { return DataObjectCardCapabilitiesAdapter.supportsDfSelectionByPathAdapter.GetValue(this); }
            set { DataObjectCardCapabilitiesAdapter.supportsDfSelectionByPathAdapter.SetValue(this,value); }
        }

        public bool SupportsDfSelectionByPartialName
        {
            get { return DataObjectCardCapabilitiesAdapter.supportsDfSelectionByPartialNameAdapter.GetValue(this); }
            set { DataObjectCardCapabilitiesAdapter.supportsDfSelectionByPartialNameAdapter.SetValue(this,value); }
        }

        public bool SupportsDfSelectionByFullName
        {
            get { return DataObjectCardCapabilitiesAdapter.supportsDfSelectionByFullNameAdapter.GetValue(this); }
            set { DataObjectCardCapabilitiesAdapter.supportsDfSelectionByFullNameAdapter.SetValue(this,value); }
        }
    }
}