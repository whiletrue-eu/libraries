using System;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.DragNDrop.Facades.ImageLibraryModel;

namespace WhileTrue.DragNDrop.Modules.ImageLibraryViewer
{
    internal class ItemAdapterBase : ObservableObject
    {
        private static readonly ObjectCache<object,ItemAdapterBase> objectCache = new ObjectCache<object, ItemAdapterBase>(CreateItem);

        private static ItemAdapterBase CreateItem(object value)
        {
            if( value is IGroup)
            {
                return new GroupAdapter((IGroup) value);
            }
            else if( value is IImage)
            {
                return new ImageAdapter((IImage) value);
            }
            else
            {
                throw new ArgumentException("unkown type");
            }
        }

        public static ItemAdapterBase GetInstance(object value)
        {
            return objectCache.GetObject(value);
        }
    }
}