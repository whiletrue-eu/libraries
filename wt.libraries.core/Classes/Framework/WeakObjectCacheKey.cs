using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Framework
{
    public static class WeakObjectCacheKeyHelper
    {
        public static object Unwrap<T>(WeakReference<T> reference) where T : class => reference.TryGetTarget(out T Target) ? Target : new object();
    }

    /// <summary>
    ///     Implements a key that is composed of one instances. this class can be used if keys cannot be null but it is a
    ///     requirement to have a null key
    /// </summary>
    [PublicAPI]
    public class WeakObjectCacheKey<TParam1Type> where TParam1Type:class
    {
        // ReSharper disable once StaticMemberInGenericType

        private readonly WeakReference<TParam1Type> param1;

        /// <summary />
        public WeakObjectCacheKey(TParam1Type param1)
        {
            this.param1 = new WeakReference<TParam1Type>(param1);
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
            object Param1 = WeakObjectCacheKeyHelper.Unwrap(this.param1);
            return Equals(Param1, default(TParam1Type)) ? 0 : Param1.GetHashCode();
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
            Debug.Assert(other is WeakObjectCacheKey<TParam1Type>);

            var Other = (WeakObjectCacheKey<TParam1Type>) other;
            object Param1 = WeakObjectCacheKeyHelper.Unwrap(this.param1);
            object OtherParam1 = WeakObjectCacheKeyHelper.Unwrap(Other.param1);

            return
                Equals(Param1, OtherParam1);
        }
    }

    /// <summary>
    ///     Implements a key that is composed of two instances. the key is equal if both instances are equal.
    /// </summary>
    [PublicAPI]
    public class WeakObjectCacheKey<TParam1Type, TParam2Type> where TParam1Type : class where TParam2Type : class
    {
        /// <summary />
        public WeakObjectCacheKey(TParam1Type param1, TParam2Type param2)
        {
            this.param1 = new WeakReference<TParam1Type>(param1);
            this.param2 = new WeakReference<TParam2Type>(param2);
        }

        /// <summary />
        private readonly WeakReference<TParam1Type> param1;

        /// <summary />
        private readonly WeakReference<TParam2Type> param2;

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            object Param1 = WeakObjectCacheKeyHelper.Unwrap(this.param1);
            object Param2 = WeakObjectCacheKeyHelper.Unwrap(this.param2);

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
            Debug.Assert(other is WeakObjectCacheKey<TParam1Type, TParam2Type>);

            var Other = (WeakObjectCacheKey<TParam1Type, TParam2Type>) other;
            object Param1 = WeakObjectCacheKeyHelper.Unwrap(this.param1);
            object OtherParam1 = WeakObjectCacheKeyHelper.Unwrap(Other.param1);
            object Param2 = WeakObjectCacheKeyHelper.Unwrap(this.param2);
            object OtherParam2 = WeakObjectCacheKeyHelper.Unwrap(Other.param2);

            return
                Equals(Param1, OtherParam1) &&
                Equals(Param2, OtherParam2);
        }
    }

    /// <summary>
    ///     Implements a key that is composed of three instances. the key is equal if all instances are equal.
    /// </summary>
    [PublicAPI]
    public class WeakObjectCacheKey<TParam1Type, TParam2Type, TParam3Type> where TParam1Type : class where TParam2Type : class where TParam3Type : class
    {
        /// <summary />
        public WeakObjectCacheKey(TParam1Type param1, TParam2Type param2, TParam3Type param3)
        {
            this.param1 = new WeakReference<TParam1Type>(param1);
            this.param2 = new WeakReference<TParam2Type>(param2);
            this.param3 = new WeakReference<TParam3Type>(param3);
        }

        /// <summary />
        private readonly WeakReference<TParam1Type> param1;

        /// <summary />
        private readonly WeakReference<TParam2Type> param2;

        /// <summary />
        private readonly WeakReference<TParam3Type> param3;

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            object Param1 = WeakObjectCacheKeyHelper.Unwrap(this.param1);
            object Param2 = WeakObjectCacheKeyHelper.Unwrap(this.param2);
            object Param3 = WeakObjectCacheKeyHelper.Unwrap(this.param3);

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
            Debug.Assert(other is WeakObjectCacheKey<TParam1Type, TParam2Type, TParam3Type>);

            var Other = (WeakObjectCacheKey<TParam1Type, TParam2Type, TParam3Type>) other;

            object Param1 = WeakObjectCacheKeyHelper.Unwrap(this.param1);
            object OtherParam1 = WeakObjectCacheKeyHelper.Unwrap(Other.param1);
            object Param2 = WeakObjectCacheKeyHelper.Unwrap(this.param2);
            object OtherParam2 = WeakObjectCacheKeyHelper.Unwrap(Other.param2);
            object Param3 = WeakObjectCacheKeyHelper.Unwrap(this.param3);
            object OtherParam3 = WeakObjectCacheKeyHelper.Unwrap(Other.param3);

            return
                Equals(Param1, OtherParam1) &&
                Equals(Param2, OtherParam2) &&
                Equals(Param3, OtherParam3);
        }
    }

    /// <summary>
    ///     Implements a key that is composed of four instances. the key is equal if all instances are equal.
    /// </summary>
    [PublicAPI]
    public class WeakObjectCacheKey<TParam1Type, TParam2Type, TParam3Type, TParam4Type> where TParam1Type : class where TParam2Type : class where TParam3Type : class where TParam4Type : class
    {
        /// <summary />
        public WeakObjectCacheKey(TParam1Type param1, TParam2Type param2, TParam3Type param3, TParam4Type param4)
        {
            this.param1 = new WeakReference<TParam1Type>(param1);
            this.param2 = new WeakReference<TParam2Type>(param2);
            this.param3 = new WeakReference<TParam3Type>(param3);
            this.param4 = new WeakReference<TParam4Type>(param4);
        }

        /// <summary />
        private readonly WeakReference<TParam1Type> param1;

        /// <summary />
        private readonly WeakReference<TParam2Type> param2;

        /// <summary />
        private readonly WeakReference<TParam3Type> param3;

        /// <summary />
        private readonly WeakReference<TParam4Type> param4;

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            object Param1 = WeakObjectCacheKeyHelper.Unwrap(this.param1);
            object Param2 = WeakObjectCacheKeyHelper.Unwrap(this.param2);
            object Param3 = WeakObjectCacheKeyHelper.Unwrap(this.param3);
            object Param4 = WeakObjectCacheKeyHelper.Unwrap(this.param4);
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
            Debug.Assert(other is WeakObjectCacheKey<TParam1Type, TParam2Type, TParam3Type, TParam4Type>);

            var Other = (WeakObjectCacheKey<TParam1Type, TParam2Type, TParam3Type, TParam4Type>) other;

            object Param1 = WeakObjectCacheKeyHelper.Unwrap(this.param1);
            object OtherParam1 = WeakObjectCacheKeyHelper.Unwrap(Other.param1);
            object Param2 = WeakObjectCacheKeyHelper.Unwrap(this.param2);
            object OtherParam2 = WeakObjectCacheKeyHelper.Unwrap(Other.param2);
            object Param3 = WeakObjectCacheKeyHelper.Unwrap(this.param3);
            object OtherParam3 = WeakObjectCacheKeyHelper.Unwrap(Other.param3);
            object Param4 = WeakObjectCacheKeyHelper.Unwrap(this.param4);
            object OtherParam4 = WeakObjectCacheKeyHelper.Unwrap(Other.param4);

            return
                Equals(Param1, OtherParam1) &&
                Equals(Param2, OtherParam2) &&
                Equals(Param3, OtherParam3) &&
                Equals(Param4, OtherParam4);
        }
    }
}