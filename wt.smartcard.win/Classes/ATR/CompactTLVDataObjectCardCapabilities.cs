using System;
using System.Diagnostics.CodeAnalysis;

namespace WhileTrue.Classes.ATR
{
    public class CompactTlvDataObjectCardCapabilities : CompactTlvDataObjectBase
    {
        private byte? maximumNumberOfLogicalChannels;
        private bool supportsDfSelectionByFullName;
        private bool supportsDfSelectionByPartialName;
        private bool supportsDfSelectionByPath;
        private bool supportsDfSelectionByFileId;
        private bool supportsImplicitDfSelection;
        private bool supportsShortFileId;
        private bool supportsRecordNumber;
        private bool supportsRecordId;
        private WriteFunctionsBehaviour? writeFunctionsBehaviour;
        private int? dataUnitSize;
        private bool? supportsExtendedLcAndLe;
        private LogicalChannelAssignment? logicalChannelAssignment;
        /*
     ISO 7816-4 ch. 8.3.6 Card capabilities
     This data object is optional and of variable length. Its value field consists of either the first
     software function table, or the first two software tables, or the three software function tables.
     This data object is introduced by '71','72' or '73'.
     Table 85 shows the first software function table.
     Table 85 - First software function table
     b8 b7 b6 b5 b4 b3 b2 b1 Meaning
                             DF selection
      1 -- -- -- -- -- -- -- - by full DF name
     --  1 -- -- -- -- -- -- - by partial DF name
     -- --  1 -- -- -- -- -- - by path
     -- -- --  1 -- -- -- -- - by file identifier
     -- -- -- --  1 -- -- -- - implicit
                             EF management
     -- -- -- -- --  1 -- -- - Short EF identifier supported
     -- -- -- -- -- --  1 -- - Record number supported
     -- -- -- -- -- -- --  1 - Record identifier supported
     Table 86 shows the second software function table which is the data coding byte. The data coding
     byte may also be present as the second data element in the file control parameter with tag '82' (see table 2).
     Table 86 - Second software function table (data coding byte)
     b8 b7 b6 b5 b4 b3 b2 b1  Meaning
     --  x  x -- -- -- -- --  Behavior of write functions
     --  0  0 -- -- -- -- --  - one-time write
     --  0  1 -- -- -- -- --  - proprietary
     --  1  0 -- -- -- -- --  - write OR
     --  1  1 -- -- -- -- --  - write AND
     -- -- -- -- --  x  x  x  Data unit size in nibbles (power of 2, e.g. '001'=2 nibbles) (default value=one byte)
      x -- --  x  x -- -- --  0..00 (other values are RFU)
     Table 87 shows the third software functions table.
     Table 87 - Third software function table
     b8 b7 b6 b5 b4 b3 b2 b1  Meaning
      x -- -- -- -- -- -- --  0 (1 is RFU)
     --  1 -- -- -- -- -- --  - Extended Lc and Le fields
     -- --  x -- -- -- -- --  0 (1 is RFU)
     -- -- --  x  x -- -- --  Logical channel assignment
     -- -- --  0  1 -- -- --  - by the card
     -- -- --  1  0 -- -- --  - by the interface device
     -- -- --  0  0 -- -- --  No logical channel
     -- -- -- -- --  x -- --  0 (1 is RFU)
     -- -- -- -- -- --  x  y  Maximum number of logical channels (=2*x+y+1)
         */
        public CompactTlvDataObjectCardCapabilities(AtrCompactTlvHistoricalCharacters owner)
            : base(owner)
        {
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        protected override byte[] GetValue()
        {
            byte[] Data = null;
            //Settings can only set in conjunction with all other data coded in the same byte so querying one property is enough
            if (this.SupportsExtendedLcAndLe != null)
            {
                //Byte 3 (conditional)
                Data = new byte[3];
                Data[2] = (byte) (((byte)(this.SupportsExtendedLcAndLe.Value?0x40:0x00))|
                                  (byte)this.LogicalChannelAssignment.Value | 
                                  (byte)(this.MaximumNumberOfLogicalChannels.Value-1));
            }

            if (this.WriteFunctionsBehaviour != null)
            {
                //Byte 2 (conditional)
                Data = Data ?? new byte[2];
                Data[1] = (byte) ((byte) this.WriteFunctionsBehaviour |
                                  (byte) Math.Log(this.DataUnitSize.Value,2));
            }

            //Byte 1 (always there if tag is set)
            Data = Data ?? new byte[1];
            Data[0] = (byte) ((this.SupportsDfSelectionByFullName ? 0x80 : 0x00) |
                              (this.SupportsDfSelectionByPartialName ? 0x40 : 0x00) |
                              (this.SupportsDfSelectionByPath ? 0x20 : 0x00) |
                              (this.SupportsDfSelectionByFileId ? 0x10 : 0x00) |
                              (this.SupportsImplicitDfSelection ? 0x08 : 0x00) |
                              (this.SupportsShortFileId ? 0x04 : 0x00) |
                              (this.SupportsRecordNumber ? 0x02 : 0x00) |
                              (this.SupportsRecordId ? 0x01 : 0x00));

            return Data;
        }

        protected override void UpdateValue(byte[] data)
        {
            if (data != null)
            {
                this.IsApplicable = true;
                if( data.Length < 1 || data.Length > 3)
                {
                    throw new ArgumentException("Length of card capabilities must be 1, 2 or 3");
                }

                if (data.Length >= 1)
                {
                    this.SupportsDfSelectionByFullName = (data[0] & 0x80) == 0x80;
                    this.SupportsDfSelectionByPartialName = (data[0] & 0x40) == 0x40;
                    this.SupportsDfSelectionByPath = (data[0] & 0x20) == 0x20;
                    this.SupportsDfSelectionByFileId = (data[0] & 0x10) == 0x10;
                    this.SupportsImplicitDfSelection = (data[0] & 0x08) == 0x08;

                    this.SupportsShortFileId = (data[0] & 0x04) == 0x04;
                    this.SupportsRecordNumber = (data[0] & 0x02) == 0x02;
                    this.SupportsRecordId = (data[0] & 0x01) == 0x01;
                }

                if (data.Length >= 2)
                {
                    this.WriteFunctionsBehaviour = (WriteFunctionsBehaviour)(data[1] & 0x60);
                    this.DataUnitSize = (int)Math.Pow(2, (data[1] & 0x07));
                }
                else
                {
                    this.WriteFunctionsBehaviour = null;
                    this.DataUnitSize = null;
                }

                if (data.Length >= 3)
                {
                    this.SupportsExtendedLcAndLe = (data[2] & 0x40) == 0x40;
                    this.LogicalChannelAssignment = (LogicalChannelAssignment)(data[2] & 0x18);
                    this.MaximumNumberOfLogicalChannels = (byte) ((data[2] & 0x03)+1);
                }
                else
                {
                    this.SupportsExtendedLcAndLe = null;
                    this.LogicalChannelAssignment = null;
                    this.MaximumNumberOfLogicalChannels = null;
                }
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTlvTypes Type => CompactTlvTypes.CardCapabilities;

        protected override byte[] GetDefaultValue()
        {
            return new byte[] { 0x71,0x00 };
        }

        public byte? MaximumNumberOfLogicalChannels
        {
            get { return this.maximumNumberOfLogicalChannels; }
            private set
            {
                this.SetAndInvoke(ref this.maximumNumberOfLogicalChannels, value); 
            }
        }

        public LogicalChannelAssignment? LogicalChannelAssignment
        {
            get { return this.logicalChannelAssignment; }
            private set
            {
                this.SetAndInvoke(ref this.logicalChannelAssignment, value);
            }
        }

        public bool? SupportsExtendedLcAndLe
        {
            get { return this.supportsExtendedLcAndLe; }
            private set
            {
                this.SetAndInvoke(ref this.supportsExtendedLcAndLe, value); 
            }
        }

        public void SetExtendedLengthAndLogicalChannels(bool extendedLcAndLeFieldsSupported, LogicalChannelAssignment logicalChannelAssignment, byte maximumNumberOfLogicalChannels)
        {
            if (maximumNumberOfLogicalChannels < 1 || maximumNumberOfLogicalChannels > 4)
            {
                throw new ArgumentException("Maxium number of channels must be a value between 1 and 4");
            }

            this.SupportsExtendedLcAndLe = extendedLcAndLeFieldsSupported;
            this.LogicalChannelAssignment = logicalChannelAssignment;
            this.MaximumNumberOfLogicalChannels = maximumNumberOfLogicalChannels;
            
            this.NotifyChanged();
        }
        public void SetExtendedLengthAndLogicalChannelsToNotIndicated()
        {
            this.SupportsExtendedLcAndLe = null;
            this.LogicalChannelAssignment = null;
            this.MaximumNumberOfLogicalChannels = null;

            this.NotifyChanged();
        }

        public int? DataUnitSize
        {
            get { return this.dataUnitSize; }
            private set
            {
                this.SetAndInvoke(ref this.dataUnitSize, value); 
            }
        }

        public WriteFunctionsBehaviour? WriteFunctionsBehaviour
        {
            get { return this.writeFunctionsBehaviour; }
            private set
            {
                this.SetAndInvoke(ref this.writeFunctionsBehaviour, value);
            }
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public void SetWriteFunctionsBehaviourAndDataUnitSize(WriteFunctionsBehaviour writeFunctionsBehaviour, int dataUnitSize)
        {
            if (Math.Pow(2, (byte)Math.Log(dataUnitSize, 2)) != dataUnitSize || 
                dataUnitSize < Math.Pow(2, 0) || dataUnitSize > Math.Pow(2, 7))
            {
                throw new ArgumentException("Data Unit size must be a value equal to a power of 2 between 2 pow 0 and 2 pow 7",nameof(dataUnitSize));
            }

            this.WriteFunctionsBehaviour = writeFunctionsBehaviour;
            this.DataUnitSize = dataUnitSize;
            this.NotifyChanged();
        }

        public void SetWriteFunctionsBehaviourAndDataUnitSizeToNotIndicated()
        {
            this.WriteFunctionsBehaviour = null;
            this.DataUnitSize = null;
            this.NotifyChanged();
        }

        public bool SupportsRecordId
        {
            get { return this.supportsRecordId; }
            set 
            { 
                this.SetAndInvoke(ref this.supportsRecordId, value);
                this.NotifyChanged();
            }
        }

        public bool SupportsRecordNumber
        {
            get { return this.supportsRecordNumber; }
            set
            {
                this.SetAndInvoke(ref this.supportsRecordNumber, value);
                this.NotifyChanged();
            }
        }

        public bool SupportsShortFileId
        {
            get { return this.supportsShortFileId; }
            set
            {
                this.SetAndInvoke(ref this.supportsShortFileId, value);
                this.NotifyChanged();
            }
        }

        public bool SupportsImplicitDfSelection
        {
            get { return this.supportsImplicitDfSelection; }
            set
            {
                this.SetAndInvoke(ref this.supportsImplicitDfSelection, value);
                this.NotifyChanged();
            }
        }

        public bool SupportsDfSelectionByFileId
        {
            get { return this.supportsDfSelectionByFileId; }
            set
            {
                this.SetAndInvoke(ref this.supportsDfSelectionByFileId, value);
                this.NotifyChanged();
            }
        }

        public bool SupportsDfSelectionByPath
        {
            get { return this.supportsDfSelectionByPath; }
            set
            {
                this.SetAndInvoke(ref this.supportsDfSelectionByPath, value);
                this.NotifyChanged();
            }
        }

        public bool SupportsDfSelectionByPartialName
        {
            get { return this.supportsDfSelectionByPartialName; }
            set
            {
                this.SetAndInvoke(ref this.supportsDfSelectionByPartialName, value);
                this.NotifyChanged();
            }
        }

        public bool SupportsDfSelectionByFullName
        {
            get { return this.supportsDfSelectionByFullName; }
            set
            {
                this.SetAndInvoke(ref this.supportsDfSelectionByFullName, value);
                this.NotifyChanged();
            }
        }
    }
}