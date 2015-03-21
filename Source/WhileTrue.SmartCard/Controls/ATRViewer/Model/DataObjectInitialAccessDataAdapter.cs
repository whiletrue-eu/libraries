using System;
using System.ComponentModel;
using System.Runtime.Remoting.Contexts;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.CodeInspection;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class DataObjectInitialAccessDataAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTLVDataObjectInitialAccessData value;
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

            initialAccessDataAdapter = PropertyFactory.Create(
                @this=>@this.InitialAccessDataApdu,
                @this=>@this.value.InitialAccessData.GetApdu().ToHexString(" ")
                );
            isInitialAccessDataReadBinaryAdapter = PropertyFactory.Create(
                @this => @this.IsInitialAccessDataReadBinary,
                @this=>@this.value.InitialAccessData is CompactTLVDataObjectInitialAccessData.ReadBinary
                );
            isInitialAccessDataReadRecordAdapter = PropertyFactory.Create(
                @this => @this.IsInitialAccessDataReadRecord,
                @this=>@this.value.InitialAccessData is CompactTLVDataObjectInitialAccessData.ReadRecord
                );
            isInitialAccessDataCompleteApduAdapter = PropertyFactory.Create(
                @this => @this.IsInitialAccessDataCompleteApdu,
                @this=>@this.value.InitialAccessData is CompactTLVDataObjectInitialAccessData.CustomApdu
                );
            lengthToReadBinaryAdapter = PropertyFactory.Create(
                @this => @this.LengthToReadBinary,
                @this => @this.value.InitialAccessData is CompactTLVDataObjectInitialAccessData.ReadBinary
                    ? ((CompactTLVDataObjectInitialAccessData.ReadBinary)@this.value.InitialAccessData).LengthToRead
                    : (byte?)null
                );
            lengthToReadRecordAdapter = PropertyFactory.Create(
                @this => @this.LengthToReadRecord,
                @this => @this.value.InitialAccessData is CompactTLVDataObjectInitialAccessData.ReadRecord
                    ? ((CompactTLVDataObjectInitialAccessData.ReadRecord) @this.value.InitialAccessData).LengthToRead
                    : (byte?) null
                );
            transparentShortFileIdAdapter = PropertyFactory.Create(
                @this => @this.TransparentShortFileId,
                @this => @this.value.InitialAccessData is CompactTLVDataObjectInitialAccessData.ReadBinary
                    ? ((CompactTLVDataObjectInitialAccessData.ReadBinary)@this.value.InitialAccessData).ShortFileId
                    : (byte?)null
                );
            recordShortFileIdAdapter = PropertyFactory.Create(
                @this => @this.RecordShortFileId,
                @this => @this.value.InitialAccessData is CompactTLVDataObjectInitialAccessData.ReadRecord
                    ? ((CompactTLVDataObjectInitialAccessData.ReadRecord)@this.value.InitialAccessData).ShortFileID
                    : (byte?)null
                );
            isTransparentShortFileIdDefinedAdapter = PropertyFactory.Create(
                @this => @this.IsTransparentShortFileIdDefined,
                @this => @this.value.InitialAccessData is CompactTLVDataObjectInitialAccessData.ReadBinary
                    ? ((CompactTLVDataObjectInitialAccessData.ReadBinary)@this.value.InitialAccessData).ShortFileId !=null
                    : false
                );
            customApduAdapter = PropertyFactory.Create(
                @this => @this.CustomApdu,
                @this=> ((CompactTLVDataObjectInitialAccessData.CustomApdu)@this.value.InitialAccessData).ApduBytes.ToHexString(" ")
                );
        }

        public DataObjectInitialAccessDataAdapter(CompactTLVDataObjectInitialAccessData value)
            : base(value)
        {
            this.value = value;
        }

        public string InitialAccessDataApdu
        {
            get { return initialAccessDataAdapter.GetValue(this); }
        }


        public bool IsInitialAccessDataReadBinary
        {
            get { return isInitialAccessDataReadBinaryAdapter.GetValue(this); }
            set { this.value.InitialAccessData = new CompactTLVDataObjectInitialAccessData.ReadBinary(0x00); }
        }

        public bool IsInitialAccessDataReadRecord
        {
            get { return isInitialAccessDataReadRecordAdapter.GetValue(this); }
            set { this.value.InitialAccessData = new CompactTLVDataObjectInitialAccessData.ReadRecord(0x00, 0x00); }
        }

        public bool IsInitialAccessDataCompleteApdu
        {
            get { return isInitialAccessDataCompleteApduAdapter.GetValue(this); }
            set { this.value.InitialAccessData = new CompactTLVDataObjectInitialAccessData.CustomApdu(new byte[]{0x00,0x00,0x00,0x00,0x00}); }
        }

        public byte? LengthToReadBinary
        {
            get { return lengthToReadBinaryAdapter.GetValue(this); }
            set
            {
                value.DbC_AssureNotNull();
            }
        }

        public byte? LengthToReadRecord
        {
            get { return lengthToReadRecordAdapter.GetValue(this); }
            set
            {
                value.DbC_AssureNotNull();
            }
        }

        public byte? TransparentShortFileId
        {
            get { return transparentShortFileIdAdapter.GetValue(this); }
            set
            {
                value.DbC_AssureNotNull();
                this.LengthToReadBinary.DbC_AssureNotNull();
                this.value.InitialAccessData = new CompactTLVDataObjectInitialAccessData.ReadBinary((byte) value,(byte) this.LengthToReadBinary);
            }
        }

        public byte? RecordShortFileId
        {
            get { return recordShortFileIdAdapter.GetValue(this); }
            set
            {
                value.DbC_AssureNotNull();
                this.LengthToReadRecord.DbC_AssureNotNull();
                this.value.InitialAccessData = new CompactTLVDataObjectInitialAccessData.ReadRecord((byte)value, (byte)this.LengthToReadRecord);
            }
        }

        public bool IsTransparentShortFileIdDefined
        {
            get { return isTransparentShortFileIdDefinedAdapter.GetValue(this); }
            set
            {
                this.LengthToReadBinary.DbC_AssureNotNull();
                if (value)
                {
                    this.value.InitialAccessData = new CompactTLVDataObjectInitialAccessData.ReadBinary(0x00, (byte)this.LengthToReadBinary);
                }
                else
                {
                    this.value.InitialAccessData = new CompactTLVDataObjectInitialAccessData.ReadBinary((byte)this.LengthToReadBinary);
                }
            }
        }

        public string CustomApdu
        {
            get { return customApduAdapter.GetValue(this); }
            set
            {
                Helper.SetAsHexValue(value, 5,5,_=>this.value.InitialAccessData=new CompactTLVDataObjectInitialAccessData.CustomApdu(_));
            }
        }
    }
}