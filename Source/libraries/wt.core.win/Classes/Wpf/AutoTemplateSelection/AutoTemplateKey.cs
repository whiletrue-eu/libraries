using System;

namespace WhileTrue.Classes.Wpf
{
    internal class AutoTemplateKey
    {
        private readonly Type type;
        private readonly string view;

        public AutoTemplateKey(Type type, string view)
        {
            this.type = type;
            this.view = view;
        }

        public override bool Equals(object other)
        {
            if (other is AutoTemplateKey)
            {
                AutoTemplateKey OtherKey = (AutoTemplateKey) other;
                return this.type == OtherKey.type && this.view == OtherKey.view;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.type.GetHashCode() & this.view.GetHashCode();
        }

        public override string ToString()
        {
            return $"Auto template for type {this.type}, view {this.view}";
        }
    }
}