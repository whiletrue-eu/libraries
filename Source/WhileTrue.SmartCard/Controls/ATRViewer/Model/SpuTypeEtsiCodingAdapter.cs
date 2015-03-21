using System.Collections.Generic;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class SpuTypeEtsiCodingAdapter : ObservableObject
    {
        private readonly GlobalInterfaceBytes globalInterfaceBytes;
        private readonly SpuTypeEtsiCoding etsiCoding;
        private static readonly ReadOnlyPropertyAdapter<SpuTypeEtsiCodingAdapter, bool> lowImpedanceOnIoLineAvailableAdapter;
        private static readonly ReadOnlyPropertyAdapter<SpuTypeEtsiCodingAdapter, bool> interChipUSBSupportedAdapter;
        private static readonly ReadOnlyPropertyAdapter<SpuTypeEtsiCodingAdapter, bool> clfInterfaceSupportedAdapter;
        private static readonly ReadOnlyPropertyAdapter<SpuTypeEtsiCodingAdapter, EnumerationAdapter<EtsiSpuSecureChannelSupport>> secureChannelSupportAdapter;
        
        static SpuTypeEtsiCodingAdapter()
        {
            IPropertyAdapterFactory<SpuTypeEtsiCodingAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<SpuTypeEtsiCodingAdapter>();

            lowImpedanceOnIoLineAvailableAdapter = PropertyFactory.Create(
                @this => @this.LowImpedanceOnIoLineAvailable,
                @this => @this.etsiCoding.LowImpedanceOnIoLineAvailable
                );

            interChipUSBSupportedAdapter = PropertyFactory.Create(
                @this => @this.InterChipUSBSupported,
                @this => @this.etsiCoding.InterChipUSBSupported
                );

            clfInterfaceSupportedAdapter = PropertyFactory.Create(
                @this => @this.ClfInterfaceSupported,
                @this => @this.etsiCoding.ClfInterfaceSupported
                );

            secureChannelSupportAdapter = PropertyFactory.Create(
                @this => @this.SecureChannelSupport,
                @this => EnumerationAdapter<EtsiSpuSecureChannelSupport>.GetInstanceFor(@this.etsiCoding.SecureChannelSupport)
                );
    
        }

        public SpuTypeEtsiCodingAdapter(GlobalInterfaceBytes globalInterfaceBytes, SpuTypeEtsiCoding etsiCoding)
        {
            this.globalInterfaceBytes = globalInterfaceBytes;
            this.etsiCoding = etsiCoding;
        }

        public EnumerationAdapter<EtsiSpuSecureChannelSupport> SecureChannelSupport
        {
            get { return secureChannelSupportAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpu(SPUUse.Proprietary, SpuTypeEtsiCoding.GetSpuTypeEtsiCodingAsByte(
                    this.etsiCoding.LowImpedanceOnIoLineAvailable,
                    this.etsiCoding.InterChipUSBSupported,
                    this.etsiCoding.ClfInterfaceSupported,
                    value
                    ));
            }
        }

        public IEnumerable<EnumerationAdapter<EtsiSpuSecureChannelSupport>> SecureChannelSupportValues
        {
            get { return EnumerationAdapter<EtsiSpuSecureChannelSupport>.Items; }
        }

        public bool ClfInterfaceSupported
        {
            get { return clfInterfaceSupportedAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpu(SPUUse.Proprietary, SpuTypeEtsiCoding.GetSpuTypeEtsiCodingAsByte(
                    this.etsiCoding.LowImpedanceOnIoLineAvailable,
                    this.etsiCoding.InterChipUSBSupported,
                    value,
                    this.etsiCoding.SecureChannelSupport
                    ));
            }
        }

        public bool InterChipUSBSupported
        {
            get { return interChipUSBSupportedAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpu(SPUUse.Proprietary, SpuTypeEtsiCoding.GetSpuTypeEtsiCodingAsByte(
                    this.etsiCoding.LowImpedanceOnIoLineAvailable,
                    value,
                    this.etsiCoding.ClfInterfaceSupported,
                    this.etsiCoding.SecureChannelSupport
                    ));
            }
        }

        public bool LowImpedanceOnIoLineAvailable
        {
            get { return lowImpedanceOnIoLineAvailableAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpu(SPUUse.Proprietary, SpuTypeEtsiCoding.GetSpuTypeEtsiCodingAsByte(
                    value,
                    this.etsiCoding.InterChipUSBSupported,
                    this.etsiCoding.ClfInterfaceSupported,
                    this.etsiCoding.SecureChannelSupport
                    ));
            }
        }

    }
}