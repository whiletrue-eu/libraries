using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace WhileTrue.Classes.XTransformer
{
    internal class StylesheetExtensionMethods
    {
        private readonly Dictionary<string, StylesheetTransformer> stylesheets = new Dictionary<string, StylesheetTransformer>();
        private readonly XmlResolver resolver;
        private readonly Dictionary<string, object> extensions;
        private readonly string dataBaseUri;
        private readonly Stack<StylesheetTransformer> stylesheetCallstack = new Stack<StylesheetTransformer>();

        /// <summary/>
        public StylesheetExtensionMethods(XmlResolver resolver, Dictionary<string, object> extensions, string dataBaseUri)
        {
            this.extensions = extensions;
            this.dataBaseUri = dataBaseUri;
            this.resolver = resolver;
        }

        internal void LoadStylesheet(string id, Uri path)
        {
            if (this.stylesheets.ContainsKey(id))
            {
                throw new InvalidOperationException($"stylesheet with id '{id}' is already loaded");
            }
            StylesheetTransformer Transformer = new StylesheetTransformer(path, this.resolver, this.dataBaseUri);
            this.stylesheets.Add(id, Transformer);
        }

        // ReSharper disable InconsistentNaming
        public string transform(string stylesheetFile, IXPathNavigable input)
        {
            return this.Transform(stylesheetFile, input, new Dictionary<string, object>());
        }

        public string resolve(string uri)
        {
            Uri Uri = this.resolver.ResolveUri(this.stylesheetCallstack.Peek().StylesheetUri, uri);
            using (StreamReader Reader = new StreamReader((Stream)this.resolver.GetEntity(Uri, null, typeof(Stream))))
            {
                return Reader.ReadToEnd();
            }
        }

        public object eval(XPathNavigator context, string xpath)
        {
            return context.Evaluate(xpath);
        }
        // ReSharper restore InconsistentNaming

        internal string Transform(string stylesheetFile, IXPathNavigable input, Dictionary<string, object> arguments)
        {
            if (this.stylesheets.ContainsKey(stylesheetFile) == false)
            {
                this.LoadStylesheet(stylesheetFile, new Uri(stylesheetFile, UriKind.RelativeOrAbsolute));
            }

            this.stylesheetCallstack.Push(this.stylesheets[stylesheetFile]);

            try
            {
                return this.stylesheetCallstack.Peek().Transform(input, arguments, this.extensions);
            }
            finally
            {
                this.stylesheetCallstack.Pop();
            }
            
        }
        private class StylesheetTransformer
        {
            private readonly XmlResolver resolver;
            private readonly XslCompiledTransform transform;

            /// <summary/>
            public StylesheetTransformer(Uri stylesheetUri, XmlResolver resolver, string dataBaseUri)
            {
                this.StylesheetUri = stylesheetUri;
                this.resolver = resolver;
                this.transform = new XslCompiledTransform(Debugger.IsAttached);
                XmlReader Reader = XmlReader.Create(stylesheetUri.ToString(), new XmlReaderSettings { XmlResolver = resolver });
                this.transform.Load(Reader, XsltSettings.TrustedXslt, resolver);
            }

            public Uri StylesheetUri { get; }


            public string Transform(IXPathNavigable input, Dictionary<string, object> parameter, Dictionary<string, object> extensions)
            {
                using (MemoryStream OutputStream = new MemoryStream())
                {
                    XsltArgumentList Arguments = new XsltArgumentList();
                    foreach (KeyValuePair<string, object> Parameter in parameter)
                    {
                        Arguments.AddParam(Parameter.Key, string.Empty, Parameter.Value);
                    }
                    foreach (KeyValuePair<string, object> Extension in extensions)
                    {
                        Arguments.AddExtensionObject(Extension.Key, Extension.Value);
                    }
                    Arguments.XsltMessageEncountered += StylesheetTransformer.XsltMessageEncountered;

                    this.transform.Transform(input, Arguments, new XmlTextWriter(new StreamWriter(OutputStream)), this.resolver);

                    OutputStream.Seek(0, SeekOrigin.Begin);

                    using (TextReader Reader = new StreamReader(OutputStream))
                    {
                        return Reader.ReadToEnd();
                    }
                }
            }

            private static void XsltMessageEncountered(object sender, XsltMessageEncounteredEventArgs e)
            {
                string Message = e.Message.Trim();
                Trace.WriteLine(Message);
            }
        }

    }
}