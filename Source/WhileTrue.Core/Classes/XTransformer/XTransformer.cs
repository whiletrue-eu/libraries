using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace WhileTrue.Classes.XTransformer
{
    public class XTransformer
    {
        private readonly XmlResolver resolver;
        private readonly Dictionary<string, object> extensions = new Dictionary<string, object>();
        private readonly StylesheetExtensionMethods styleSheetExtension;

        /// <summary/>
        public XTransformer(Uri mainStylesheetFile, XmlResolver resolver)
            :this(mainStylesheetFile,resolver, resolver.ResolveUri(mainStylesheetFile,"").IsFile?resolver.ResolveUri(mainStylesheetFile,"").LocalPath:null)
        {

        }

        /// <summary/>
        public XTransformer(Uri mainStylesheetFile, XmlResolver resolver, string dataBaseUri)
        {
            this.resolver = resolver;

            this.styleSheetExtension = new StylesheetExtensionMethods(this.resolver, this.extensions, dataBaseUri);

            this.extensions.Add("ext:stylesheets", this.styleSheetExtension);
            this.extensions.Add("ext:timple", new TimpleExtensionMethods());
            this.extensions.Add("ext:xml", new XmlExtensionMethods());
            if (dataBaseUri != null)
            {
                this.extensions.Add("ext:file", new FileExtensionMethods(dataBaseUri));
            }

            this.styleSheetExtension.LoadStylesheet(string.Empty, mainStylesheetFile);
        }

        public string Transform(string input)
        {
            return this.Transform(input, new Dictionary<string, object>());
        }

        public string Transform(string input, Dictionary<string, object> arguments)
        {
            return this.styleSheetExtension.Transform(string.Empty, new XPathDocument(new XmlTextReader(new StringReader(input))), arguments);
        }
    }
}
