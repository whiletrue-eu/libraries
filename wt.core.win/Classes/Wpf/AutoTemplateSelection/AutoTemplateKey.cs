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
                var OtherKey = (AutoTemplateKey) other;
                return type == OtherKey.type && view == OtherKey.view;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return type.GetHashCode() & view.GetHashCode();
        }

        public override string ToString()
        {
            return $"Auto template for type {type}, view {view}";
        }
    }
}