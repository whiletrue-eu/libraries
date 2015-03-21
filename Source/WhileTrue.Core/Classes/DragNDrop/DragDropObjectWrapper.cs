using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.DragNDrop
{
    /// <summary>
    /// Wrapper class that is used for drag and drop in case the type to drag
    /// is not serializable
    /// </summary>
    [Serializable]
    public class DragDropObjectWrapper : ISerializable
    {
        private readonly IDataObject dragData;
        private static readonly Dictionary<Guid, WeakReference<IDataObject>> objectCache = new Dictionary<Guid, WeakReference<IDataObject>>();

        ///<summary>
        /// Constructor for deserialisation
        ///</summary>
        public DragDropObjectWrapper(SerializationInfo info,StreamingContext context)
        {
            Guid ObjectID = new Guid(info.GetString("ID"));
            this.dragData = GetObject(ObjectID);
        }

        ///<summary/>
        public DragDropObjectWrapper(IDataObject dragData)
        {
            this.dragData = dragData;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //only support "serialization" in same application through shared object cache
            Guid ObjectID = Guid.NewGuid();
            AddObject(ObjectID, this.dragData);
            info.AddValue("ID",ObjectID.ToString());
        }

        private static void AddObject(Guid objectID, IDataObject dataObject)
        {
            RemoveCollectedObjects();
            objectCache.Add(objectID, new WeakReference<IDataObject>(dataObject));
        }

        private static IDataObject GetObject(Guid objectID)
        {
            RemoveCollectedObjects();
            if (objectCache.ContainsKey(objectID))
            {          
                IDataObject Target;
                bool CouldGetTarget = objectCache[objectID].TryGetTarget(out Target);

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
            (from CacheEntry in objectCache select new{CacheEntry.Key,IsAlive=CacheEntry.Value.TryGetTarget(out Target)} into Reference where Reference.IsAlive==false select Reference.Key).ToArray().ForEach(value=>objectCache.Remove(value));
        }

        /// <summary>
        /// Retruns the real data to drag and drop
        /// </summary>
        public IDataObject DragData
        {
            get { return this.dragData; }
        }
    }
}