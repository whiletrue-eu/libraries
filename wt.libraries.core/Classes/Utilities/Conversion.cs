using System.Linq;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    ///     Provides utility methods for data conversion
    /// </summary>
    public static class Conversion
    {
        /// <summary />
        public static string ToHexString(this IEnumerable<byte> data)
        {
            return ToHexString(data, "");
        }

        /// <summary />
        public static string ToHexString(this IEnumerable<byte> data, string separator)
        {
            return ToHexString(data.ToArray(), separator);
        }

        /// <summary />
        public static string ToHexString(this byte[] data)
        {
            return ToHexString(data, "");
        }

        /// <summary />
        public static string ToHexString(this byte[] data, string separator)
        {
            if (data != null)
            {
                var Bytes = new List<string>(data.Length);
                foreach (var Byte in data) Bytes.Add($"{Byte:X2}");
                return string.Join(separator, Bytes.ToArray());
            }

            return "";
        }

        /// <summary />
        public static string ToHexString(this byte data)
        {
            return ToHexString(new[] {data}, "");
        }

        /// <summary />
        public static bool CanConvertToByteArray(this string hexData)
        {
            if (hexData != null) return Regex.IsMatch(hexData.Replace(" ", ""), "^([a-fA-F0-9]{2})*$");
            return false;
        }

        /// <summary />
        public static byte[] ToByteArray(this string hexData)
        {
            hexData = hexData.Replace(" ", "");
            if (hexData.Length % 2 != 0) throw new Exception("HexAscii string must be of even length!");

            var Data = new List<byte>();
            for (var Index = 0; Index < hexData.Length; Index += 2)
                Data.Add(Convert.ToByte(hexData.Substring(Index, 2), 16));
            return Data.ToArray();
        }

        /// <summary />
        public static byte[] ToAsciiBytes(this string asciiData)
        {
            var Data = new List<byte>();
            for (var Index = 0; Index < asciiData.Length; Index++) Data.Add((byte) asciiData[Index]);

            return Data.ToArray();
        }

        /// <summary />
        public static byte[] ToByteArray(this uint value)
        {
            var Value = new byte[4];

            Value[3] = (byte) (value & 0xFF);
            Value[2] = (byte) ((value >> 8) & 0xFF);
            Value[1] = (byte) ((value >> 16) & 0xFF);
            Value[0] = (byte) ((value >> 24) & 0xFF);

            return Value;
        }

        /// <summary />
        public static byte[] ToByteArray(this ushort value)
        {
            var Value = new byte[2];

            Value[3] = (byte) (value & 0xFF);
            Value[2] = (byte) ((value >> 8) & 0xFF);

            return Value;
        }

        /// <summary />
        public static byte ToBcd(this int value)
        {
            if (value > 99 || value < 0) throw new ArgumentOutOfRangeException();

            var Bcd = (byte) ((value / 10) << 4);
            Bcd |= (byte) (value % 10);

            return Bcd;
        }


        /// <summary />
        public static int ToInt32(byte[] value, int offset = 0)
        {
            if (value.Length < offset + 4)
                throw new ArgumentException("not enough data to convert to (u)int", nameof(offset));

            return (value[offset] << 24) | (value[offset + 1] << 16) | (value[offset + 2] << 8) | value[offset + 3];
        }

        /// <summary />
        public static uint ToUInt32(byte[] value, int offset = 0)
        {
            unchecked
            {
                return (uint) ToInt32(value, offset);
            }
        }

        /// <summary />
        public static short ToInt16(byte[] value, int offset = 0)
        {
            if (value.Length < offset + 2)
                throw new ArgumentException("not enough data to convert to (u)short", nameof(offset));

            return (short) ((value[offset] << 8) | value[offset + 1]);
        }

        /// <summary />
        public static ushort ToUInt16(byte[] value, int offset = 0)
        {
            unchecked
            {
                return (ushort) ToInt16(value, offset);
            }
        }

        /// <summary />
        public static byte ToUInt8(byte[] value, int offset = 0)
        {
            if (value.Length < offset + 1)
                throw new ArgumentException("not enough data to convert to (s)byte", nameof(offset));

            return value[offset];
        }

        /// <summary />
        public static sbyte ToInt8(byte[] value, int offset = 0)
        {
            unchecked
            {
                return (sbyte) ToUInt8(value, offset);
            }
        }


        /// <summary />
        public static byte[] ToByteArray(this int value)
        {
            unchecked
            {
                return ToByteArray((uint) value);
            }
        }

        /// <summary />
        public static byte[] ToByteArray(this short value)
        {
            unchecked
            {
                return ToByteArray((ushort) value);
            }
        }

        /// <summary />
        public static TArgetType ChangeType<TArgetType>(object value)
        {
            return (TArgetType) ChangeType(value, typeof(TArgetType));
        }

        /// <summary />
        public static object ChangeType(object value, Type targetType)
        {
            if (value == null && targetType.IsByRef) return null;

            var SourceType = value?.GetType();

            if (SourceType != null && targetType.IsAssignableFrom(SourceType))
                return value;
            throw new InvalidOperationException(
                $"value of type {(SourceType != null ? SourceType.FullName : "<null>")} cannot be converted to type {targetType.FullName}");
            /* }
                }*/
        }
    }
}