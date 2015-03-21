using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR.Tokenized
{
    public class AtrInterfaceByteGroupToken : ObservableObject,IAtrToken
    {
        private readonly TokenizedAtr owner;
        private readonly NextInterfaceBytesIndicator interfaceBytesIndicatorForThisGroup;
        private byte? ta;
        private byte? tb;
        private byte? tc;
        private AtrInterfaceByteGroupToken nextGroup;
        private static readonly ReadOnlyPropertyAdapter<AtrInterfaceByteGroupToken, int> numberAdapter;

        static AtrInterfaceByteGroupToken()
        {
            numberAdapter = GetPropertyAdapterFactory<AtrInterfaceByteGroupToken>().Create(
                @this=>@this.Number,
                @this => @this.owner.InterfaceByteGroups.TakeWhile(_ => _ != @this).Count() + 1
                );
        }

        internal AtrInterfaceByteGroupToken(TokenizedAtr owner, AtrReadStream atr, NextInterfaceBytesIndicator nextInterfaceBytesIndicator)
        {
            this.owner = owner;
            this.interfaceBytesIndicatorForThisGroup = nextInterfaceBytesIndicator;
            this.interfaceBytesIndicatorForThisGroup.PropertyChanged += interfaceBytesIndicatorForThisGroup_PropertyChanged;

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
            this.InvokePropertyChanged(() => Type);
            this.owner.NotifyChanged();
        }

        private void interfaceBytesIndicatorForNextGroup_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.InvokePropertyChanged(() => Bytes);
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

        public NextInterfaceBytesIndicator NextInterfaceBytesIndicator
        {
            get
            {
                return this.nextGroup != null ? this.NextGroup.InterfaceBytesIndicator : null;
            }
        }

        internal NextInterfaceBytesIndicator InterfaceBytesIndicator
        {
            get { return this.interfaceBytesIndicatorForThisGroup; }
        }

        public InterfaceByteGroupType Type
        {
            get { return this.interfaceBytesIndicatorForThisGroup.GroupType; }
        }

        public byte? Ta
        {
            get { return this.ta; }
            set
            {
                this.SetAndInvoke(() => Ta, ref this.ta, value, null,
                    delegate
                    {
                        this.interfaceBytesIndicatorForThisGroup.TaExists = value == null ? false : true;
                        this.owner.NotifyChanged();
                        this.InvokePropertyChanged(()=>Bytes);
                    });
            }
        }

        public byte? Tb
        {
            get { return this.tb; }
            set
            {
                this.SetAndInvoke(() => Tb, ref this.tb, value, null,
                    delegate
                    {
                        this.interfaceBytesIndicatorForThisGroup.TbExists = value == null ? false : true;
                        this.owner.NotifyChanged();
                        this.InvokePropertyChanged(() => Bytes);
                    });
            }
        }

        public byte? Tc
        {
            get { return this.tc; }
            set
            {
                this.SetAndInvoke(() => Tc, ref this.tc, value, null,
                    delegate
                    {
                        this.interfaceBytesIndicatorForThisGroup.TcExists = value == null ? false : true;
                        this.owner.NotifyChanged();
                        this.InvokePropertyChanged(() => Bytes);
                    });
            }
        }

        public AtrInterfaceByteGroupToken NextGroup
        {
            get { return this.nextGroup; }
            internal set
            {

                this.SetAndInvoke(() => NextGroup, ref this.nextGroup, value,
                    delegate
                    {
                        if (this.nextGroup != null)
                        {
                            this.nextGroup.InterfaceBytesIndicator.PropertyChanged -= interfaceBytesIndicatorForNextGroup_PropertyChanged;
                        }
                    },
                    delegate
                    {
                        if (this.nextGroup != null)
                        {
                            this.nextGroup.InterfaceBytesIndicator.PropertyChanged += interfaceBytesIndicatorForNextGroup_PropertyChanged;
                        }
                        this.InvokePropertyChanged(() => Bytes);
                        this.InvokePropertyChanged(() => NextInterfaceBytesIndicator);
                    });

            }
        }

        public int Number{ get { return numberAdapter.GetValue(this); } }


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