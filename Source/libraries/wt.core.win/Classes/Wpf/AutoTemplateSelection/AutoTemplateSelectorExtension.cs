// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable 1591
using System;
using System.Windows;
using System.Windows.Markup;

namespace WhileTrue.Classes.Wpf
{
    /// <summary/>
    public class AutoTemplateSelectorExtension : MarkupExtension
    {
        [ConstructorArgument("view")]
        public string View{ get; set;}

        public AutoTemplateSelectorExtension():this(""){}

        public AutoTemplateSelectorExtension(string view)
        {
            this.View = view;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget Target = (IProvideValueTarget) serviceProvider.GetService(typeof (IProvideValueTarget));

            if (Target.TargetObject is FrameworkElement)
            {
                return new AutoDataTemplateSelector((FrameworkElement) Target.TargetObject, this.View);
            }
            else
            {
                return this;
            }
        }
    }
}