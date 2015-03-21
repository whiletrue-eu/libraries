using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    /// Provides utility functions for array handling
    /// </summary>
    public static class ArrayUtils
    {
        /// <summary>
        /// returns true if all items in the array <c>a1</c> have the same
        /// value as the items in <c>a2</c>
        /// </summary>
        public static bool HasEqualValue<ItemType>(this ItemType[] a1, ItemType[] a2, Func<ItemType, ItemType, bool> compare)
        {
            if (a1.Length != a2.Length)
            {
                return false;
            }
            for (int Index = 0; Index < a1.Length; Index++)
            {
                if (! compare(a1[Index], a2[Index]))
                {
                    return false;
                }
            }
            return true;
        }  
        
        /// <summary>
        /// returns true if all items in the array <c>a1</c> have the same
        /// value as the items in <c>a2</c>
        /// </summary>
        public static bool HasEqualValue(this Array a1, Array a2)
        {
            if (a1.Length != a2.Length)
            {
                return false;
            }
            for (int Index = 0; Index < a1.Length; Index++)
            {
                if (! Equals(a1.GetValue(Index), a2.GetValue(Index)))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a subarray of the array.
        /// </summary>
        /// <param name="array">array to work on</param>
        /// <param name="offset">offset the array should be copied from</param>
        /// <returns></returns>
        public static ArrayType[] GetSubArray<ArrayType>(this ArrayType[] array, int offset)
        {
            return GetSubArray(array, offset, array.Length - offset);
        }

        /// <summary>
        /// Returns a subarray of the array.
        /// </summary>
        /// <param name="array">array to work on</param>
        /// <param name="offset">offset the array should be copied from</param>
        /// <param name="length">length to copy. If length is negative, the array is copied to the nth last element, where n is <c>-index</c></param>
        /// <returns></returns>
        public static ArrayType[] GetSubArray<ArrayType>(this ArrayType[] array, int offset, int length)
        {
            if( length < 0 )
            {
                length = array.Length - offset - (-length);
            }
            ArrayType[] Temp = new ArrayType[length];
            Array.Copy(array, offset, Temp, 0, length);
            return Temp;
        }

        ///<summary>
        /// Returns <c>true</c> if the value given is contained in the array
        ///</summary>
        public static bool Contains<ArrayType>(this ArrayType[] array, ArrayType value)
        {
            return Array.IndexOf(array, value) != -1;
        }

        ///<summary>
        /// Converts all items of the enumeration given into a new type using the given delegate
        ///</summary>
        public static IEnumerable<ReturnType> ConvertTo<SourceType,ReturnType>(this IEnumerable<SourceType> values, Func<SourceType,ReturnType> convertDelegate)
        {
            return from Value in values select convertDelegate(Value);
        }

        ///<summary>
        /// Converts all items of the array given into a new type using the given delegate
        ///</summary>
        public static ReturnType[] ConvertTo<SourceType, ReturnType>(this SourceType[] values, Func<SourceType, ReturnType> convertDelegate)
        {
            return ConvertTo((IEnumerable<SourceType>)values, convertDelegate).ToArray();
        }

        /// <summary>
        /// Flattens a enumeration of enumerations of the smae type into a flat list
        /// </summary>
        public static IEnumerable<ArrayType> Flatten<ArrayType>(this IEnumerable<IEnumerable<ArrayType>> value)
        {
            return from Enumeration in value
                   from Value in Enumeration
                   select Value; 
        }

        /// <summary>
        /// Executes the given <c>action</c> on each item of the enumeration
        /// </summary>
        public static void ForEach<ArrayType>(this IEnumerable<ArrayType> sequence, Action<ArrayType> action)
        {
            foreach (ArrayType Item in sequence)
            {
                action(Item);
            }
        }

        /// <summary>
        /// Executes the given <c>action</c> on each item of the enumeration
        /// </summary>
        public static void ForEach(this IEnumerable sequence, Action<object> action)
        {
            foreach (object Item in sequence)
            {
                action(Item);
            }
        }
    }
}