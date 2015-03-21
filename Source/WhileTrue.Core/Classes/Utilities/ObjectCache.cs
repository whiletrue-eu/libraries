using WhileTrue.Classes.Framework;
#pragma warning disable 1574
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WhileTrue.Classes.Utilities
{
    ///<summary/>
    public abstract class ObjectCacheBase<ObjectType> where ObjectType:class
    {
        private readonly Dictionary<object, WeakReference<ObjectType>> objects = new Dictionary<object, WeakReference<ObjectType>>();
 
        protected ObjectType Add(object key, ObjectType newObject)
        {
            lock (this.objects)
            {
                this.objects.Add(key, new WeakReference<ObjectType>(newObject));
            }
            return newObject;
        }

        public void ForgetObject(object key)
        {
            lock (this.objects)
            {
                if (this.objects.ContainsKey(key))
                {
                    this.objects.Remove(key);
                }
            }
        }

        protected ObjectType Lookup(object key)
        {
            //Clean up the cache...
            lock (this.objects)
            {
                ObjectType Value;
                object[] CollectedKeys = (from Entry in this.objects where Entry.Value.TryGetTarget(out Value) == false select Entry.Key).ToArray();
                CollectedKeys.ForEach(collectedKey => this.objects.Remove(collectedKey));

                if (this.objects.ContainsKey(key))
                {
                    WeakReference<ObjectType> Reference = this.objects[key];
                    ObjectType Target;
                    bool CouldGetTarget = Reference.TryGetTarget(out Target);
                    return CouldGetTarget ? Target : null;
                }
                else
                {
                    return null;
                }
            }
        }
    }

    ///<summary>
    /// Provides a cache for object instances.
    ///</summary>
    /// <remarks>
    /// <para>
    /// For parameter, the same object instance is returned.
    /// The equality of the parameter is evaluated using the <see cref="object.Equals"/> method.
    /// </para>
    /// <para>
    /// The object instances are saved using <see cref="WeakReference"/>s, the cache is automatically cleaned
    /// of collected objects. Such objects will be re-created when they are needed again.
    /// </para>
    /// </remarks>
    public sealed class ObjectCache<KeyType,ObjectType>:ObjectCacheBase<ObjectType> where ObjectType:class
    {
        private readonly Func<KeyType, ObjectType> createFunc;

        /// <summary>
        /// Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        /// be created.
        /// </summary>
        public ObjectCache(Func<KeyType, ObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        /// Returns the object instance that was created for the given parameter. If such an object
        /// does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public ObjectType GetObject(KeyType key)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key) ?? this.Add(key, this.createFunc(key));
        }
    }

    ///<summary>
    /// Provides a cache for object instances.
    ///</summary>
    /// <remarks>
    /// <para>
    /// For every combination of parameters, the same object instance is returned.
    /// The equality of parameters is evaluated using the <see cref="object.Equals"/> method.
    /// </para>
    /// <para>
    /// The object instances are saved using <see cref="WeakReference"/>s, the cache is automatically cleaned
    /// of collected objects. Such objects will be re-created when they are needed again.
    /// </para>
    /// </remarks>
    public sealed class ObjectCache<KeyType, Param1Type, ObjectType> : ObjectCacheBase<ObjectType> where ObjectType : class
    {
        private readonly Func<KeyType, Param1Type, ObjectType> createFunc;
                
        /// <summary>
        /// Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        /// be created.
        /// </summary>
        public ObjectCache(Func<KeyType, Param1Type, ObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public ObjectType GetObject(KeyType key, Param1Type param1)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key) ?? this.Add(key, this.createFunc(key, param1));
        }  
        
        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, <c>null</c> is returned
        /// </summary>
        public ObjectType GetObject(KeyType key)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key);
        }
    }

    ///<summary>
    /// Provides a cache for object instances.
    ///</summary>
    /// <remarks>
    /// <para>
    /// For every combination of parameters, the same object instance is returned.
    /// The equality of parameters is evaluated using the <see cref="object.Equals"/> method.
    /// </para>
    /// <para>
    /// The object instances are saved using <see cref="WeakReference"/>s, the cache is automatically cleaned
    /// of collected objects. Such objects will be re-created when they are needed again.
    /// </para>
    /// </remarks>
    public class ObjectCache<KeyType, Param1Type, Param2Type, ObjectType> : ObjectCacheBase<ObjectType> where ObjectType : class
    {
        private readonly Func<KeyType, Param1Type, Param2Type, ObjectType> createFunc;
                
        /// <summary>
        /// Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        /// be created.
        /// </summary>
        public ObjectCache(Func<KeyType, Param1Type, Param2Type, ObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }
        
        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public ObjectType GetObject(KeyType key, Param1Type param1, Param2Type param2)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key) ?? this.Add(key, this.createFunc(key, param1, param2));
        }

        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, <c>null</c> is returned
        /// </summary>
        public ObjectType GetObject(KeyType key)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key);
        }
    }

    ///<summary>
    /// Provides a cache for object instances.
    ///</summary>
    /// <remarks>
    /// <para>
    /// For every combination of parameters, the same object instance is returned.
    /// The equality of parameters is evaluated using the <see cref="object.Equals"/> method.
    /// </para>
    /// <para>
    /// The object instances are saved using <see cref="WeakReference"/>s, the cache is automatically cleaned
    /// of collected objects. Such objects will be re-created when they are needed again.
    /// </para>
    /// </remarks>
    public class ObjectCache<KeyType, Param1Type, Param2Type, Param3Type, ObjectType> : ObjectCacheBase<ObjectType> where ObjectType : class
    {
        private readonly Func<KeyType, Param1Type, Param2Type, Param3Type, ObjectType> createFunc;
        
        /// <summary>
        /// Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        /// be created.
        /// </summary>
        public ObjectCache(Func<KeyType, Param1Type, Param2Type, Param3Type, ObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public ObjectType GetObject(KeyType key, Param1Type param1, Param2Type param2, Param3Type param3)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key) ?? this.Add(key, this.createFunc(key, param1, param2, param3));
        }

        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, <c>null</c> is returned
        /// </summary>
        public ObjectType GetObject(KeyType key)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key);
        }
    }

    public class ObjectCacheKey<Param1Type>
    {
        private readonly Param1Type param1;

        public ObjectCacheKey(Param1Type param1)
        {
            this.param1 = param1;
        }

        public override int GetHashCode()
        {
            return
                (Equals(this.param1, default(Param1Type)) ? 0 : this.param1.GetHashCode());
        }
        public override bool Equals(object other)
        {
            Debug.Assert(other is ObjectCacheKey<Param1Type>);

            ObjectCacheKey<Param1Type> Other = (ObjectCacheKey<Param1Type>)other;

            return
                Equals(this.param1, Other.param1);
        }
    }

    public class ObjectCacheKey<Param1Type, Param2Type>
    {
        private readonly Param1Type param1;
        private readonly Param2Type param2;

        public ObjectCacheKey(Param1Type param1, Param2Type param2)
        {
            this.param1 = param1;
            this.param2 = param2;
        }

        public override int GetHashCode()
        {
            return
                (Equals(this.param1, default(Param1Type)) ? 0 : this.param1.GetHashCode()) ^
                (Equals(this.param2, default(Param2Type)) ? 0 : this.param2.GetHashCode());
        }
        public override bool Equals(object other)
        {
            Debug.Assert(other is ObjectCacheKey<Param1Type, Param2Type>);

            ObjectCacheKey<Param1Type, Param2Type> Other = (ObjectCacheKey<Param1Type, Param2Type>)other;

            return
                Equals(this.param1, Other.param1) &&
                Equals(this.param2, Other.param2);
        }

        public Param1Type Param1
        {
            get { return this.param1; }
        }

        public Param2Type Param2
        {
            get { return this.param2; }
        }
    }

    public class ObjectCacheKey<Param1Type, Param2Type, Param3Type>
    {
        private readonly Param1Type param1;
        private readonly Param2Type param2;
        private readonly Param3Type param3;

        public ObjectCacheKey(Param1Type param1, Param2Type param2, Param3Type param3)
        {
            this.param1 = param1;
            this.param2 = param2;
            this.param3 = param3;
        }

        public override int GetHashCode()
        {
            return
                (Equals(this.param1, default(Param1Type)) ? 0 : this.param1.GetHashCode()) ^
                (Equals(this.param2, default(Param2Type)) ? 0 : this.param2.GetHashCode()) ^
                (Equals(this.param3, default(Param3Type)) ? 0 : this.param3.GetHashCode());
        }
        public override bool Equals(object other)
        {
            Debug.Assert(other is ObjectCacheKey<Param1Type, Param2Type, Param3Type>);

            ObjectCacheKey<Param1Type, Param2Type, Param3Type> Other = (ObjectCacheKey<Param1Type, Param2Type, Param3Type>)other;

            return
                Equals(this.param1, Other.param1) &&
                Equals(this.param2, Other.param2) &&
                Equals(this.param3, Other.param3);
        }

        public Param1Type Param1
        {
            get { return this.param1; }
        }

        public Param2Type Param2
        {
            get { return this.param2; }
        }

        public Param3Type Param3
        {
            get { return this.param3; }
        }
    }

    public class ObjectCacheKey<Param1Type, Param2Type, Param3Type, Param4Type>
    {
        private readonly Param1Type param1;
        private readonly Param2Type param2;
        private readonly Param3Type param3;
        private readonly Param4Type param4;

        public ObjectCacheKey(Param1Type param1, Param2Type param2, Param3Type param3, Param4Type param4)
        {
            this.param1 = param1;
            this.param2 = param2;
            this.param3 = param3;
            this.param4 = param4;
        }

        public override int GetHashCode()
        {
            return
                (Equals(this.param1, default(Param1Type)) ? 0 : this.param1.GetHashCode()) ^
                (Equals(this.param2, default(Param2Type)) ? 0 : this.param2.GetHashCode()) ^
                (Equals(this.param3, default(Param3Type)) ? 0 : this.param3.GetHashCode()) ^
                (Equals(this.param4, default(Param4Type)) ? 0 : this.param4.GetHashCode());
        }
        public override bool Equals(object other)
        {
            Debug.Assert(other is ObjectCacheKey<Param1Type, Param2Type, Param3Type, Param4Type>);

            ObjectCacheKey<Param1Type, Param2Type, Param3Type, Param4Type> Other = (ObjectCacheKey<Param1Type, Param2Type, Param3Type, Param4Type>)other;

            return
                Equals(this.param1, Other.param1) &&
                Equals(this.param2, Other.param2) &&
                Equals(this.param3, Other.param3) &&
                Equals(this.param4, Other.param4);
        }

        public Param1Type Param1
        {
            get { return this.param1; }
        }

        public Param2Type Param2
        {
            get { return this.param2; }
        }

        public Param3Type Param3
        {
            get { return this.param3; }
        }

        public Param4Type Param4
        {
            get { return this.param4; }
        }
    }
}