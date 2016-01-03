
// ReSharper disable UnusedMember.Global
using System;
using System.Windows.Markup;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Allows a property changed event to be thrown as routed event in a wpf UI.
    /// This extension provides the correct routed event for the given EventName, which is the name of the property that changes shall be caught for.
    /// </summary>
    public class PropertyChangedEventExtension : MarkupExtension
    {
        /// <summary/>
        public PropertyChangedEventExtension()
        {
        }

        /// <summary/>
        public PropertyChangedEventExtension(string eventName)
        {
            this.EventName = eventName;
        }

        /// <summary>
        /// Name of the property to be caught
        /// </summary>
        [ConstructorArgument("eventName")]
        public string EventName { get; set; }

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension. 
        /// </summary>
        /// <returns>
        /// The object value to set on the property where the extension is applied. 
        /// </returns>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        public override object ProvideValue(IServiceProvider serviceProvider) => PropertyChangedRoutedEventFactory.GetRoutedEvent($"{this.EventName}Changed");
    }
}