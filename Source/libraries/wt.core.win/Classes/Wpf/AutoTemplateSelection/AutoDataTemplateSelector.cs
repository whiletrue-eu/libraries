// ReSharper disable UnusedMember.Global
using System;
using System.Windows;
using System.Windows.Controls;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Provides data template variants (per 'view' string) by using the template selector capabilities of controls.
    /// TO mark the dataTemplates, use <see cref="DataTemplateKey"/>s on the DataTemplates
    /// </summary>
    public class AutoDataTemplateSelector : DataTemplateSelector
    {
        private readonly FrameworkElement frameworkElement;
        private readonly string view;

        ///<summary/>
        public AutoDataTemplateSelector(FrameworkElement frameworkElement)
            :this(frameworkElement,"")
        {
        }

        ///<summary/>
        public AutoDataTemplateSelector(FrameworkElement frameworkElement, string view)
        {
            this.frameworkElement = frameworkElement;
            this.view = view;
        }

        ///<summary/>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate Template = this.TryFindTemplate(item.GetType());
            return Template ?? base.SelectTemplate(item, container);
        }

        private DataTemplate TryFindTemplate(Type itemType)
        {
            if (itemType != null)
            {
                object Resource = this.frameworkElement.TryFindResource(new AutoTemplateKey(itemType, this.view));
                if (Resource != null)
                {
                    return (DataTemplate) Resource;
                }
                else
                {
                    foreach (Type InterfaceType in itemType.GetInterfaces())
                    {
                        DataTemplate Template = this.TryFindTemplate(InterfaceType);
                        if (Template != null)
                        {
                            return Template;
                        }
                    }
                    return this.TryFindTemplate(itemType.BaseType);
                }
            }
            else
            {
                return null;
            }
        }
    }
}