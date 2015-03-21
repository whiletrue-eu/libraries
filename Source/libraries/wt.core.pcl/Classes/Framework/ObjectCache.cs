using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Framework
{
    ///<summary/>
    [PublicAPI]
    public abstract class ObjectCacheBase<TObjectType> where TObjectType:class
    {
        private readonly Dictionary<object, WeakReference<TObjectType>> objects = new Dictionary<object, WeakReference<TObjectType>>();

        /// <summary>
        /// Removes an object from the cache
        /// </summary>
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

        /// <summary>
        /// Looks up the object with the given key or creates one using the given delegate and adds it to the cache. The cache is cleaned up from objects that were collected.
        /// </summary>
        protected TObjectType Lookup(object key, Func<TObjectType> createFunc )
        {
            //Clean up the cache...
            lock (this.objects)
            {
                TObjectType Value;
                Task.Run(delegate
                {
                    lock (this.objects)
                    {
                        object[] CollectedKeys = (from Entry in this.objects where Entry.Value.TryGetTarget(out Value) == false select Entry.Key).ToArray();
                        CollectedKeys.ForEach(collectedKey => this.objects.Remove(collectedKey));
                    }
                });
                if (this.objects.ContainsKey(key))
                {
                    WeakReference<TObjectType> Reference = this.objects[key];
                    TObjectType Target;
                    if (Reference.TryGetTarget(out Target))
                    {
                        return Target;
                    }
                    else
                    {
                        //Remove dead instance reference
                        this.objects.Remove(key);
                    }
                }
                //Key not found or found but not alive. Create new
                TObjectType NewObject = createFunc();
                this.objects.Add(key, new WeakReference<TObjectType>(NewObject));
                return NewObject;
            }
        }
    }

    ///<summary>
    /// Provides a cache for object instances.
    ///</summary>
    /// <remarks>
    /// <para>
    /// For parameter, the same object instance is returned.
    /// The equality of the parameter is evaluated using the <see cref="object.Equals(object)"/> method.
    /// </para>
    /// <para>
    /// The object instances are saved using <see cref="WeakReference"/>s, the cache is automatically cleaned
    /// of collected objects. Such objects will be re-created when they are needed again.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public sealed class ObjectCache<TKeyType,TObjectType>:ObjectCacheBase<TObjectType> where TObjectType:class
    {
        private readonly Func<TKeyType, TObjectType> createFunc;

        /// <summary>
        /// Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        /// be created.
        /// </summary>
        public ObjectCache(Func<TKeyType, TObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        /// Returns the object instance that was created for the given parameter. If such an object
        /// does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public TObjectType GetObject(TKeyType key)
        {
                return object.ReferenceEquals(key, null) ? null : this.Lookup(key, ()=>this.createFunc(key));
        }
    }

    ///<summary>
    /// Provides a cache for object instances.
    ///</summary>
    /// <remarks>
    /// <para>
    /// For every combination of parameters, the same object instance is returned.
    /// The equality of parameters is evaluated using the <see cref="object.Equals(object)"/> method.
    /// </para>
    /// <para>
    /// The object instances are saved using <see cref="WeakReference"/>s, the cache is automatically cleaned
    /// of collected objects. Such objects will be re-created when they are needed again.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public sealed class ObjectCache<TKeyType, TParam1Type, TObjectType> : ObjectCacheBase<TObjectType> where TObjectType : class
    {
        private readonly Func<TKeyType, TParam1Type, TObjectType> createFunc;
                
        /// <summary>
        /// Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        /// be created.
        /// </summary>
        public ObjectCache(Func<TKeyType, TParam1Type, TObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public TObjectType GetObject(TKeyType key, TParam1Type param1)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key, () => this.createFunc(key, param1));
        }  
        
        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, <c>null</c> is returned
        /// </summary>
        public TObjectType GetObject(TKeyType key)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key, () => { throw new ArgumentException("Object not found in cache"); });
        }
    }

    ///<summary>
    /// Provides a cache for object instances.
    ///</summary>
    /// <remarks>
    /// <para>
    /// For every combination of parameters, the same object instance is returned.
    /// The equality of parameters is evaluated using the <see cref="object.Equals(object)"/> method.
    /// </para>
    /// <para>
    /// The object instances are saved using <see cref="WeakReference"/>s, the cache is automatically cleaned
    /// of collected objects. Such objects will be re-created when they are needed again.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public class ObjectCache<TKeyType, TParam1Type, TParam2Type, TObjectType> : ObjectCacheBase<TObjectType> where TObjectType : class
    {
        private readonly Func<TKeyType, TParam1Type, TParam2Type, TObjectType> createFunc;
                
        /// <summary>
        /// Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        /// be created.
        /// </summary>
        public ObjectCache(Func<TKeyType, TParam1Type, TParam2Type, TObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }
        
        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public TObjectType GetObject(TKeyType key, TParam1Type param1, TParam2Type param2)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key, () => this.createFunc(key, param1, param2));
        }

        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, <c>null</c> is returned
        /// </summary>
        public TObjectType GetObject(TKeyType key)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key, () => { throw new ArgumentException("Object not found in cache"); });
        }
    }

    ///<summary>
    /// Provides a cache for object instances.
    ///</summary>
    /// <remarks>
    /// <para>
    /// For every combination of parameters, the same object instance is returned.
    /// The equality of parameters is evaluated using the <see cref="object.Equals(object)"/> method.
    /// </para>
    /// <para>
    /// The object instances are saved using <see cref="WeakReference"/>s, the cache is automatically cleaned
    /// of collected objects. Such objects will be re-created when they are needed again.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public class ObjectCache<TKeyType, TParam1Type, TParam2Type, TParam3Type, TObjectType> : ObjectCacheBase<TObjectType> where TObjectType : class
    {
        private readonly Func<TKeyType, TParam1Type, TParam2Type, TParam3Type, TObjectType> createFunc;
        
        /// <summary>
        /// Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        /// be created.
        /// </summary>
        public ObjectCache(Func<TKeyType, TParam1Type, TParam2Type, TParam3Type, TObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public TObjectType GetObject(TKeyType key, TParam1Type param1, TParam2Type param2, TParam3Type param3)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key, () => this.createFunc(key, param1, param2, param3));
        }

        /// <summary>
        /// Returns the object instance that was created for the given parameters. If such an object
        /// does not exist yet, <c>null</c> is returned
        /// </summary>
        public TObjectType GetObject(TKeyType key)
        {
            return object.ReferenceEquals(key, null) ? null : this.Lookup(key, () => { throw new ArgumentException("Object not found in cache"); });
        }
    }
}