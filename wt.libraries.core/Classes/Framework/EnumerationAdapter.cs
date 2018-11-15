using System;
using System.Linq;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    ///     Provides a wrapper for enumerations that support string names and descriptions for each enumeration item
    /// </summary>
    /// <remarks>
    ///     Use <see cref="Items" /> property inside a static constructore to initialize the item descriptions
    /// </remarks>
    public class EnumerationAdapter<TEnumeration> where TEnumeration : struct
    {
        static EnumerationAdapter()
        {
            Items = new EnumerationAdapter<TEnumeration>[0];
        }

        /// <summary>
        ///     Create a wrapper for an enumeration value. us ein conjunction with <see cref="Items" />
        /// </summary>
        public EnumerationAdapter(TEnumeration value, string name, string description)
        {
            Value = value;
            Name = name;
            Description = description;
        }

        /// <summary>
        ///     Used to set the wrappers for each enum value. Use inside a static constructor
        /// </summary>
        public static EnumerationAdapter<TEnumeration>[] Items { get; set; }

        /// <summary>
        ///     Enumeration value that is wrapped by this instance
        /// </summary>
        public TEnumeration Value { get; }

        /// <summary>
        ///     Readable name of the enum value
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Description of the enum value
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        ///     Allows implicit conversion between the source enumeration and the corresponding adapter
        /// </summary>
        public static implicit operator EnumerationAdapter<TEnumeration>(TEnumeration value)
        {
            return GetInstanceFor(value);
        }

        /// <summary>
        ///     Allows implicit conversion between the source enumeration and the corresponding adapter
        /// </summary>
        public static implicit operator TEnumeration(EnumerationAdapter<TEnumeration> value)
        {
            return value.Value;
        }


        /// <summary>
        ///     Retrieves the instance that wraps the given enumeration value
        /// </summary>
        public static EnumerationAdapter<TEnumeration> GetInstanceFor(TEnumeration value)
        {
            var Wrapper = (from Item in Items where Equals(Item.Value, value) select Item).FirstOrDefault();
            if (Wrapper == null)
                throw new ArgumentException(
                    $"EnumerationAdapter for value {Enum.GetName(typeof(TEnumeration), value)} is not defined. Add EnumerationAdapter.Items for enum type {typeof(TEnumeration)} to your model");
            return Wrapper;
        }

        /// <summary>
        ///     Retrieves the instance that wraps the given enumeration value
        /// </summary>
        public static EnumerationAdapter<TEnumeration> GetInstanceFor(TEnumeration? value)
        {
            if (value.HasValue)
            {
                var Wrapper = (from Item in Items where Equals(Item.Value, value) select Item).FirstOrDefault();
                if (Wrapper == null)
                    throw new ArgumentException(
                        $"EnumerationAdapter for value {Enum.GetName(typeof(TEnumeration), value)} is not defined. Add EnumerationAdapter.Items for enum type {typeof(TEnumeration)} to your model");
                return Wrapper;
            }

            return null;
        }
    }
}