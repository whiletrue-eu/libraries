// ReSharper disable UnusedMember.Global

using System;
using System.Windows;
using System.Windows.Controls;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    ///     Provides data template variants (per 'view' string) by using the template selector capabilities of controls.
    ///     TO mark the dataTemplates, use <see cref="DataTemplateKey" />s on the DataTemplates
    /// </summary>
    public class AutoDataTemplateSelector : DataTemplateSelector
    {
        private readonly FrameworkElement frameworkElement;
        private readonly string view;

        /// <summary />
        public AutoDataTemplateSelector(FrameworkElement frameworkElement)
            : this(frameworkElement, "")
        {
        }

        /// <summary />
        public AutoDataTemplateSelector(FrameworkElement frameworkElement, string view)
        {
            this.frameworkElement = frameworkElement;
            this.view = view;
        }

        /// <summary />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var Template = TryFindTemplate(item.GetType());
            return Template ?? base.SelectTemplate(item, container);
        }

        private DataTemplate TryFindTemplate(Type itemType)
        {
            if (itemType != null)
            {
                var Resource = frameworkElement.TryFindResource(new AutoTemplateKey(itemType, view));
                if (Resource != null) return (DataTemplate) Resource;

                foreach (var InterfaceType in itemType.GetInterfaces())
                {
                    var Template = TryFindTemplate(InterfaceType);
                    if (Template != null) return Template;
                }

                return TryFindTemplate(itemType.BaseType);
            }

            return null;
        }
    }
}