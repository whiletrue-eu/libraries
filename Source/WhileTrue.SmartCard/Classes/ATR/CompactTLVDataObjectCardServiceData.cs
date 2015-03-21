using System;

namespace WhileTrue.Classes.ATR
{
    public class CompactTLVDataObjectCardServiceData : CompactTLVDataObjectBase
    {
        private FileIOServices fileIOServicesMethod;
        private bool dataObjectsAvailableInAtrFile;
        private bool dataObjectsAvailableInDirFile;
        private bool supportsApplicationSelectionByPartialDfName;
        private bool supportsApplicationSelectionByFullDfName;
        /*
              ISO 7816-4 ch. 8.3.2 Card service data
     This data object denotes the methods available in the card for supporting the services described in
     clause 9.
     This data object is introduced by '31'.
     When this data object is not present, the card supports only the implicit application selection.
     Table 80 - Card-profile for application-independent card services
     NOTE - The contents of the DIR and ATR files may give information on selection methods.
     b8 b7 b6 b5 b4 b3 b2 b1 Meaning
      1 -- -- -- -- -- -- -- Direct application selection by full DF name
     --  1 -- -- -- -- -- -- Selection by partial DF name (see 9.3.2)
     -- --  x  x -- -- -- -- Data objects available
     -- --  1 -- -- -- -- -- - in DIR file
     -- -- --  1 -- -- -- -- - in ATR file
     -- -- -- --  x -- -- -- File I/O services by
     -- -- -- --  1 -- -- -- - READ BINARY command
     -- -- -- --  0 -- -- -- - READ RECORD(S) command
     -- -- -- -- --  x  x  x '000' (other value are RFU)
         */
        public CompactTLVDataObjectCardServiceData(AtrCompactTlvHistoricalCharacters owner)
            : base(owner)
        {
        }

        protected override byte[] GetValue()
        {
            return new []{(byte)(
                (this.supportsApplicationSelectionByFullDfName?0x80:0x00) |
                (this.supportsApplicationSelectionByPartialDfName?0x40:0x00) |
                (this.dataObjectsAvailableInDirFile?0x20:0x00) |
                (this.dataObjectsAvailableInAtrFile?0x10:0x00) |
                (this.fileIOServicesMethod==FileIOServices.ReadBinary?0x08:0x00) 
                )};
        }

        protected override void UpdateValue(byte[] data)
        {
            if (data != null)
            {
                this.IsApplicable = true;
                if (data.Length == 1)
                {
                    this.SupportsApplicationSelectionByFullDFName = (data[0] & 0x80) == 0x80;
                    this.SupportsApplicationSelectionByPartialDFName = (data[0] & 0x40) == 0x40;
                    this.DataObjectsAvailableInDirFile = (data[0] & 0x20) == 0x20;
                    this.DataObjectsAvailableInAtrFile = (data[0] & 0x10) == 0x10;
                    this.FileIOServicesMethod = (data[0] & 0x08) == 0x08 ? FileIOServices.ReadBinary : FileIOServices.ReadRecord;
                }
                else
                {
                    throw new ArgumentException("Length of card service data must be 1");
                }
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTLVTypes Type
        {
            get { return CompactTLVTypes.CardServiceData; }
        }

        protected override byte[] GetDefaultValue()
        {
            return new byte[] { 0x31,0x00 };
        }

        public bool SupportsApplicationSelectionByFullDFName
        {
            get { return this.supportsApplicationSelectionByFullDfName; }
            set
            {
                this.SetAndInvoke(()=>SupportsApplicationSelectionByFullDFName, ref this.supportsApplicationSelectionByFullDfName, value);
                this.NotifyChanged();
            }
        }

        public bool SupportsApplicationSelectionByPartialDFName
        {
            get { return this.supportsApplicationSelectionByPartialDfName; }
            set
            {
                this.SetAndInvoke(()=>SupportsApplicationSelectionByPartialDFName, ref this.supportsApplicationSelectionByPartialDfName, value); 
                this.NotifyChanged();
            }
        }

        public bool DataObjectsAvailableInDirFile
        {
            get { return this.dataObjectsAvailableInDirFile; }
            set
            {
                this.SetAndInvoke(()=>DataObjectsAvailableInDirFile, ref this.dataObjectsAvailableInDirFile, value); 
                this.NotifyChanged();
            }
        }

        public bool DataObjectsAvailableInAtrFile
        {
            get { return this.dataObjectsAvailableInAtrFile; }
            set
            {
                this.SetAndInvoke(()=>DataObjectsAvailableInAtrFile, ref this.dataObjectsAvailableInAtrFile, value); 
                this.NotifyChanged();
            }
        }

        public FileIOServices FileIOServicesMethod
        {
            get { return this.fileIOServicesMethod; }
            set
            {
                this.SetAndInvoke(()=>FileIOServicesMethod, ref this.fileIOServicesMethod, value); 
                this.NotifyChanged();
            }
        }
    }
}