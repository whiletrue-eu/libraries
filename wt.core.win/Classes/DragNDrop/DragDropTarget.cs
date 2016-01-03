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

        /// <summary>
        /// Get the drop effects that are possible for the data object given. 
        /// By comparing the drop effects to the ones accepted by the source, the effective drop effects
        /// are calculated. The drop effect applied is calculated depending on modifier keys pressed by the user
        /// and the <see cref="IDragDropTarget.GetDefaultEffect"/> value.
        /// </summary>
        public DragDropEffects GetDropEffects(IDataObject data)
        {
            DragDropItemHandlerBase ItemHandler = this.GetItemHandler(data);
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

        /// <summary>
        /// Gets the default effect that shall be used when no modifier key is pressed (but only if the 
        /// source supports the effect)
        /// </summary>
        public DragDropEffect GetDefaultEffect(IDataObject data)
        {
            DragDropItemHandlerBase ItemHandler = this.GetItemHandler(data);
            if (ItemHandler != null)
            {
                return ItemHandler.GetDefaultEffect(data);
            }
            else
            {
                return DragDropEffect.None;
            }
        }

        /// <summary>
        /// Drops the data on the target with the given drop effect. <c>additionalDropInfo</c> may contain
        /// additonal data that can be used during the drop operation, depending on the type of target UI control
        /// </summary>
        public void DoDrop(IDataObject data, DragDropEffect effect, AdditionalDropInfo additionalDropInfo)
        {
            DragDropItemHandlerBase ItemHandler = this.GetItemHandler(data);
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
        internal class DragDropItemHandler<TItemType> : DragDropItemHandlerBase
        {
            private readonly Func<TItemType, DragDropEffects> allowedEffects;
            private readonly Func<TItemType, DragDropEffect> defaultEffect;
            private readonly Action<TItemType, DragDropEffect, AdditionalDropInfo> doDropAction;
            private readonly TypeConverter typeConverter;

            public DragDropItemHandler(Func<TItemType, DragDropEffects> allowedEffects, Func<TItemType, DragDropEffect> defaultEffect, Action<TItemType, DragDropEffect, AdditionalDropInfo> doDropAction)
            {
                this.allowedEffects = allowedEffects;
                this.defaultEffect = defaultEffect;
                this.doDropAction = doDropAction;

                this.typeConverter = TypeDescriptor.GetConverter(typeof (TItemType));
            }

            #region Overrides of DragDropItemHandlerBase

            public override DragDropEffects GetDropEffects(IDataObject data)
            {
                TItemType Data = this.GetData(data);
                return this.allowedEffects(Data);
            }

            public override void DoDrop(IDataObject data, DragDropEffect effect, AdditionalDropInfo additionalDropInfo)
            {
                TItemType Data = this.GetData(data);
                this.doDropAction(Data, effect, additionalDropInfo);
            }

            public override DragDropEffect GetDefaultEffect(IDataObject data)
            {
                TItemType Data = this.GetData(data);
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

            private TItemType GetData(IDataObject data)
            {
                return this.GetDataResolverCached(data).Invoke();
            }


            private IDataObject cachedDataObject;
            private Func<TItemType> cachedDataResolver;

            private Func<TItemType> GetDataResolverCached(IDataObject data)
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

            private Func<TItemType> GetDataResolver(IDataObject data)
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
                else if (data.GetDataPresent(typeof (TItemType)))
                {
                    // dataobject contains the target type -> return that
                    return () => (TItemType) data.GetData(typeof (TItemType));
                }
                else if (this.typeConverter.CanConvertFrom(typeof (IDataObject)))
                {
                    // target type supports conversion from IDataObject -> return the converted value
                    return () => (TItemType) this.typeConverter.ConvertFrom(data);
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
                                if (typeof (TItemType).IsAssignableFrom(Type))
                                {
                                    return () => (TItemType) data.GetData(Type);
                                }
                                else if (this.typeConverter.CanConvertFrom(Type))
                                {
                                    return () => (TItemType) this.typeConverter.ConvertFrom(data);
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