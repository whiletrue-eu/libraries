using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using JetBrains.Annotations;

namespace WhileTrue.Classes.XTransformer
{
    /// <summary>
    /// Implements an XSL stylsheet processor that can be easily extended by extension functions
    /// </summary>
    [PublicAPI]
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
            this.extensions.Add("ext:xml", new XmlExtensionMethods());
            if (dataBaseUri != null)
            {
                this.extensions.Add("ext:file", new FileExtensionMethods(dataBaseUri));
            }

            this.styleSheetExtension.LoadStylesheet(string.Empty, mainStylesheetFile);
        }

        /// <summary>
        /// Transforms the string input and returns the resulting document
        /// </summary>
        public string Transform(string input, Dictionary<string, object> arguments=null)
        {
            return this.styleSheetExtension.Transform(string.Empty, new XPathDocument(new XmlTextReader(new StringReader(input))), arguments?? new Dictionary<string, object>());
        }
    }
}
