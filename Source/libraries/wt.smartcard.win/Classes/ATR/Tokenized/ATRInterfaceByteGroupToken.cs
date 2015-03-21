using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR.Tokenized
{
    public class AtrInterfaceByteGroupToken : ObservableObject,IAtrToken
    {
        private readonly TokenizedAtr owner;
        private byte? ta;
        private byte? tb;
        private byte? tc;
        private AtrInterfaceByteGroupToken nextGroup;
        private static readonly ReadOnlyPropertyAdapter<AtrInterfaceByteGroupToken, int> numberAdapter;

        static AtrInterfaceByteGroupToken()
        {
            AtrInterfaceByteGroupToken.numberAdapter = ObservableObject.GetPropertyAdapterFactory<AtrInterfaceByteGroupToken>().Create(
                nameof(AtrInterfaceByteGroupToken.Number),
                instance => instance.owner.InterfaceByteGroups.TakeWhile(_ => _ != instance).Count() + 1
                );
        }

        internal AtrInterfaceByteGroupToken(TokenizedAtr owner, AtrReadStream atr, NextInterfaceBytesIndicator nextInterfaceBytesIndicator)
        {
            this.owner = owner;
            this.InterfaceBytesIndicator = nextInterfaceBytesIndicator;
            this.InterfaceBytesIndicator.PropertyChanged += this.interfaceBytesIndicatorForThisGroup_PropertyChanged;

            if( nextInterfaceBytesIndicator.TaExists )
            {
                this.ta = atr.GetNextByte();
            }
            if (nextInterfaceBytesIndicator.TbExists)
            {
                this.tb = atr.GetNextByte();
            }
            if (nextInterfaceBytesIndicator.TcExists)
            {
                this.tc = atr.GetNextByte();
            }
        }

        private void interfaceBytesIndicatorForThisGroup_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.InvokePropertyChanged(nameof(AtrInterfaceByteGroupToken.Type));
            this.owner.NotifyChanged();
        }

        private void interfaceBytesIndicatorForNextGroup_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.InvokePropertyChanged(nameof(AtrInterfaceByteGroupToken.Bytes));
            this.owner.NotifyChanged();
        }

        public byte[] Bytes
        {
            get
            {
                List<byte> Bytes = new List<byte>();
                if( this.Ta.HasValue )
                {
                    Bytes.Add(this.Ta.Value);
                }
                if (this.Tb.HasValue)
                {
                    Bytes.Add(this.Tb.Value);
                }
                if (this.Tc.HasValue)
                {
                    Bytes.Add(this.Tc.Value);
                }
                if( this.NextInterfaceBytesIndicator !=null)
                {
                    Bytes.Add(this.NextInterfaceBytesIndicator.GetAsByte((byte) this.NextGroup.Type));
                }
                return Bytes.ToArray();
            }
        }

        public NextInterfaceBytesIndicator NextInterfaceBytesIndicator => this.nextGroup != null ? this.NextGroup.InterfaceBytesIndicator : null;

        internal NextInterfaceBytesIndicator InterfaceBytesIndicator { get; }

        public InterfaceByteGroupType Type => this.InterfaceBytesIndicator.GroupType;

        public byte? Ta
        {
            get { return this.ta; }
            set
            {
                this.SetAndInvoke(ref this.ta, value, 
                    delegate
                    {
                        this.InterfaceBytesIndicator.TaExists = value == null ? false : true;
                        this.owner.NotifyChanged();
                        this.InvokePropertyChanged(nameof(AtrInterfaceByteGroupToken.Bytes));
                    });
            }
        }

        public byte? Tb
        {
            get { return this.tb; }
            set
            {
                this.SetAndInvoke(ref this.tb, value, 
                    delegate
                    {
                        this.InterfaceBytesIndicator.TbExists = value == null ? false : true;
                        this.owner.NotifyChanged();
                        this.InvokePropertyChanged(nameof(AtrInterfaceByteGroupToken.Bytes));
                    });
            }
        }

        public byte? Tc
        {
            get { return this.tc; }
            set
            {
                this.SetAndInvoke(ref this.tc, value, 
                    delegate
                    {
                        this.InterfaceBytesIndicator.TcExists = value != null;
                        this.owner.NotifyChanged();
                        this.InvokePropertyChanged(nameof(AtrInterfaceByteGroupToken.Bytes));
                    });
            }
        }

        public AtrInterfaceByteGroupToken NextGroup
        {
            get { return this.nextGroup; }
            internal set
            {
                if (this.nextGroup != null)
                {
                    this.nextGroup.InterfaceBytesIndicator.PropertyChanged -= this.interfaceBytesIndicatorForNextGroup_PropertyChanged;
                }
                this.SetAndInvoke(ref this.nextGroup, value,
                    delegate
                    {

                        this.InvokePropertyChanged(nameof(AtrInterfaceByteGroupToken.Bytes));
                        this.InvokePropertyChanged(nameof(AtrInterfaceByteGroupToken.NextInterfaceBytesIndicator));
                    });
                if (this.nextGroup != null)
                {
                    this.nextGroup.InterfaceBytesIndicator.PropertyChanged += this.interfaceBytesIndicatorForNextGroup_PropertyChanged;
                }
            }
        }

        public int Number => AtrInterfaceByteGroupToken.numberAdapter.GetValue(this);


        public void AddGroup(InterfaceByteGroupType type)
        {
            this.owner.InterfaceByteGroups.AppendGroup(this, type);
        }

        public void RemoveNextGroup()
        {
            DbC.AssureNotNull(this.NextGroup);
            this.owner.InterfaceByteGroups.Remove(this.NextGroup);
        }
    }
}