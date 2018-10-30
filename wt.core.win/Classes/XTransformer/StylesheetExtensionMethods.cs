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
        private readonly string dataBaseUri;
        private readonly Dictionary<string, object> extensions;
        private readonly XmlResolver resolver;
        private readonly Stack<StylesheetTransformer> stylesheetCallstack = new Stack<StylesheetTransformer>();

        private readonly Dictionary<string, StylesheetTransformer> stylesheets =
            new Dictionary<string, StylesheetTransformer>();

        /// <summary />
        public StylesheetExtensionMethods(XmlResolver resolver, Dictionary<string, object> extensions,
            string dataBaseUri)
        {
            this.extensions = extensions;
            this.dataBaseUri = dataBaseUri;
            this.resolver = resolver;
        }

        internal void LoadStylesheet(string id, Uri path)
        {
            if (stylesheets.ContainsKey(id))
                throw new InvalidOperationException($"stylesheet with id '{id}' is already loaded");
            var Transformer = new StylesheetTransformer(path, resolver, dataBaseUri);
            stylesheets.Add(id, Transformer);
        }

        internal string Transform(string stylesheetFile, IXPathNavigable input, Dictionary<string, object> arguments)
        {
            if (stylesheets.ContainsKey(stylesheetFile) == false)
                LoadStylesheet(stylesheetFile, new Uri(stylesheetFile, UriKind.RelativeOrAbsolute));

            stylesheetCallstack.Push(stylesheets[stylesheetFile]);

            try
            {
                return stylesheetCallstack.Peek().Transform(input, arguments, extensions);
            }
            finally
            {
                stylesheetCallstack.Pop();
            }
        }

        private class StylesheetTransformer
        {
            private readonly XmlResolver resolver;
            private readonly XslCompiledTransform transform;

            /// <summary />
            public StylesheetTransformer(Uri stylesheetUri, XmlResolver resolver, string dataBaseUri)
            {
                StylesheetUri = stylesheetUri;
                this.resolver = resolver;
                transform = new XslCompiledTransform(Debugger.IsAttached);
                var Reader = XmlReader.Create(stylesheetUri.ToString(), new XmlReaderSettings {XmlResolver = resolver});
                transform.Load(Reader, XsltSettings.TrustedXslt, resolver);
            }

            public Uri StylesheetUri { get; }


            public string Transform(IXPathNavigable input, Dictionary<string, object> parameter,
                Dictionary<string, object> extensions)
            {
                using (var OutputStream = new MemoryStream())
                {
                    var Arguments = new XsltArgumentList();
                    foreach (var Parameter in parameter)
                        Arguments.AddParam(Parameter.Key, string.Empty, Parameter.Value);
                    foreach (var Extension in extensions) Arguments.AddExtensionObject(Extension.Key, Extension.Value);
                    Arguments.XsltMessageEncountered += XsltMessageEncountered;

                    transform.Transform(input, Arguments, new XmlTextWriter(new StreamWriter(OutputStream)), resolver);

                    OutputStream.Seek(0, SeekOrigin.Begin);

                    using (TextReader Reader = new StreamReader(OutputStream))
                    {
                        return Reader.ReadToEnd();
                    }
                }
            }

            private static void XsltMessageEncountered(object sender, XsltMessageEncounteredEventArgs e)
            {
                var Message = e.Message.Trim();
                Trace.WriteLine(Message);
            }
        }

        // ReSharper disable InconsistentNaming
        public string transform(string stylesheetFile, IXPathNavigable input)
        {
            return Transform(stylesheetFile, input, new Dictionary<string, object>());
        }

        public string resolve(string uri)
        {
            var Uri = resolver.ResolveUri(stylesheetCallstack.Peek().StylesheetUri, uri);
            using (var Reader = new StreamReader((Stream) resolver.GetEntity(Uri, null, typeof(Stream))))
            {
                return Reader.ReadToEnd();
            }
        }

        public object eval(XPathNavigator context, string xpath)
        {
            return context.Evaluate(xpath);
        }
        // ReSharper restore InconsistentNaming
    }
}