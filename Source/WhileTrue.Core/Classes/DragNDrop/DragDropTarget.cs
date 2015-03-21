using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    ///<summary>
    /// Allows easy implementation of the <see cref="IDragDropTarget"/> interface by providing
    /// delegates for different item types.
    ///</summary>
    public class DragDropTarget : IDragDropTarget
    {
        private readonly DragDropItemHandlerBase[] handlers;

        internal DragDropTarget(DragDropItemHandlerBase[] handlers)
        {
            this.handlers = handlers;
        }

        /// <summary>
        /// Returns a factor for this class that allows to set handling routines for multiple item types.
        /// </summary>
        public static DragDropTargetFactory GetFactory()
        {
            return new DragDropTargetFactory();
        }

        public DragDropEffects GetDropEffects(IDataObject data)
        {
            DragDropItemHandlerBase ItemHandler = GetItemHandler(data);
            if (ItemHandler != null)
            {
                return ItemHandler.GetDropEffects(data);
            }
            else
            {
                return DragDropEffects.None;
            }
        }

        private DragDropItemHandlerBase GetItemHandler(IDataObject data)
        {
            return (from Handler in this.handlers where Handler.CanHandle(data) select Handler).FirstOrDefault();
        }

        public DragDropEffect GetDefaultEffect(IDataObject data)
        {
            DragDropItemHandlerBase ItemHandler = GetItemHandler(data);
            if (ItemHandler != null)
            {
                return ItemHandler.GetDefaultEffect(data);
            }
            else
            {
                return DragDropEffect.None;
            }
        }

        public void DoDrop(IDataObject data, DragDropEffect effect, AdditionalDropInfo additionalDropInfo)
        {
            DragDropItemHandlerBase ItemHandler = GetItemHandler(data);
            if (ItemHandler != null)
            {
                ItemHandler.DoDrop(data, effect, additionalDropInfo);
            }
            else
            {
                throw new InvalidOperationException("drag drop not requested, but doDrop was called");
            }
        }

        internal abstract class DragDropItemHandlerBase
        {
            public abstract bool CanHandle(IDataObject data);
            public abstract DragDropEffects GetDropEffects(IDataObject data);
            public abstract void DoDrop(IDataObject data, DragDropEffect effect, AdditionalDropInfo additionalDropInfo);
            public abstract DragDropEffect GetDefaultEffect(IDataObject data);
        }
        internal class DragDropItemHandler<ItemType> : DragDropItemHandlerBase
        {
            private readonly Func<ItemType, DragDropEffects> allowedEffects;
            private readonly Func<ItemType, DragDropEffect> defaultEffect;
            private readonly Action<ItemType, DragDropEffect, AdditionalDropInfo> doDropAction;
            private readonly TypeConverter typeConverter;

            public DragDropItemHandler(Func<ItemType, DragDropEffects> allowedEffects, Func<ItemType, DragDropEffect> defaultEffect, Action<ItemType, DragDropEffect, AdditionalDropInfo> doDropAction)
            {
                this.allowedEffects = allowedEffects;
                this.defaultEffect = defaultEffect;
                this.doDropAction = doDropAction;

                this.typeConverter = TypeDescriptor.GetConverter(typeof (ItemType));
            }

            #region Overrides of DragDropItemHandlerBase

            public override DragDropEffects GetDropEffects(IDataObject data)
            {
                ItemType Data = this.GetData(data);
                return this.allowedEffects(Data);
            }

            public override void DoDrop(IDataObject data, DragDropEffect effect, AdditionalDropInfo additionalDropInfo)
            {
                ItemType Data = this.GetData(data);
                this.doDropAction(Data, effect, additionalDropInfo);
            }

            public override DragDropEffect GetDefaultEffect(IDataObject data)
            {
                ItemType Data = this.GetData(data);
                return this.defaultEffect(Data);
            }

            /// <summary>
            /// Search order for data in IDataObject:
            /// * target type of item is present
            /// * target type support conversion from IDataObject
            /// * For each type that is discovered from GetFormats():
            ///   * item type is a base class of type
            ///   * type can be converted to item type
            /// </summary>
            public override bool CanHandle(IDataObject data)
            {
                return this.GetDataResolverCached(data) != null;
            }

            private ItemType GetData(IDataObject data)
            {
                return this.GetDataResolverCached(data).Invoke();
            }


            private IDataObject cachedDataObject;
            private Func<ItemType> cachedDataResolver;

            private Func<ItemType> GetDataResolverCached(IDataObject data)
            {
                if (this.cachedDataObject == data)
                {
                    return this.cachedDataResolver;
                }
                else
                {
                    this.cachedDataObject = data;
                    this.cachedDataResolver = this.GetDataResolver(data);
                }
                return this.cachedDataResolver;
            }

            private Func<ItemType> GetDataResolver(IDataObject data)
            {
                if (data == null)
                {
                    //Data could not be retrieved -> maybe non-serializable data was dragged in from another application
                    return null;
                }
                if (data.GetDataPresent(typeof(DragDropObjectWrapper)))
                {
                    //Dataobject is wrapped because of serialisation. unwrap before further processing
                    return this.GetDataResolver(((DragDropObjectWrapper) data.GetData(typeof (DragDropObjectWrapper))).DragData);
                }
                else if (data.GetDataPresent(typeof (ItemType)))
                {
                    // dataobject contains the target type -> return that
                    return () => (ItemType) data.GetData(typeof (ItemType));
                }
                else if (this.typeConverter.CanConvertFrom(typeof (IDataObject)))
                {
                    // target type supports conversion from IDataObject -> return the converted value
                    return () => (ItemType) this.typeConverter.ConvertFrom(data);
                }
                else
                {
                    // try to iterate through all the formats indicated in IDataObject,if we
                    // can find one that can be casted or converted into the target format
                    foreach (string Format in data.GetFormats())
                    {
                        foreach (Assembly Assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) //reverse, as custom types are more probable
                        {
                            Type Type = Assembly.GetType(Format, false);
                            if (Type != null)
                            {
                                if (typeof (ItemType).IsAssignableFrom(Type))
                                {
                                    return () => (ItemType) data.GetData(Type);
                                }
                                else if (this.typeConverter.CanConvertFrom(Type))
                                {
                                    return () => (ItemType) this.typeConverter.ConvertFrom(data);
                                }
                            }
                        }
                    }
                    return null;
                }
            }



            #endregion
        }
    }
}