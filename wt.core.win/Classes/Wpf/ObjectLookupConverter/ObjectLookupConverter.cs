using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;
using WhileTrue.Classes.Logging;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    ///     returns the object given for the object to convert from the map
    /// </summary>
    /// <remarks>
    ///     <br />
    ///     namespace: wt = http://schemas.whiletrue.eu/xaml<br />
    ///     <br />
    ///     <br />
    ///     You can use two kinds of items for lookup. <see cref="ObjectLookupItem" /> will compare its <c>Key</c> against the
    ///     given value and returns its <c>Result</c>. <see cref="ObjectLookupOtherwiseItem" /> will return its value whenever
    ///     it is encountered. Because of that, the otherwise item should be decalred as the last item as fallback.<br />
    ///     <br />
    ///     The key of each item will be compared to the given value in the order, the items are declared. <br />
    ///     If different types of the key and value are encountered, the type will automatically converted to be comparable in
    ///     the following order:<br />
    ///     * The type converter of the key is asked to convert the key to the type of the value <br />
    ///     * The type converter of the key is asked to convert the value to the type of the key <br />
    ///     * The type converter of the value is asked to convert the key to the type of the value <br />
    ///     * The type converter of the value is asked to convert the value to the type of the key <br />
    ///     <br />
    ///     If the types can be converted (or are of the same type from the beginning), their values will be compared by
    ///     calling the <see cref="object.Equals(object,object)" /> method.
    ///     Otherwise, they will not be compared and the next item will be checked.
    ///     <br />
    ///     Usage:
    ///     <code>
    /// &lt;ResourceDictionary>
    ///   &lt;wt:ObjectLookupConverter x:Key="objectLookupConverter">
    ///     &lt;wt:ObjectLookupItem Key="True">Value is TRUE&lt;/wt:ObjectLookupItem>
    ///     &lt;wt:ObjectLookupOtherwiseItem>Value is NOT TRUE (maybe FALSE!)&lt;/wt:ObjectLookupOtherwiseItem>
    ///   &lt;/wt:ObjectLookupConverter>
    /// &lt;/ResourceDictionary>
    /// 
    /// Visibility="{Binding Path=...,Converter={StaticResource objectLookupConverter}}"
    /// </code>
    ///     <br />
    ///     if no ObjectLookupOtherwiseItem exists, the original value is returned.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if a key was given that does not match any lookup item</exception>
    [ContentProperty("LookupItems")]
    [PublicAPI]
    public class ObjectLookupConverter : IValueConverter
    {
        /// <summary>
        ///     gets/Sets a name used for debugging output
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the collection of lookup items (see <see cref="ObjectLookupItem" />)
        /// </summary>
        public ObservableCollection<ObjectLookupItemBase> LookupItems { get; } =
            new ObservableCollection<ObjectLookupItemBase>();

        private object Convert(object value)
        {
            foreach (var LookupItem in LookupItems)
                if (LookupItem is ObjectLookupOtherwiseItem)
                {
                    return LookupItem.Value;
                }
                else if (LookupItem is ObjectLookupItem)
                {
                    var Item = (ObjectLookupItem) LookupItem;

                    var KeyType = Item.Key?.GetType();
                    var ValueType = value?.GetType();

                    if (KeyType == ValueType)
                    {
                        if (Equals(Item.Key, value)) return LookupItem.Value;
                    }
                    else if (KeyType != null && ValueType != null)
                    {
                        var KeyConverter = TypeDescriptor.GetConverter(Item.Key);
                        var ValueConverter = TypeDescriptor.GetConverter(value);
                        if (KeyConverter.CanConvertTo(ValueType))
                        {
                            var ConvertedKey = KeyConverter.ConvertTo(Item.Key, ValueType);
                            if (Equals(ConvertedKey, value)) return LookupItem.Value;
                        }
                        else if (KeyConverter.CanConvertFrom(ValueType))
                        {
                            var ConvertedValue = KeyConverter.ConvertFrom(value);
                            if (Equals(Item.Key, ConvertedValue)) return LookupItem.Value;
                        }
                        else if (ValueConverter.CanConvertTo(KeyType))
                        {
                            var ConvertedValue = KeyConverter.ConvertTo(value, KeyType);
                            if (Equals(Item.Key, ConvertedValue)) return LookupItem.Value;
                        }
                        else if (ValueConverter.CanConvertFrom(ValueType))
                        {
                            var ConvertedKey = KeyConverter.ConvertFrom(Item.Key);
                            if (Equals(ConvertedKey, value)) return LookupItem.Value;
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unknown lookup item type");
                }

            //Not found - Fallback: return unconverted value
            return value;
        }

        private object ConvertBack(object value)
        {
            foreach (var LookupItem in LookupItems)
                if (LookupItem is ObjectLookupOtherwiseItem)
                {
                    DebugLogger.WriteLine(this, LoggingLevel.Normal,
                        () => string.Format(
                            "WARNING: ObjectLookupConverter could convert back '{1}' as it is the 'otherwise' item: '{0}'",
                            value, Name));
                    return null;
                }
                else if (LookupItem is ObjectLookupItem)
                {
                    var Item = (ObjectLookupItem) LookupItem;

                    var DestinationType = Item.Value?.GetType();
                    var ValueType = value?.GetType();

                    if (DestinationType == ValueType)
                    {
                        if (Equals(Item.Value, value)) return Item.Key;
                    }
                    else if (DestinationType != null && ValueType != null)
                    {
                        var Converter = TypeDescriptor.GetConverter(Item.Value);
                        var ValueConverter = TypeDescriptor.GetConverter(value);
                        if (Converter.CanConvertTo(ValueType))
                        {
                            var ConvertedValue = Converter.ConvertTo(Item.Value, ValueType);
                            if (Equals(ConvertedValue, value)) return Item.Key;
                        }
                        else if (Converter.CanConvertFrom(ValueType))
                        {
                            var ConvertedValue = Converter.ConvertFrom(value);
                            if (Equals(Item.Value, ConvertedValue)) return Item.Key;
                        }
                        else if (ValueConverter.CanConvertTo(DestinationType))
                        {
                            var ConvertedValue = Converter.ConvertTo(value, DestinationType);
                            if (Equals(Item.Value, ConvertedValue)) return Item.Key;
                        }
                        else if (ValueConverter.CanConvertFrom(ValueType))
                        {
                            var ConvertedValue = Converter.ConvertFrom(Item.Value);
                            if (Equals(ConvertedValue, value)) return Item.Key;
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unknown lookup item type");
                }

            //Not found - Fallback: return unconverted value
            return value;
        }

        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var Result = Convert(value);

            DebugLogger.WriteLine(this, LoggingLevel.Normal,
                () => string.Format("ObjectLookupConverter '{2}': '{0}' -> '{1}'", value, Result, Name));

            return Result;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var Result = ConvertBack(value);

            DebugLogger.WriteLine(this, LoggingLevel.Normal,
                () => string.Format("ObjectLookupConverter convert back '{2}': '{0}' -> '{1}'", value, Result, Name));

            return Result;
        }

        #endregion
    }
}