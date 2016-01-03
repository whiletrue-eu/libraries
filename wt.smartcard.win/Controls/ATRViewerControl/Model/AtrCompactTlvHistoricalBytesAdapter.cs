using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class AtrCompactTlvHistoricalBytesAdapter : AtrHistoricalBytesAdapterBase
    {
        private readonly AtrCompactTlvHistoricalCharacters historicalCharacters;
        private static EnumerablePropertyAdapter<AtrCompactTlvHistoricalBytesAdapter, CompactTlvDataObjectBase, DataObjectBaseAdapter> dataObjectAdapter;
        private readonly DelegateCommand<CompactTlvTypes> addDataObjectCommand;
        private readonly DelegateCommand<CompactTlvTypes> removeDataObjectCommand;


        static AtrCompactTlvHistoricalBytesAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<AtrCompactTlvHistoricalBytesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrCompactTlvHistoricalBytesAdapter>();

            AtrCompactTlvHistoricalBytesAdapter.dataObjectAdapter = PropertyFactory.Create(
                nameof(AtrCompactTlvHistoricalBytesAdapter.DataObjects),
                instance => instance.historicalCharacters.DataObjects,
                (instance,value)=>DataObjectBaseAdapter.GetObject(value)
                );

            EnumerationAdapter<FileIoServices>.Items = new []
            {
                new EnumerationAdapter<FileIoServices>(FileIoServices.ReadBinary, "Read Binary", "Read Binary"), 
                new EnumerationAdapter<FileIoServices>(FileIoServices.ReadRecord, "Read Record(s)", "Read Record(s)")
            };

            EnumerationAdapter<StatusWordIndication>.Items = new[]
            {
                new EnumerationAdapter<StatusWordIndication>(StatusWordIndication.NormalProcessing, "Normal Processing", "Normal Processing 0x9000"), 
                new EnumerationAdapter<StatusWordIndication>(StatusWordIndication.StatusNotIndicated, "Status not indicated", "Status not indicated 0x0000"), 
                new EnumerationAdapter<StatusWordIndication>(StatusWordIndication.Rfu, "RFU", "RFU")
            };

            EnumerationAdapter<KnownLifeCycle>.Items = new[]
            {
                new EnumerationAdapter<KnownLifeCycle>(KnownLifeCycle.NotIndicated, "Not Indicated", "Not Indicated 0x00"), 
                new EnumerationAdapter<KnownLifeCycle>(KnownLifeCycle.Rfu, "RFU", "RFU")
            };

            EnumerationAdapter<StatusWordCoding>.Items = new[]
            {
                new EnumerationAdapter<StatusWordCoding>(StatusWordCoding.WithinTlvData, "Within TLV data objects", "Within TLV data objects"), 
                new EnumerationAdapter<StatusWordCoding>(StatusWordCoding.FollowingTlvData, "Following the TLV data", "Following the TLV data")
            };

            EnumerationAdapter<LogicalChannelAssignment>.Items = new []
            {
                new EnumerationAdapter<LogicalChannelAssignment>(LogicalChannelAssignment.AssignedByCard, "Assigned by card", "Assigned by card (01)"), 
                new EnumerationAdapter<LogicalChannelAssignment>(LogicalChannelAssignment.AssignedbyInterfaceDevice, "Assigned by interface device", "Assigned by interface device (10)"), 
                new EnumerationAdapter<LogicalChannelAssignment>(LogicalChannelAssignment.NoLogicalChannel, "No logical channel", "No logical channel (00)"), 
                new EnumerationAdapter<LogicalChannelAssignment>(LogicalChannelAssignment.Rfu11, "RFU 11", "RFU (11)")
            };

            EnumerationAdapter<WriteFunctionsBehaviour>.Items = new[]
            {
                new EnumerationAdapter<WriteFunctionsBehaviour>(WriteFunctionsBehaviour.OneTimeWrite, "One time write", "One time write (00)"), 
                new EnumerationAdapter<WriteFunctionsBehaviour>(WriteFunctionsBehaviour.Proprietary, "Proprietary", "Proprietary (01)"), 
                new EnumerationAdapter<WriteFunctionsBehaviour>(WriteFunctionsBehaviour.WriteAnd, "Write AND", "Write AND (10)"), 
                new EnumerationAdapter<WriteFunctionsBehaviour>(WriteFunctionsBehaviour.WriteOr, "Write OR", "Write OR (11)") 
            };
        }

        public IEnumerable<DataObjectBaseAdapter> DataObjects => AtrCompactTlvHistoricalBytesAdapter.dataObjectAdapter.GetCollection(this);

        public AtrCompactTlvHistoricalBytesAdapter(AtrCompactTlvHistoricalCharacters historicalCharacters, InterpretedAtrAdapter interpretedAtr)
            : base(historicalCharacters, interpretedAtr)
        {
            this.historicalCharacters = historicalCharacters;

            this.addDataObjectCommand = new DelegateCommand<CompactTlvTypes>(this.AddDataObject, _ => this.historicalCharacters.DataObjects.Any(item=>item.Type==_)==false, null, this.HandleException);
            this.removeDataObjectCommand = new DelegateCommand<CompactTlvTypes>(this.RemoveDataObject, _ => this.historicalCharacters.DataObjects.Any(item => item.Type == _), null, this.HandleException);
        }

        private void RemoveDataObject(CompactTlvTypes type)
        {
            this.historicalCharacters.RemoveDataObject(type);
        }

        private void AddDataObject(CompactTlvTypes type)
        {
            this.historicalCharacters.AddDataObject(type);
        }

        public ICommand AddDataObjectCommand => this.addDataObjectCommand;

        public ICommand RemoveDataObjectCommand => this.removeDataObjectCommand;
    }
}