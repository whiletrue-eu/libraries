using System.Xml;

namespace Blabber.Classes.Utils
{
    public static class XmlStreamUtils
    {
        public static XmlElement ReadAsXmlElement(this XmlReader reader)
        {
            using (XmlReader ElementReader = reader.ReadSubtree())
            {
                ElementReader.Read();
                string ElementContent = ElementReader.ReadOuterXml();

                XmlDocumentFragment fragment = new XmlDocument().CreateDocumentFragment();
                fragment.InnerXml = ElementContent;
                ElementReader.Close();

                return (XmlElement) fragment.FirstChild;
            }
        }

        public static XmlElement ReadStartAsXmlElement(this XmlReader reader)
        {
            XmlElement Element = new XmlDocument().CreateElement(reader.Name, reader.NamespaceURI);
            int AttributeCount = reader.AttributeCount;
            for (int Index = 0; Index < AttributeCount; Index++)
            {
                reader.MoveToAttribute(Index);
                Element.SetAttribute(reader.Name, reader.Value);
            }

            return Element;
        }
    }
}