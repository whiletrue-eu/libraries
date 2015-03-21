using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class ProtocolParameterByteValueAdapter : ProtocolParameterByteAdapterBase
    {
        public string Value { get; private set; }

        public ProtocolParameterByteValueAdapter(byte value)
        {
            this.Value = value.ToHexString();
        }
    }
}