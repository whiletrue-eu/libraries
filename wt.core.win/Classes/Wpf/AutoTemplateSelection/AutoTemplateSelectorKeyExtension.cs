using System;
using System.Reflection;
using System.Windows;

namespace WhileTrue.Classes.Wpf
{
    /// <summary/>
    public class AutoTemplateSelectorKeyExtension : ResourceKey
    {
        private readonly Type type;
        private readonly string view;

        /// <summary/>
        public AutoTemplateSelectorKeyExtension(Type type)
            :this(type,"")
        {
        }

        /// <summary/>
        public AutoTemplateSelectorKeyExtension(Type type, string view)
        {
            this.type = type;
            this.view = view;
        }

        /// <summary/>
        public override Assembly Assembly => null;

        /// <summary/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new AutoTemplateKey(this.type, this.view);
        }
    }
}