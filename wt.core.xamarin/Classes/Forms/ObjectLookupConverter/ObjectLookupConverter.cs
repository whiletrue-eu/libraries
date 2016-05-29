using System;
using System.Collections.ObjectModel;
using System.Globalization;
using JetBrains.Annotations;
using Xamarin.Forms;

namespace WhileTrue.Classes.Forms
{
    /// <summary>
    /// returns the object given for the object to convert from the map
    /// </summary>
    /// <remarks>
    /// <br/>
    /// namespace: wt = http://schemas.whiletrue.eu/xaml<br/>
    /// <br/>
    /// <br/>
    /// You can use two kinds of items for lookup. <see cref="ObjectLookupItem"/> will compare its <c>Key</c> against the given value and returns its <c>Result</c>. <see cref="ObjectLookupOtherwiseItem"/> will return its value whenever it is encountered. Because of that, the otherwise item should be decalred as the last item as fallback.<br/>
    /// <br/>
    /// The key of each item will be compared to the given value in the order, the items are declared. <br/>
    /// If different types of the key and value are encountered, the type will automatically converted to be comparable in the following order:<br/>
    /// * The type converter of the key is asked to convert the key to the type of the value <br/>
    /// * The type converter of the key is asked to convert the value to the type of the key <br/>
    /// * The type converter of the value is asked to convert the key to the type of the value <br/>
    /// * The type converter of the value is asked to convert the value to the type of the key <br/>
    /// <br/>
    /// If the types can be converted (or are of the same type from the beginning), their values will be compared by calling the <see cref="object.Equals(object,object)"/> method. 
    /// Otherwise, they will not be compared and the next item will be checked.
    /// <br/>
    /// Usage:
    /// <code>
    /// &lt;ResourceDictionary>
    ///   &lt;wt:ObjectLookupConverter x:Key="objectLookupConverter">
    ///     &lt;wt:ObjectLookupItem Key="True">Value is TRUE&lt;/wt:ObjectLookupItem>
    ///     &lt;wt:ObjectLookupOtherwiseItem>Value is NOT TRUE (maybe FALSE!)&lt;/wt:ObjectLookupOtherwiseItem>
    ///   &lt;/wt:ObjectLookupConverter>
    /// &lt;/ResourceDictionary>
    /// 
    /// Visibility="{Binding Path=...,Converter={StaticResource objectLookupConverter}}"
    /// </code>
    /// <br/>
    /// if no ObjectLookupOtherwiseItem exists, the original value is returned.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if a key was given that does not match any lookup item</exception>
    [ContentProperty(nameof(ObjectLookupConverter.LookupItems))]
    [PublicAPI]
    public class ObjectLookupConverter : IValueConverter
    {
        /// <summary>
        /// gets/Sets a name used for debugging output
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the collection of lookup items (see <see cref="ObjectLookupItem"/>)
        /// </summary>
        public ObservableCollection<ObjectLookupItemBase> LookupItems { get; } = new ObservableCollection<ObjectLookupItemBase>();

        #region IValueConverter Members

        /// <summary/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object Result = this.Convert(value);
            return Result;
        }

        /// <summary/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object Result = this.ConvertBack(value);
            return Result;
        }

        #endregion

        private object Convert(object value)
        {
            foreach (ObjectLookupItemBase LookupItem in this.LookupItems)
            {
                if (LookupItem is ObjectLookupOtherwiseItem)
                {
                    return LookupItem.Value;
                }
                else if (LookupItem is ObjectLookupItem)
                {
                    ObjectLookupItem Item = (ObjectLookupItem) LookupItem;

                    Type KeyType = Item.Key?.GetType();
                    Type ValueType = value?.GetType();

                    if (KeyType == ValueType)
                    {
                        if (object.Equals(Item.Key, value))
                        {
                            return LookupItem.Value;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("TODO"); //TODO: implement dynamic casting
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unknown lookup item type");
                }
            }

            //Not found - Fallback: return unconverted value
            return value;
        }
        private object ConvertBack(object value)
        {
            foreach (ObjectLookupItemBase LookupItem in this.LookupItems)
            {
                if (LookupItem is ObjectLookupOtherwiseItem)
                {
                    return null;
                }
                else if (LookupItem is ObjectLookupItem)
                {
                    ObjectLookupItem Item = (ObjectLookupItem)LookupItem;

                    Type DestinationType = Item.Value?.GetType();
                    Type ValueType = value?.GetType();

                    if (DestinationType == ValueType)
                    {
                        if (object.Equals(Item.Value, value))
                        {
                            return Item.Key;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("TODO"); //TODO: implement dynamic casting
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unknown lookup item type");
                }
            }

            //Not found - Fallback: return unconverted value
            return value;
        }
    }
}