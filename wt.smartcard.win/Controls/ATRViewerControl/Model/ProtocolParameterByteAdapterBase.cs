using System;
using WhileTrue.Classes.ATR;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class ProtocolParameterByteAdapterBase
    {
        public static ProtocolParameterByteAdapterBase GetObject(ParameterByte parameterByte)
        {
            switch (parameterByte.HasValue)
            {
                case ParameterByte.ValueIndicator.Set:
                    return new ProtocolParameterByteValueAdapter(parameterByte.Value);
                case ParameterByte.ValueIndicator.Unset:
                    return new UnsetProtocolParameterByteAdapter();
                case ParameterByte.ValueIndicator.Irrelevant:
                    return new IrrelevantProtocolParameterByteAdapter();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}