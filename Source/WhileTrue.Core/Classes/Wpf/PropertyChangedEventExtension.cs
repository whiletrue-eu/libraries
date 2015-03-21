#pragma warning disable 1591
// ReSharper disable UnusedMember.Global
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Wpf
{
    public class PropertyChangedEventExtension : MarkupExtension
    {
        private string eventName;

        public PropertyChangedEventExtension()
        {
        }

        public PropertyChangedEventExtension(string eventName)
        {
            this.eventName = eventName;
        }

        [ConstructorArgument("eventName")]
        public string EventName
        {
            get { return this.eventName; }
            set { this.eventName = value; }
        }  
        
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return PropertyChangedRoutedEventFactory.GetRoutedEvent(string.Format("{0}Changed", this.eventName));
        }
    }
}