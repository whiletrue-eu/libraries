using System.Xml;

namespace Blabber.Classes.Utils
{
    public static class XmlDOMUtils
    {
        public static string TryGetAttributeValue(this XmlElement node, string attribute)
        {
            return node.HasAttribute(attribute) ? node.Attributes[attribute].Value : null;
        }
    }
}