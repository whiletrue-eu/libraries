using System;
using System.IO;
using System.Text;
using System.Xml;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.XTransformer;

namespace WhileTrue
{
    public class Program
    {
        public static void Main(string[] commandLine)
        {
            string Input = commandLine[0];
            string Script = commandLine[1];
            string Output = commandLine[2];

            XTransformer Transformer = new XTransformer(new Uri(Script), new XmlUrlResolverEx());
            string Data = Transformer.Transform(File.ReadAllText(Input));

            try
            {
                XmlDocument Doc = new XmlDocument();
                Doc.LoadXml(Data);
                XmlTextWriter Writer = new XmlTextWriter(Output, Encoding.UTF8);
                Writer.Formatting = Formatting.Indented;
                Doc.WriteTo(Writer);
                Writer.Flush();
            }
            catch
            {
                File.WriteAllText(Output, Data);
            }
        }
    }
}
