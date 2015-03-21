using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Provides a wrapper for enumerations that support string names and descriptions for each enumeration item
    /// </summary>
    /// <remarks>
    /// Use <see cref="Items"/> property inside a static constructore to initialize the item descriptions
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public class EnumerationAdapter<TEnumeration> where TEnumeration : struct
    {
        static EnumerationAdapter()
        {
            Items = new EnumerationAdapter<TEnumeration>[0];
        }

        /// <summary>
        /// Used to set the wrappers for each enum value. Use inside a static constructor
        /// </summary>
        public static EnumerationAdapter<TEnumeration>[] Items { get; set; }

        /// <summary>
        /// Enumeration value that is wrapped by this instance
        /// </summary>
        public TEnumeration Value { get; private set; }
        /// <summary>
        /// Readable name of the enum value
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Description of the enum value
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Create a wrapper for an enumeration value. us ein conjunction with <see cref="Items"/>
        /// </summary>
        public EnumerationAdapter(TEnumeration value, string name, string description)
        {
            this.Value = value;
            this.Name = name;
            this.Description = description;
        }

        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Allows implicit conversion between the source enumeration and the corresponding adapter
        /// </summary>
        public static implicit operator EnumerationAdapter<TEnumeration>( TEnumeration value )
        {
            return GetInstanceFor(value);
        }

        /// <summary>
        /// Allows implicit conversion between the source enumeration and the corresponding adapter
        /// </summary>
        public static implicit operator TEnumeration(EnumerationAdapter<TEnumeration> value)
        {
            return value.Value;
        }


        /// <summary>
        /// Retrieves the instance that wraps the given enumeration value
        /// </summary>
        public static EnumerationAdapter<TEnumeration> GetInstanceFor(TEnumeration value)
        {
            EnumerationAdapter<TEnumeration> Wrapper = (from Item in Items where Equals(Item.Value, value) select Item).FirstOrDefault();
            if( Wrapper == null )
            {
                throw new ArgumentException(string.Format("EnumerationAdapter for value {0} is not defined. Add EnumerationAdapter.Items for enum type {1} to your model", 
                    Enum.GetName(typeof(TEnumeration), value),
                    typeof(TEnumeration)));
            }
            else
            {
                return Wrapper;
            }
        }    
        /// <summary>
        /// Retrieves the instance that wraps the given enumeration value
        /// </summary>
        public static EnumerationAdapter<TEnumeration> GetInstanceFor(TEnumeration? value)
        {
            if (value.HasValue)
            {
                EnumerationAdapter<TEnumeration> Wrapper = (from Item in Items where Equals(Item.Value, value) select Item).FirstOrDefault();
                if (Wrapper == null)
                {
                    throw new ArgumentException(string.Format("EnumerationAdapter for value {0} is not defined. Add EnumerationAdapter.Items for enum type {1} to your model",
                        Enum.GetName(typeof (TEnumeration), value),
                        typeof (TEnumeration)));
                }
                else
                {
                    return Wrapper;
                }
            }
            else
            {
                return null;
            }
        }
    }
}