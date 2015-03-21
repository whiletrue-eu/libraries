using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace WhileTrue.Classes.Timple
{
    /// <summary>
    /// Provides a simple templating engine for C# Source Code
    /// </summary>
    /// <remarks>
    /// Templating supports attribute replacement, multiplying of code bloks and 'template comments'
    /// that denote text removed during the resolving of the template.<br/>
    /// When using the engine, you have to provide two parameters:<br/>
    /// a) The template text to be processed<br/>
    /// b) and XML document that defines the attributes and structure to be applied to the template<br/>
    /// <br/>
    /// in the XML document two features are used for templating, attributes that allow text passages denoted 
    /// in the template to be replaced by the attribute value noted in the XML element, and elements which represent
    /// code bloks that are multiplied for each element that occurs in the XML structure.<br/>
    /// In code blocks, again attributes or inner code blocks can be used.<br/>
    /// The template engine uses the names defined in attribute markups and code block markups to identify the 
    /// correlating attributes or elements defined in the XML document.<br/>
    /// <br/>
    /// The following template control parameters are supported:
    /// * Attribute replacement, either visible to the compiler or as a code comment<br/>
    /// Markup: <c>__AttributeName__</c> or <c>/*__AttributeName__*/</c><br/>
    /// The engine will search for an attribute with the name 'AttributeName' and replaces it with the text defined as value
    /// <example>
    /// Template:
    /// <code>
    /// /*__Accessibility__*/ class __ClassName__
    /// {
    ///     public __ClassName__()
    ///     {
    ///     }
    /// }
    /// </code>
    /// XML Parameter:
    /// <code>
    /// <Timple Accessibility="public" ClassName="MyClass"/>
    /// </code>
    /// Output:
    /// <code>
    /// public class MyClass
    /// {
    ///     public MyClass()
    ///     {
    ///     }
    /// }
    /// </code>
    /// </example>
    /// * Template Comment deletion<br/>
    /// Markup: <c>/*!!*/Comment/*!!*/</c><br/>
    /// The engine will search for the comment and removes it from the output.
    /// This is handy to supply keyword/identifiers to the complier to make the code meaningful that 
    /// are not required later in the output.
    /// <example>
    /// Template:
    /// <code>
    /// /*__Accessibility__*/ /*!!*/internal/*!!*/ class __ClassName__
    /// {
    ///     public __ClassName__()
    ///     {
    ///     }
    /// }
    /// </code>
    /// XML Parameter:
    /// <code>
    /// <Timple Accessibility="public" ClassName="MyClass"/>
    /// </code>
    /// Output:
    /// <code>
    /// public  class MyClass
    /// {
    ///     public MyClass()
    ///     {
    ///     }
    /// }
    /// </code>
    /// </example>
    /// * Code Blocks that are multiplied by the number of element occurences in the XML. With the same feature, it is also possible to create conditional code blocks.<br/>
    /// Markup: <c>/*BlockName>>*/ ... /*&lt;&lt;BlockName*/</c><br/>
    /// The engine will search for elements with the name 'BlockName' and duplicates its content. Inside a code block, attribute replacements or other code blocks may be defined.<br/>
    /// <example>
    /// Template:
    /// <code>
    /// /*__Accessibility__*/ class __ClassName__
    /// {
    ///     public __ClassName__()
    ///     {
    ///     }
    /// 
    /// /*Property>>*/
    ///     public __Type__ __Name__
    ///     {
    /// /*Setter>>*/
    ///         set
    ///         {
    ///             this.__FieldName__ = value;
    ///         }
    /// /*&lt;&lt;Setter*/
    ///         get
    ///         {
    ///             return this.__FieldName__;
    ///         }
    ///     }
    /// /*&lt;&lt;Property*/
    /// }
    /// </code>
    /// It is also possible to generate block content if a certain element is not defined. For this, you can prefix the 
    /// block name with a '!': <c>/*!BlockName>>*/ ... /*&lt;&lt;!BlockName*/</c>. The scope for replacements within the
    /// block will be the same as before.<br/>
    /// XML Parameter:
    /// <code>
    /// <Timple Accessibility="public" ClassName="MyClass">
    ///     <Property Name="GetSetProperty" Type="int" FieldName="getset">
    ///         <Setter/>
    ///     </Property>
    ///     <Property Name="GetOnlyProperty" Type="string" FieldName="getonly"/>
    /// </Timple>
    /// </code>
    /// Output:
    /// <code>
    /// public class MyClass
    /// {
    ///     public MyClass()
    ///     {
    ///     }
    ///
    /// 
    ///     public int GetSetProperty
    ///     {
    /// 
    ///         set
    ///         {
    ///             this.getset = value;
    ///         }
    /// 
    ///         get
    ///         {
    ///             return this.getset;
    ///         }
    ///     }
    /// 
    /// 
    /// 
    ///     public string GetOnlyProperty
    ///     {
    /// 
    ///         get
    ///         {
    ///             return this.getonly;
    ///         }
    ///     }
    /// 
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public class TimpleEngine
    {
        private readonly string template;
        private readonly XmlElement parameter;
        private static Regex parser;

        static TimpleEngine()
        {
            TimpleEngine.parser =
                new Regex(
                    @"
                    (
                        (?<=\r\n|^)\p{Z}*/\*(?<BlockName>.*?)>>\*/\p{Z}*(\r\n|$) #Captures the complete line if the only thing on that line is the block beginning
                        |
                        /\*(?<BlockName>.*?)>>\*/
                    )
                    (?<BlockContent>.*?)
                    (
                        (?<=\r\n|^)\p{Z}*/\*<<\k<BlockName>\*/\p{Z}*(\r\n|$) #Captures the complete line if the only thing on that line is the block ending
                        | 
                        /\*<<\k<BlockName>\*/
                    )
                    |
                    __(?<AttributeName>.*?)__
                    |
                    /\*__(?<AttributeName>.*?)__\*/
                    |
                    /\*!!\*/(?<Comment>.*?)/\*!!\*/
                    ",
                    RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
        }

        public TimpleEngine(string template, XmlElement parameter)
        {
            if( parameter == null)
            {
                throw new ArgumentException("parameter");
            }
            this.template = template;
            this.parameter = parameter;
        }

        public string Process()
        {
            return TimpleEngine.parser.Replace(this.template, this.MatchEvaluator);
        }

        public const string SchemaUri="http://www.w3.org/2001/XMLSchema";
        public static string GenerateXsd(string template, string rootName, string targetNamespace)
        {
            XmlDocument SchemaDocument = new XmlDocument();
            XmlElement SchemaDeclaration = SchemaDocument.CreateElement("xsd:schema",SchemaUri);
            SchemaDeclaration.SetAttribute("elementFormDefault","qualified");
            SchemaDeclaration.SetAttribute("targetNamespace",targetNamespace);
            SchemaDocument.AppendChild(SchemaDeclaration);
            Match Match = TimpleEngine.parser.Match(template);

            XmlElement ElementDeclaration = SchemaDocument.CreateElement("xsd:element",SchemaUri);
            ElementDeclaration.SetAttribute("name", rootName);
            SchemaDeclaration.AppendChild(ElementDeclaration);

            XmlElement TypeDeclaration = SchemaDocument.CreateElement("xsd:complexType",SchemaUri);
            ElementDeclaration.AppendChild(TypeDeclaration);
            GenerateXsdElement(TypeDeclaration, Match);
            
            return SchemaDocument.OuterXml;
        }

        private static void GenerateXsdElement(XmlElement parentElement, Match match)
        {
            Match Match = match;
            List<string> Attributes = new List<string>();
            Dictionary<string,string> Elements = new Dictionary<string, string>();

            while (Match.Success)
            {
                if (Match.Groups["AttributeName"].Success)
                {
                    string AttributeName = Match.Groups["AttributeName"].Value;
                    if (Attributes.Contains(AttributeName) == false)
                    {
                        Attributes.Add(AttributeName);
                    }
                }
                else if (Match.Groups["BlockName"].Success)
                {
                    string BlockName = Match.Groups["BlockName"].Value;
                    string BlockContent = Match.Groups["BlockContent"].Value;

                    if (Elements.ContainsKey(BlockName) == false)
                    {
                        Elements.Add(BlockName, BlockContent);
                    }
                    else
                    {
                        Elements[BlockName] = Elements[BlockName]+BlockContent;
                    }
                }

                Match = Match.NextMatch();
            }

            if (Elements.Count > 0)
            {
                XmlElement Choice = parentElement.OwnerDocument.CreateElement("xsd:choice", SchemaUri);
                Choice.SetAttribute("minOccurs", "0");
                Choice.SetAttribute("maxOccurs", "unbounded");
                parentElement.AppendChild(Choice);

                foreach (KeyValuePair<string, string> Element in Elements)
                {
                    XmlElement ElementDeclaration = parentElement.OwnerDocument.CreateElement("xsd:element", SchemaUri);
                    ElementDeclaration.SetAttribute("name", Element.Key);
                    XmlElement TypeDeclaration = parentElement.OwnerDocument.CreateElement("xsd:complexType", SchemaUri);
                    ElementDeclaration.AppendChild(TypeDeclaration);

                    GenerateXsdElement(TypeDeclaration, TimpleEngine.parser.Match(Element.Value));

                    Choice.AppendChild(ElementDeclaration);
                }
                parentElement.AppendChild(Choice);
            }

            foreach (string Attribute in Attributes)
            {
                XmlElement AttributeDeclaration = parentElement.OwnerDocument.CreateElement("xsd:attribute", SchemaUri);
                AttributeDeclaration.SetAttribute("name", Attribute);
                AttributeDeclaration.SetAttribute("type", "xsd:string");
                AttributeDeclaration.SetAttribute("use", "optional");
                parentElement.AppendChild(AttributeDeclaration);
            }
        }

        private string MatchEvaluator(Match match)
        {
            if( match.Groups["AttributeName"].Success )
            {
                string AttributeName = match.Groups["AttributeName"].Value;
                return TimpleEngine.ResolveAttribute(this.parameter,AttributeName);
            }
            else if (match.Groups["BlockName"].Success )
            {
                string BlockName = match.Groups["BlockName"].Value;
                string BlockContent = match.Groups["BlockContent"].Value;

                StringBuilder Content = new StringBuilder();
                if (BlockName.StartsWith("!") == false)
                {
                    XmlNodeList Nodes = this.parameter.SelectNodes(BlockName);
                    if (Nodes != null)
                    {
                        // If / ForEach
                        foreach (XmlElement BlockParameter in Nodes)
                        {
                            Content.Append(new TimpleEngine(BlockContent, BlockParameter).Process());
                        }
                    }
                }
                else
                {
                    XmlNodeList Nodes = this.parameter.SelectNodes(BlockName.Substring(1));
                    // If Not (element prefixed by '!')
                    if( Nodes != null && Nodes.Count > 1 )
                    {
                        Content.Append(new TimpleEngine(BlockContent, this.parameter).Process());
                    }
                }
                return Content.ToString();
            }
            else if (match.Groups["Comment"].Success )
            {
                return "";
            }
            else
            {
                return "";
            }
        }

        private static string ResolveAttribute(XmlElement xmlElement, string attributeName)
        {
            if( xmlElement.HasAttribute(attributeName))
            {
                return xmlElement.GetAttribute(attributeName);
            }
            else
            {
                if( xmlElement.ParentNode as XmlElement != null )
                {
                    return TimpleEngine.ResolveAttribute((XmlElement) xmlElement.ParentNode, attributeName);
                }
                else
                {
                    return "";
                }
            }
        }
    }
}
