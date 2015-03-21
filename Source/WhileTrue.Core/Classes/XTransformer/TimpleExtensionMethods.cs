using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using WhileTrue.Classes.Timple;

namespace WhileTrue.Classes.XTransformer
{
    internal class TimpleExtensionMethods
    {
        public string process(string template, IXPathNavigable structure)
        {
            XmlDocument Structure = new XmlDocument();
            try
            {
                Structure.LoadXml(structure.CreateNavigator().OuterXml);
            }
            catch (XmlException Exception)
            {
                throw new XsltException("Error in 'process': template structre could not be loaded as XML",Exception);
            }
            return new TimpleEngine(template, Structure.DocumentElement).Process();
        }

        public string generateXsd(string template, string rootName, string targetNamespace)
        {
            return TimpleEngine.GenerateXsd(template, rootName, targetNamespace);
        }
    }
}