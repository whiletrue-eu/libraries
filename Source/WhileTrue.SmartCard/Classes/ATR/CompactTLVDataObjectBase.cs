using System;
using System.Linq;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public abstract class CompactTLVDataObjectBase : ObservableObject
    {
        private readonly AtrCompactTlvHistoricalCharacters owner;
        private bool isApplicable;
        private string dataError;
        private byte[] data;
        private byte? tag;
        private static ReadOnlyPropertyAdapter<CompactTLVDataObjectBase, byte> tagAdapter;
        private bool isUpdating;

        protected CompactTLVDataObjectBase(AtrCompactTlvHistoricalCharacters owner)
        {
            this.owner = owner;
        }

        protected abstract byte[] GetValue();

        public byte? Tag
        {
            get { return this.tag; }
            protected set { this.SetAndInvoke(()=>Tag, ref this.tag, value); }
        }

        public bool IsApplicable
        {
            get { return this.isApplicable; }
            protected set { this.SetAndInvoke(()=>IsApplicable,ref this.isApplicable, value); }
        }

        public string DataError
        {
            get { return this.dataError; }
            private set { this.SetAndInvoke(()=>DataError, ref this.dataError, value); }
        }

        protected void NotifyChanged()
        {
            if (this.isUpdating == false)
            {
                byte[] Value = this.GetValue();
                if (this.Tag.HasValue)
                {
                    this.Data = new[] {(byte) (((this.Tag & 0x0F) << 4) | Value.Length)}.Concat(Value).ToArray();
                }
                else
                {
                    //For status indicator outside of TLV data
                    this.Data = Value;
                }
                this.owner.NotifyChanged();
            }
        }

        public void UpdateTlvData(byte? tagLength, byte[] data)
        {
            if (data != null)
            {
                if (tagLength.HasValue)
                {
                    this.Data = new[] {tagLength.Value}.Concat(data).ToArray();
                    this.Tag = (byte) (((tagLength & 0xF0) >> 4) | 0x40);
                }
                else
                {
                    this.Data = data;
                    this.Tag = null;
                }
            }
            else
            {
                this.Data = null;
            }
            try
            {
                this.isUpdating = true;
                this.UpdateValue(data);
                this.DataError = null;
            }
            catch (Exception Exception)
            {
                this.DataError = Exception.Message;
            }
            finally
            {
                this.isUpdating = false;
            }
        }

        protected abstract void UpdateValue(byte[] data);

        public byte[] Data
        {
            get { return this.data; }
            private set { this.SetAndInvoke(()=>Data, ref this.data, value); }
        }

        public abstract CompactTLVTypes Type { get; }

        internal void Remove()
        {
            this.IsApplicable = false;
            this.NotifyChanged();
        }

        internal void Add()
        {
            byte[] Default = this.GetDefaultValue();
            this.UpdateTlvData(Default[0], Default.GetSubArray(1));
            this.NotifyChanged();
        }

        protected abstract byte[] GetDefaultValue();
    }
}