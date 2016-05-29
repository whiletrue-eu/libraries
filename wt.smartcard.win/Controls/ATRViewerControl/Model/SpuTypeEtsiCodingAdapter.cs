using System.Collections.Generic;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class SpuTypeEtsiCodingAdapter : ObservableObject
    {
        private readonly GlobalInterfaceBytes globalInterfaceBytes;
        private readonly SpuTypeEtsiCoding etsiCoding;
        private static readonly ReadOnlyPropertyAdapter<SpuTypeEtsiCodingAdapter, bool> lowImpedanceOnIoLineAvailableAdapter;
        private static readonly ReadOnlyPropertyAdapter<SpuTypeEtsiCodingAdapter, bool> interChipUsbSupportedAdapter;
        private static readonly ReadOnlyPropertyAdapter<SpuTypeEtsiCodingAdapter, bool> clfInterfaceSupportedAdapter;
        private static readonly ReadOnlyPropertyAdapter<SpuTypeEtsiCodingAdapter, EnumerationAdapter<EtsiSpuSecureChannelSupport>> secureChannelSupportAdapter;
        
        static SpuTypeEtsiCodingAdapter()
        {
            IPropertyAdapterFactory<SpuTypeEtsiCodingAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<SpuTypeEtsiCodingAdapter>();

            SpuTypeEtsiCodingAdapter.lowImpedanceOnIoLineAvailableAdapter = PropertyFactory.Create(
                nameof(SpuTypeEtsiCodingAdapter.LowImpedanceOnIoLineAvailable),
                instance => instance.etsiCoding.LowImpedanceOnIoLineAvailable
                );

            SpuTypeEtsiCodingAdapter.interChipUsbSupportedAdapter = PropertyFactory.Create(
                nameof(SpuTypeEtsiCodingAdapter.InterChipUsbSupported),
                instance => instance.etsiCoding.InterChipUsbSupported
                );

            SpuTypeEtsiCodingAdapter.clfInterfaceSupportedAdapter = PropertyFactory.Create(
                nameof(SpuTypeEtsiCodingAdapter.ClfInterfaceSupported),
                instance => instance.etsiCoding.ClfInterfaceSupported
                );

            SpuTypeEtsiCodingAdapter.secureChannelSupportAdapter = PropertyFactory.Create(
                nameof(SpuTypeEtsiCodingAdapter.SecureChannelSupport),
                instance => EnumerationAdapter<EtsiSpuSecureChannelSupport>.GetInstanceFor(instance.etsiCoding.SecureChannelSupport)
                );
    
        }

        public SpuTypeEtsiCodingAdapter(GlobalInterfaceBytes globalInterfaceBytes, SpuTypeEtsiCoding etsiCoding)
        {
            this.globalInterfaceBytes = globalInterfaceBytes;
            this.etsiCoding = etsiCoding;
        }

        public EnumerationAdapter<EtsiSpuSecureChannelSupport> SecureChannelSupport
        {
            get { return SpuTypeEtsiCodingAdapter.secureChannelSupportAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpu(SpuUse.Proprietary, SpuTypeEtsiCoding.GetSpuTypeEtsiCodingAsByte(
                    this.etsiCoding.LowImpedanceOnIoLineAvailable,
                    this.etsiCoding.InterChipUsbSupported,
                    this.etsiCoding.ClfInterfaceSupported,
                    value
                    ));
            }
        }

        public IEnumerable<EnumerationAdapter<EtsiSpuSecureChannelSupport>> SecureChannelSupportValues => EnumerationAdapter<EtsiSpuSecureChannelSupport>.Items;

        public bool ClfInterfaceSupported
        {
            get { return SpuTypeEtsiCodingAdapter.clfInterfaceSupportedAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpu(SpuUse.Proprietary, SpuTypeEtsiCoding.GetSpuTypeEtsiCodingAsByte(
                    this.etsiCoding.LowImpedanceOnIoLineAvailable,
                    this.etsiCoding.InterChipUsbSupported,
                    value,
                    this.etsiCoding.SecureChannelSupport
                    ));
            }
        }

        public bool InterChipUsbSupported
        {
            get { return SpuTypeEtsiCodingAdapter.interChipUsbSupportedAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpu(SpuUse.Proprietary, SpuTypeEtsiCoding.GetSpuTypeEtsiCodingAsByte(
                    this.etsiCoding.LowImpedanceOnIoLineAvailable,
                    value,
                    this.etsiCoding.ClfInterfaceSupported,
                    this.etsiCoding.SecureChannelSupport
                    ));
            }
        }

        public bool LowImpedanceOnIoLineAvailable
        {
            get { return SpuTypeEtsiCodingAdapter.lowImpedanceOnIoLineAvailableAdapter.GetValue(this); }
            set
            {
                this.globalInterfaceBytes.SetSpu(SpuUse.Proprietary, SpuTypeEtsiCoding.GetSpuTypeEtsiCodingAsByte(
                    value,
                    this.etsiCoding.InterChipUsbSupported,
                    this.etsiCoding.ClfInterfaceSupported,
                    this.etsiCoding.SecureChannelSupport
                    ));
            }
        }

    }
}