using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    ///     Provides utility functions for array handling
    /// </summary>
    public static class ArrayUtils
    {
        /// <summary>
        ///     returns true if all items in the array <c>a1</c> have the same
        ///     value as the items in <c>a2</c>
        /// </summary>
        public static bool HasEqualValue<TItemType>(this TItemType[] a1, TItemType[] a2,
            Func<TItemType, TItemType, bool> compare)
        {
            if (a1.Length != a2.Length) return false;
            for (var Index = 0; Index < a1.Length; Index++)
                if (!compare(a1[Index], a2[Index]))
                    return false;
            return true;
        }

        /// <summary>
        ///     returns true if all items in the array <c>a1</c> have the same
        ///     value as the items in <c>a2</c>
        /// </summary>
        public static bool HasEqualValue(this Array a1, Array a2)
        {
            if (a1.Length != a2.Length) return false;
            for (var Index = 0; Index < a1.Length; Index++)
                if (!Equals(a1.GetValue(Index), a2.GetValue(Index)))
                    return false;
            return true;
        }

        /// <summary>
        ///     Returns a subarray of the array.
        /// </summary>
        /// <param name="array">array to work on</param>
        /// <param name="offset">offset the array should be copied from</param>
        /// <returns></returns>
        public static TArrayType[] GetSubArray<TArrayType>(this TArrayType[] array, int offset)
        {
            return array.GetSubArray(offset, array.Length - offset);
        }

        /// <summary>
        ///     Returns a subarray of the array.
        /// </summary>
        /// <param name="array">array to work on</param>
        /// <param name="offset">offset the array should be copied from</param>
        /// <param name="length">
        ///     length to copy. If length is negative, the array is copied to the nth last element, where n is
        ///     <c>-index</c>
        /// </param>
        /// <returns></returns>
        public static TArrayType[] GetSubArray<TArrayType>(this TArrayType[] array, int offset, int length)
        {
            if (length < 0) length = array.Length - offset - -length;
            var Temp = new TArrayType[length];
            Array.Copy(array, offset, Temp, 0, length);
            return Temp;
        }

        /// <summary>
        ///     Returns <c>true</c> if the value given is contained in the array
        /// </summary>
        public static bool Contains<TArrayType>(this TArrayType[] array, TArrayType value)
        {
            return Array.IndexOf(array, value) != -1;
        }

        /// <summary>
        ///     Converts all items of the enumeration given into a new type using the given delegate
        /// </summary>
        public static IEnumerable<TReturnType> ConvertTo<TSourceType, TReturnType>(this IEnumerable<TSourceType> values,
            Func<TSourceType, TReturnType> convertDelegate)
        {
            return from Value in values select convertDelegate(Value);
        }

        /// <summary>
        ///     Converts all items of the array given into a new type using the given delegate
        /// </summary>
        public static TReturnType[] ConvertTo<TSourceType, TReturnType>(this TSourceType[] values,
            Func<TSourceType, TReturnType> convertDelegate)
        {
            return ((IEnumerable<TSourceType>) values).ConvertTo(convertDelegate).ToArray();
        }

        /// <summary>
        ///     Flattens a enumeration of enumerations of the smae type into a flat list
        /// </summary>
        public static IEnumerable<TArrayType> Flatten<TArrayType>(this IEnumerable<IEnumerable<TArrayType>> value)
        {
            return from Enumeration in value
                from Value in Enumeration
                select Value;
        }

        /// <summary>
        ///     Concatenates the two arrays into one
        /// </summary>
        public static T[] Concat<T>(T[] data1, T[] data2)
        {
            if (data1 == null) return data2;
            if (data2 == null) return data1;
            var Data = new T[data1.Length + data2.Length];
            Array.Copy(data1, 0, Data, 0, data1.Length);
            Array.Copy(data2, 0, Data, data1.Length, data2.Length);

            return Data;
        }

        /// <summary>
        ///     Executes the given <c>action</c> on each item of the enumeration
        /// </summary>
        public static void ForEach<TArrayType>(this IEnumerable<TArrayType> sequence, Action<TArrayType> action)
        {
            foreach (var Item in sequence) action(Item);
        }

        /// <summary>
        ///     Executes the given <c>actionAsync</c> on each item of the enumeration
        /// </summary>
        public static async Task ForEachAsync<TArrayType>(this IEnumerable<TArrayType> sequence,
            Func<TArrayType, Task> actionAsync)
        {
            foreach (var Item in sequence) await actionAsync(Item);
        }

        /// <summary>
        ///     Executes the given <c>action</c> on each item of the enumeration
        /// </summary>
        public static void ForEach(this IEnumerable sequence, Action<object> action)
        {
            foreach (var Item in sequence) action(Item);
        }
    }
}