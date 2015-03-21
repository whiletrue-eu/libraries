using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace WhileTrue.Classes.XTransformer
{
    internal class XmlExtensionMethods
    {
        public IXPathNavigable toXmlFragment(string xml)
        {
            return new XPathDocument(XmlReader.Create(new StringReader(xml)));
        }
    }
}