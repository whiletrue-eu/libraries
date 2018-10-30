using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Framework
{
    /// <summary />
    [PublicAPI]
    public abstract class ObjectCacheBase<TKeyType, TObjectType> where TObjectType : class
    {
        private readonly Dictionary<TKeyType, WeakReference<TObjectType>> objects =
            new Dictionary<TKeyType, WeakReference<TObjectType>>();

        /// <summary>
        ///     Removes an object from the cache
        /// </summary>
        public void ForgetObject(TKeyType key)
        {
            lock (objects)
            {
                if (objects.ContainsKey(key)) objects.Remove(key);
            }
        }

        /// <summary>
        ///     Looks up the object with the given key or creates one using the given delegate and adds it to the cache. The cache
        ///     is cleaned up from objects that were collected.
        /// </summary>
        protected TObjectType Lookup(TKeyType key, Func<TObjectType> createFunc)
        {
            //Clean up the cache...
            lock (objects)
            {
                TObjectType Value;
                Task.Run(delegate
                {
                    lock (objects)
                    {
                        var CollectedKeys =
                            (from Entry in objects where Entry.Value.TryGetTarget(out Value) == false select Entry.Key)
                            .ToArray();
                        CollectedKeys.ForEach(collectedKey => objects.Remove(collectedKey));
                    }
                });
                if (objects.ContainsKey(key))
                {
                    var Reference = objects[key];
                    TObjectType Target;
                    if (Reference.TryGetTarget(out Target))
                        return Target;
                    objects.Remove(key);
                }

                //Key not found or found but not alive. Create new
                var NewObject = createFunc();
                objects.Add(key, new WeakReference<TObjectType>(NewObject));
                return NewObject;
            }
        }


        /// <summary>
        ///     Returns an enumeration of all keys and objects in the cache that are still 'alive'
        /// </summary>
        public IEnumerator<KeyValuePair<TKeyType, TObjectType>> GetEnumerator()
        {
            KeyValuePair<TKeyType, WeakReference<TObjectType>>[] Objects;
            lock (objects)
            {
                Objects = objects.ToArray();
            }

            foreach (var Item in Objects)
            {
                TObjectType Value;
                if (Item.Value.TryGetTarget(out Value))
                {
                    yield return new KeyValuePair<TKeyType, TObjectType>(Item.Key, Value);
                }
            }
        }
    }

    /// <summary>
    ///     Provides a cache for object instances.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For parameter, the same object instance is returned.
    ///         The equality of the parameter is evaluated using the <see cref="object.Equals(object)" /> method.
    ///     </para>
    ///     <para>
    ///         The object instances are saved using <see cref="WeakReference" />s, the cache is automatically cleaned
    ///         of collected objects. Such objects will be re-created when they are needed again.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    public sealed class ObjectCache<TKeyType, TObjectType> : ObjectCacheBase<TKeyType, TObjectType>
        where TObjectType : class
    {
        private readonly Func<TKeyType, TObjectType> createFunc;

        /// <summary>
        ///     Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        ///     be created.
        /// </summary>
        public ObjectCache(Func<TKeyType, TObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        ///     Returns the object instance that was created for the given parameter. If such an object
        ///     does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public TObjectType GetObject(TKeyType key)
        {
            return ReferenceEquals(key, null) ? null : Lookup(key, () => createFunc(key));
        }
    }

    /// <summary>
    ///     Provides a cache for object instances.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For every combination of parameters, the same object instance is returned.
    ///         The equality of parameters is evaluated using the <see cref="object.Equals(object)" /> method.
    ///     </para>
    ///     <para>
    ///         The object instances are saved using <see cref="WeakReference" />s, the cache is automatically cleaned
    ///         of collected objects. Such objects will be re-created when they are needed again.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    public sealed class ObjectCache<TKeyType, TParam1Type, TObjectType> : ObjectCacheBase<TKeyType, TObjectType>
        where TObjectType : class
    {
        private readonly Func<TKeyType, TParam1Type, TObjectType> createFunc;

        /// <summary>
        ///     Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        ///     be created.
        /// </summary>
        public ObjectCache(Func<TKeyType, TParam1Type, TObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        ///     Returns the object instance that was created for the given parameters. If such an object
        ///     does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public TObjectType GetObject(TKeyType key, TParam1Type param1)
        {
            return ReferenceEquals(key, null) ? null : Lookup(key, () => createFunc(key, param1));
        }

        /// <summary>
        ///     Returns the object instance that was created for the given parameters. If such an object
        ///     does not exist yet, <c>null</c> is returned
        /// </summary>
        public TObjectType GetObject(TKeyType key)
        {
            return ReferenceEquals(key, null)
                ? null
                : Lookup(key, () => { throw new ArgumentException("Object not found in cache"); });
        }
    }

    /// <summary>
    ///     Provides a cache for object instances.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For every combination of parameters, the same object instance is returned.
    ///         The equality of parameters is evaluated using the <see cref="object.Equals(object)" /> method.
    ///     </para>
    ///     <para>
    ///         The object instances are saved using <see cref="WeakReference" />s, the cache is automatically cleaned
    ///         of collected objects. Such objects will be re-created when they are needed again.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    public class ObjectCache<TKeyType, TParam1Type, TParam2Type, TObjectType> : ObjectCacheBase<TKeyType, TObjectType>
        where TObjectType : class
    {
        private readonly Func<TKeyType, TParam1Type, TParam2Type, TObjectType> createFunc;

        /// <summary>
        ///     Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        ///     be created.
        /// </summary>
        public ObjectCache(Func<TKeyType, TParam1Type, TParam2Type, TObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        ///     Returns the object instance that was created for the given parameters. If such an object
        ///     does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public TObjectType GetObject(TKeyType key, TParam1Type param1, TParam2Type param2)
        {
            return ReferenceEquals(key, null) ? null : Lookup(key, () => createFunc(key, param1, param2));
        }

        /// <summary>
        ///     Returns the object instance that was created for the given parameters. If such an object
        ///     does not exist yet, <c>null</c> is returned
        /// </summary>
        public TObjectType GetObject(TKeyType key)
        {
            return ReferenceEquals(key, null)
                ? null
                : Lookup(key, () => { throw new ArgumentException("Object not found in cache"); });
        }
    }

    /// <summary>
    ///     Provides a cache for object instances.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For every combination of parameters, the same object instance is returned.
    ///         The equality of parameters is evaluated using the <see cref="object.Equals(object)" /> method.
    ///     </para>
    ///     <para>
    ///         The object instances are saved using <see cref="WeakReference" />s, the cache is automatically cleaned
    ///         of collected objects. Such objects will be re-created when they are needed again.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    public class
        ObjectCache<TKeyType, TParam1Type, TParam2Type, TParam3Type, TObjectType> : ObjectCacheBase<TKeyType,
            TObjectType> where TObjectType : class
    {
        private readonly Func<TKeyType, TParam1Type, TParam2Type, TParam3Type, TObjectType> createFunc;

        /// <summary>
        ///     Creates the cache. The <c>createFunc</c> delegate is called if a new object instance must
        ///     be created.
        /// </summary>
        public ObjectCache(Func<TKeyType, TParam1Type, TParam2Type, TParam3Type, TObjectType> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        ///     Returns the object instance that was created for the given parameters. If such an object
        ///     does not exist yet, it is created on-the-fly and stored int he cache for consecutive calls.
        /// </summary>
        public TObjectType GetObject(TKeyType key, TParam1Type param1, TParam2Type param2, TParam3Type param3)
        {
            return ReferenceEquals(key, null) ? null : Lookup(key, () => createFunc(key, param1, param2, param3));
        }

        /// <summary>
        ///     Returns the object instance that was created for the given parameters. If such an object
        ///     does not exist yet, <c>null</c> is returned
        /// </summary>
        public TObjectType GetObject(TKeyType key)
        {
            return ReferenceEquals(key, null)
                ? null
                : Lookup(key, () => { throw new ArgumentException("Object not found in cache"); });
        }
    }
}