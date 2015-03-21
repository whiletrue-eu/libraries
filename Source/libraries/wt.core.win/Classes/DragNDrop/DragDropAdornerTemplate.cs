using System;
using System.Windows;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace WhileTrue.Classes.DragNDrop
{
    /// <summary>
    /// Implements a resource dictionary key for Adorner templates for a specific type
    /// </summary>
    [DictionaryKeyProperty("TemplateKey")]
    [PublicAPI]
    public class DragDropAdornerTemplate : DataTemplate
    {
        /// <summary>
        /// Returns the key for use in resource dictionaries
        /// </summary>
        public ComponentResourceKey TemplateKey => new ComponentResourceKey(typeof(DragDrop),this.TargetType);

        /// <summary>
        /// Sets/gets the type for which the adorner template is defined
        /// </summary>
        public Type TargetType { get; set; }

    }
   
}