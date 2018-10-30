using System.Diagnostics;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    ///     Implements a key that is composed of one instances. this class can be used if keys cannot be null but it is a
    ///     requirement to have a null key
    /// </summary>
    [PublicAPI]
    public class ObjectCacheKey<TParam1Type>
    {
        private readonly TParam1Type param1;

        /// <summary />
        public ObjectCacheKey(TParam1Type param1)
        {
            this.param1 = param1;
        }

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return Equals(param1, default(TParam1Type)) ? 0 : param1.GetHashCode();
        }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="other">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object other)
        {
            Debug.Assert(other is ObjectCacheKey<TParam1Type>);

            var Other = (ObjectCacheKey<TParam1Type>) other;

            return
                Equals(param1, Other.param1);
        }
    }

    /// <summary>
    ///     Implements a key that is composed of two instances. the key is equal if both instances are equal.
    /// </summary>
    [PublicAPI]
    public class ObjectCacheKey<TParam1Type, TParam2Type>
    {
        /// <summary />
        public ObjectCacheKey(TParam1Type param1, TParam2Type param2)
        {
            Param1 = param1;
            Param2 = param2;
        }

        /// <summary />
        public TParam1Type Param1 { get; }

        /// <summary />
        public TParam2Type Param2 { get; }

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return
                (Equals(Param1, default(TParam1Type)) ? 0 : Param1.GetHashCode()) ^
                (Equals(Param2, default(TParam2Type)) ? 0 : Param2.GetHashCode());
        }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="other">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object other)
        {
            Debug.Assert(other is ObjectCacheKey<TParam1Type, TParam2Type>);

            var Other = (ObjectCacheKey<TParam1Type, TParam2Type>) other;

            return
                Equals(Param1, Other.Param1) &&
                Equals(Param2, Other.Param2);
        }
    }

    /// <summary>
    ///     Implements a key that is composed of three instances. the key is equal if all instances are equal.
    /// </summary>
    [PublicAPI]
    public class ObjectCacheKey<TParam1Type, TParam2Type, TParam3Type>
    {
        /// <summary />
        public ObjectCacheKey(TParam1Type param1, TParam2Type param2, TParam3Type param3)
        {
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
        }

        /// <summary />
        public TParam1Type Param1 { get; }

        /// <summary />
        public TParam2Type Param2 { get; }

        /// <summary />
        public TParam3Type Param3 { get; }

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return
                (Equals(Param1, default(TParam1Type)) ? 0 : Param1.GetHashCode()) ^
                (Equals(Param2, default(TParam2Type)) ? 0 : Param2.GetHashCode()) ^
                (Equals(Param3, default(TParam3Type)) ? 0 : Param3.GetHashCode());
        }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="other">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object other)
        {
            Debug.Assert(other is ObjectCacheKey<TParam1Type, TParam2Type, TParam3Type>);

            var Other = (ObjectCacheKey<TParam1Type, TParam2Type, TParam3Type>) other;

            return
                Equals(Param1, Other.Param1) &&
                Equals(Param2, Other.Param2) &&
                Equals(Param3, Other.Param3);
        }
    }

    /// <summary>
    ///     Implements a key that is composed of four instances. the key is equal if all instances are equal.
    /// </summary>
    [PublicAPI]
    public class ObjectCacheKey<TParam1Type, TParam2Type, TParam3Type, TParam4Type>
    {
        /// <summary />
        public ObjectCacheKey(TParam1Type param1, TParam2Type param2, TParam3Type param3, TParam4Type param4)
        {
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            Param4 = param4;
        }

        /// <summary />
        public TParam1Type Param1 { get; }

        /// <summary />
        public TParam2Type Param2 { get; }

        /// <summary />
        public TParam3Type Param3 { get; }

        /// <summary />
        public TParam4Type Param4 { get; }

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return
                (Equals(Param1, default(TParam1Type)) ? 0 : Param1.GetHashCode()) ^
                (Equals(Param2, default(TParam2Type)) ? 0 : Param2.GetHashCode()) ^
                (Equals(Param3, default(TParam3Type)) ? 0 : Param3.GetHashCode()) ^
                (Equals(Param4, default(TParam4Type)) ? 0 : Param4.GetHashCode());
        }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="other">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object other)
        {
            Debug.Assert(other is ObjectCacheKey<TParam1Type, TParam2Type, TParam3Type, TParam4Type>);

            var Other = (ObjectCacheKey<TParam1Type, TParam2Type, TParam3Type, TParam4Type>) other;

            return
                Equals(Param1, Other.Param1) &&
                Equals(Param2, Other.Param2) &&
                Equals(Param3, Other.Param3) &&
                Equals(Param4, Other.Param4);
        }
    }
}