using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class DataObjectInitialAccessDataAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTlvDataObjectInitialAccessData value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectInitialAccessDataAdapter, string> initialAccessDataAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectInitialAccessDataAdapter, bool> isInitialAccessDataReadBinaryAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectInitialAccessDataAdapter, bool> isInitialAccessDataReadRecordAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectInitialAccessDataAdapter, bool> isInitialAccessDataCompleteApduAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectInitialAccessDataAdapter, byte?> lengthToReadBinaryAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectInitialAccessDataAdapter, byte?> lengthToReadRecordAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectInitialAccessDataAdapter, byte?> transparentShortFileIdAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectInitialAccessDataAdapter, byte?> recordShortFileIdAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectInitialAccessDataAdapter, bool> isTransparentShortFileIdDefinedAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectInitialAccessDataAdapter, string> customApduAdapter;

        static DataObjectInitialAccessDataAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectInitialAccessDataAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectInitialAccessDataAdapter>();

            DataObjectInitialAccessDataAdapter.initialAccessDataAdapter = PropertyFactory.Create(
                nameof(DataObjectInitialAccessDataAdapter.InitialAccessDataApdu),
                instance=>instance.value.InitialAccessData.GetApdu().ToHexString(" ")
                );
            DataObjectInitialAccessDataAdapter.isInitialAccessDataReadBinaryAdapter = PropertyFactory.Create(
                nameof(DataObjectInitialAccessDataAdapter.IsInitialAccessDataReadBinary),
                instance=>instance.value.InitialAccessData is CompactTlvDataObjectInitialAccessData.ReadBinary
                );
            DataObjectInitialAccessDataAdapter.isInitialAccessDataReadRecordAdapter = PropertyFactory.Create(
                nameof(DataObjectInitialAccessDataAdapter.IsInitialAccessDataReadRecord),
                instance=>instance.value.InitialAccessData is CompactTlvDataObjectInitialAccessData.ReadRecord
                );
            DataObjectInitialAccessDataAdapter.isInitialAccessDataCompleteApduAdapter = PropertyFactory.Create(
                nameof(DataObjectInitialAccessDataAdapter.IsInitialAccessDataCompleteApdu),
                instance=>instance.value.InitialAccessData is CompactTlvDataObjectInitialAccessData.CustomApdu
                );
            DataObjectInitialAccessDataAdapter.lengthToReadBinaryAdapter = PropertyFactory.Create(
                nameof(DataObjectInitialAccessDataAdapter.LengthToReadBinary),
                instance => instance.value.InitialAccessData is CompactTlvDataObjectInitialAccessData.ReadBinary
                    ? ((CompactTlvDataObjectInitialAccessData.ReadBinary)instance.value.InitialAccessData).LengthToRead
                    : (byte?)null
                );
            DataObjectInitialAccessDataAdapter.lengthToReadRecordAdapter = PropertyFactory.Create(
                nameof(DataObjectInitialAccessDataAdapter.LengthToReadRecord),
                instance => instance.value.InitialAccessData is CompactTlvDataObjectInitialAccessData.ReadRecord
                    ? ((CompactTlvDataObjectInitialAccessData.ReadRecord) instance.value.InitialAccessData).LengthToRead
                    : (byte?) null
                );
            DataObjectInitialAccessDataAdapter.transparentShortFileIdAdapter = PropertyFactory.Create(
                nameof(DataObjectInitialAccessDataAdapter.TransparentShortFileId),
                instance => instance.value.InitialAccessData is CompactTlvDataObjectInitialAccessData.ReadBinary
                    ? ((CompactTlvDataObjectInitialAccessData.ReadBinary)instance.value.InitialAccessData).ShortFileId
                    : (byte?)null
                );
            DataObjectInitialAccessDataAdapter.recordShortFileIdAdapter = PropertyFactory.Create(
                nameof(DataObjectInitialAccessDataAdapter.RecordShortFileId),
                instance => instance.value.InitialAccessData is CompactTlvDataObjectInitialAccessData.ReadRecord
                    ? ((CompactTlvDataObjectInitialAccessData.ReadRecord)instance.value.InitialAccessData).ShortFileId
                    : (byte?)null
                );
            DataObjectInitialAccessDataAdapter.isTransparentShortFileIdDefinedAdapter = PropertyFactory.Create(
                nameof(DataObjectInitialAccessDataAdapter.IsTransparentShortFileIdDefined),
                instance => instance.value.InitialAccessData is CompactTlvDataObjectInitialAccessData.ReadBinary
                    ? ((CompactTlvDataObjectInitialAccessData.ReadBinary)instance.value.InitialAccessData).ShortFileId !=null
                    : false
                );
            DataObjectInitialAccessDataAdapter.customApduAdapter = PropertyFactory.Create(
                nameof(DataObjectInitialAccessDataAdapter.CustomApdu),
                instance=> ((CompactTlvDataObjectInitialAccessData.CustomApdu)instance.value.InitialAccessData).ApduBytes.ToHexString(" ")
                );
        }

        public DataObjectInitialAccessDataAdapter(CompactTlvDataObjectInitialAccessData value)
            : base(value)
        {
            this.value = value;
        }

        public string InitialAccessDataApdu => DataObjectInitialAccessDataAdapter.initialAccessDataAdapter.GetValue(this);


        public bool IsInitialAccessDataReadBinary
        {
            get { return DataObjectInitialAccessDataAdapter.isInitialAccessDataReadBinaryAdapter.GetValue(this); }
            set { this.value.InitialAccessData = new CompactTlvDataObjectInitialAccessData.ReadBinary(0x00); }
        }

        public bool IsInitialAccessDataReadRecord
        {
            get { return DataObjectInitialAccessDataAdapter.isInitialAccessDataReadRecordAdapter.GetValue(this); }
            set { this.value.InitialAccessData = new CompactTlvDataObjectInitialAccessData.ReadRecord(0x00, 0x00); }
        }

        public bool IsInitialAccessDataCompleteApdu
        {
            get { return DataObjectInitialAccessDataAdapter.isInitialAccessDataCompleteApduAdapter.GetValue(this); }
            set { this.value.InitialAccessData = new CompactTlvDataObjectInitialAccessData.CustomApdu(new byte[]{0x00,0x00,0x00,0x00,0x00}); }
        }

        public byte? LengthToReadBinary
        {
            get { return DataObjectInitialAccessDataAdapter.lengthToReadBinaryAdapter.GetValue(this); }
            set
            {
                value.DbC_AssureNotNull();
            }
        }

        public byte? LengthToReadRecord
        {
            get { return DataObjectInitialAccessDataAdapter.lengthToReadRecordAdapter.GetValue(this); }
            set
            {
                value.DbC_AssureNotNull();
            }
        }

        public byte? TransparentShortFileId
        {
            get { return DataObjectInitialAccessDataAdapter.transparentShortFileIdAdapter.GetValue(this); }
            set
            {
                value.DbC_AssureNotNull();
                this.LengthToReadBinary.DbC_AssureNotNull();
                this.value.InitialAccessData = new CompactTlvDataObjectInitialAccessData.ReadBinary((byte) value,(byte) this.LengthToReadBinary);
            }
        }

        public byte? RecordShortFileId
        {
            get { return DataObjectInitialAccessDataAdapter.recordShortFileIdAdapter.GetValue(this); }
            set
            {
                value.DbC_AssureNotNull();
                this.LengthToReadRecord.DbC_AssureNotNull();
                this.value.InitialAccessData = new CompactTlvDataObjectInitialAccessData.ReadRecord((byte)value, (byte)this.LengthToReadRecord);
            }
        }

        public bool IsTransparentShortFileIdDefined
        {
            get { return DataObjectInitialAccessDataAdapter.isTransparentShortFileIdDefinedAdapter.GetValue(this); }
            set
            {
                this.LengthToReadBinary.DbC_AssureNotNull();
                if (value)
                {
                    this.value.InitialAccessData = new CompactTlvDataObjectInitialAccessData.ReadBinary(0x00, (byte)this.LengthToReadBinary);
                }
                else
                {
                    this.value.InitialAccessData = new CompactTlvDataObjectInitialAccessData.ReadBinary((byte)this.LengthToReadBinary);
                }
            }
        }

        public string CustomApdu
        {
            get { return DataObjectInitialAccessDataAdapter.customApduAdapter.GetValue(this); }
            set
            {
                Helper.SetAsHexValue(value, 5,5,_=>this.value.InitialAccessData=new CompactTlvDataObjectInitialAccessData.CustomApdu(_));
            }
        }
    }
}