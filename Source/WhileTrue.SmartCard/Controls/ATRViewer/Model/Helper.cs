using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    static internal class Helper
    {
        public static void SetAsHexValue(string value, int minLength, int maxLength, Action<byte[]> setAction)
        {
            byte[] Value;
            try
            {
                Value = value.ToByteArray();
            }
            catch
            {
                throw new ArgumentException("not a hexadecimal value");
            }
            if (Value.Length >= minLength && Value.Length <= maxLength)
            {
                setAction(Value);
            }
            else
            {
                throw new ArgumentException(String.Format("length must be {0} bytes", 
                    minLength!=maxLength
                    ? string.Format("between {0} and {1}", minLength, maxLength)
                    : string.Format("{0}", minLength)
                    ));
            }
        }

        public static void SetAsHexUShortValue(string value, Action<ushort> setAction)
        {
            byte[] Value;
            try
            {
                Value = value.ToByteArray();
            }
            catch
            {
                throw new ArgumentException("not a hexadecimal value");
            }
            if (Value.Length == 2)
            {
                setAction(Conversion.ToUInt16(Value));
            }
            else
            {
                throw new ArgumentException("must be a hex value with 2 bytes");
            }
        }

        public static void SetAsHexByteValue(string value, Action<byte> setAction)
        {
            byte[] Value;
            try
            {
                Value = value.ToByteArray();
            }
            catch
            {
                throw new ArgumentException("not a hexadecimal value");
            }
            if (Value.Length == 1)
            {
                setAction(Conversion.ToUInt8(Value));
            }
            else
            {
                throw new ArgumentException("must be a hex value with 1 bytes");
            }
        }

        public static string GetExceptionMessage(this Exception exception)
        {
            if (exception is ArgumentException)
            {
                if (((ArgumentException) exception).ParamName != null)
                {
                    return exception.Message.Substring(0, exception.Message.LastIndexOf("\r\n", System.StringComparison.Ordinal));
                }
                else
                {
                    return exception.Message;
                }
            }
            else
            {
                return exception.Message;
            }
        }
    }
}