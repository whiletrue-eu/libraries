using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class AtrCompactTlvHistoricalBytesAdapter : AtrHistoricalBytesAdapterBase
    {
        private readonly AtrCompactTlvHistoricalCharacters historicalCharacters;
        private static EnumerablePropertyAdapter<AtrCompactTlvHistoricalBytesAdapter, CompactTLVDataObjectBase, DataObjectBaseAdapter> dataObjectAdapter;
        private readonly DelegateCommand<CompactTLVTypes> addDataObjectCommand;
        private readonly DelegateCommand<CompactTLVTypes> removeDataObjectCommand;


        static AtrCompactTlvHistoricalBytesAdapter()
        {
            IPropertyAdapterFactory<AtrCompactTlvHistoricalBytesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrCompactTlvHistoricalBytesAdapter>();

            dataObjectAdapter = PropertyFactory.Create(
                @this => @this.DataObjects,
                @this => @this.historicalCharacters.DataObjects,
                (@this,value)=>DataObjectBaseAdapter.GetObject(value)
                );

            EnumerationAdapter<FileIOServices>.Items = new []
            {
                new EnumerationAdapter<FileIOServices>(FileIOServices.ReadBinary, "Read Binary", "Read Binary"), 
                new EnumerationAdapter<FileIOServices>(FileIOServices.ReadRecord, "Read Record(s)", "Read Record(s)")
            };

            EnumerationAdapter<StatusWordIndication>.Items = new[]
            {
                new EnumerationAdapter<StatusWordIndication>(StatusWordIndication.NormalProcessing, "Normal Processing", "Normal Processing 0x9000"), 
                new EnumerationAdapter<StatusWordIndication>(StatusWordIndication.StatusNotIndicated, "Status not indicated", "Status not indicated 0x0000"), 
                new EnumerationAdapter<StatusWordIndication>(StatusWordIndication.RFU, "RFU", "RFU")
            };

            EnumerationAdapter<KnownLifeCycle>.Items = new[]
            {
                new EnumerationAdapter<KnownLifeCycle>(KnownLifeCycle.NotIndicated, "Not Indicated", "Not Indicated 0x00"), 
                new EnumerationAdapter<KnownLifeCycle>(KnownLifeCycle.RFU, "RFU", "RFU")
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
                new EnumerationAdapter<LogicalChannelAssignment>(LogicalChannelAssignment.RFU_11, "RFU 11", "RFU (11)")
            };

            EnumerationAdapter<WriteFunctionsBehaviour>.Items = new[]
            {
                new EnumerationAdapter<WriteFunctionsBehaviour>(WriteFunctionsBehaviour.OneTimeWrite, "One time write", "One time write (00)"), 
                new EnumerationAdapter<WriteFunctionsBehaviour>(WriteFunctionsBehaviour.Proprietary, "Proprietary", "Proprietary (01)"), 
                new EnumerationAdapter<WriteFunctionsBehaviour>(WriteFunctionsBehaviour.WriteAnd, "Write AND", "Write AND (10)"), 
                new EnumerationAdapter<WriteFunctionsBehaviour>(WriteFunctionsBehaviour.WriteOr, "Write OR", "Write OR (11)") 
            };
        }

        public IEnumerable<DataObjectBaseAdapter> DataObjects { get { return dataObjectAdapter.GetCollection(this); } }

        public AtrCompactTlvHistoricalBytesAdapter(AtrCompactTlvHistoricalCharacters historicalCharacters, InterpretedAtrAdapter interpretedAtr)
            : base(historicalCharacters, interpretedAtr)
        {
            this.historicalCharacters = historicalCharacters;

            this.addDataObjectCommand = new DelegateCommand<CompactTLVTypes>(this.AddDataObject, _ => this.historicalCharacters.DataObjects.Any(item=>item.Type==_)==false, this.HandleException);
            this.removeDataObjectCommand = new DelegateCommand<CompactTLVTypes>(this.RemoveDataObject, _ => this.historicalCharacters.DataObjects.Any(item => item.Type == _), this.HandleException);
        }

        private void RemoveDataObject(CompactTLVTypes type)
        {
            this.historicalCharacters.RemoveDataObject(type);
        }

        private void AddDataObject(CompactTLVTypes type)
        {
            this.historicalCharacters.AddDataObject(type);
        }

        public ICommand AddDataObjectCommand
        {
            get { return this.addDataObjectCommand; }
        }

        public ICommand RemoveDataObjectCommand
        {
            get { return this.removeDataObjectCommand; }
        }
    }
}