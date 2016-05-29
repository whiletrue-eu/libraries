using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.DragNDrop
{
    /// <summary>
    /// Wrapper class that is used for drag and drop in case the type to drag
    /// is not serializable
    /// </summary>
    [Serializable]
    [PublicAPI]
    public class DragDropObjectWrapper : ISerializable
    {
        private static readonly Dictionary<Guid, WeakReference<IDataObject>> objectCache = new Dictionary<Guid, WeakReference<IDataObject>>();

        ///<summary>
        /// Constructor for deserialisation
        ///</summary>
        public DragDropObjectWrapper(SerializationInfo info,StreamingContext context)
        {
            Guid ObjectId = new Guid(info.GetString("ID"));
            this.DragData = DragDropObjectWrapper.GetObject(ObjectId);
        }

        ///<summary/>
        public DragDropObjectWrapper(IDataObject dragData)
        {
            this.DragData = dragData;
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data. </param><param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization. </param><exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //only support "serialization" in same application through shared object cache
            Guid ObjectId = Guid.NewGuid();
            DragDropObjectWrapper.AddObject(ObjectId, this.DragData);
            info.AddValue("ID",ObjectId.ToString());
        }

        private static void AddObject(Guid objectId, IDataObject dataObject)
        {
            DragDropObjectWrapper.RemoveCollectedObjects();
            DragDropObjectWrapper.objectCache.Add(objectId, new WeakReference<IDataObject>(dataObject));
        }

        private static IDataObject GetObject(Guid objectId)
        {
            DragDropObjectWrapper.RemoveCollectedObjects();
            if (DragDropObjectWrapper.objectCache.ContainsKey(objectId))
            {          
                IDataObject Target;
                bool CouldGetTarget = DragDropObjectWrapper.objectCache[objectId].TryGetTarget(out Target);

                return CouldGetTarget ? Target : null;
            }
            else
            {
                return null;
            }
        }

        private static void RemoveCollectedObjects()
        {
            IDataObject Target;
            (from CacheEntry in DragDropObjectWrapper.objectCache select new{CacheEntry.Key,IsAlive=CacheEntry.Value.TryGetTarget(out Target)} into Reference where Reference.IsAlive==false select Reference.Key).ToArray().ForEach(value=>DragDropObjectWrapper.objectCache.Remove(value));
        }

        /// <summary>
        /// Retruns the real data to drag and drop
        /// </summary>
        public IDataObject DragData { get; }
    }
}