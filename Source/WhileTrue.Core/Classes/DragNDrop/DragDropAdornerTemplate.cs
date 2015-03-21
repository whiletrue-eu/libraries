using System;
using System.Windows;
using System.Windows.Markup;

namespace WhileTrue.Classes.DragNDrop
{
    [DictionaryKeyProperty("TemplateKey")]
    public class DragDropAdornerTemplate : DataTemplate
    {
        public ComponentResourceKey TemplateKey
        {
            get
            {
                return new ComponentResourceKey(typeof(DragDrop),this.TargetType);
            }
        }

        public Type TargetType { get; set; }

    }
   
}