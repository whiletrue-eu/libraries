using System.Xml;
using NUnit.Framework;

namespace WhileTrue.Classes.Timple
{
    [TestFixture]
    public class TimpleEngineTest
    {
        [Test]
        public void Attribute_placeholder_must_be_replaced_by_attribute_value()
        {
            string Template = @"
pre__ATTRIBUTE__post
";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(@"<Timple ATTRIBUTE=""Replace""/>");

            XmlElement Parameter = Doc.DocumentElement;
            
            string Result = new TimpleEngine(Template, Parameter).Process();

            Assert.AreEqual(@"
preReplacepost
",Result);
        }

        [Test]
        public void Attribute_comment_placeholder_must_be_replaced_by_attribute_value()
        {
            string Template = @"
pre/*__ATTRIBUTE__*/post
";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(@"<Timple ATTRIBUTE=""Replace""/>");

            XmlElement Parameter = Doc.DocumentElement;

            string Result = new TimpleEngine(Template, Parameter).Process();

            Assert.AreEqual(@"
preReplacepost
", Result);
        }
        
        [Test]
        public void Attributes_shall_be_resolved_recursively_when_not_defined_on_the_current_element()
        {
            string Template = @"
/*BLOCK>>*/__ATTRIBUTE__/*<<BLOCK*/
";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(@"<Timple ATTRIBUTE=""Replace""><BLOCK/></Timple>");

            XmlElement Parameter = Doc.DocumentElement;

            string Result = new TimpleEngine(Template, Parameter).Process();

            Assert.AreEqual(@"
Replace
", Result);
        }

        [Test]
        public void Attributes_shall_be_resolved_recursively_and_yield_empty_string_if_not_found()
        {
            string Template = @"
/*BLOCK>>*/__ATTRIBUTE__/*<<BLOCK*/
";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(@"<Timple><BLOCK/></Timple>");

            XmlElement Parameter = Doc.DocumentElement;

            string Result = new TimpleEngine(Template, Parameter).Process();

            Assert.AreEqual(@"

", Result);
        }

        [Test]
        public void Template_comment_block_has_to_be_removed()
        {
            string Template = @"pre/*!!*/comment/*!!*/post";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(@"<Timple/>");

            XmlElement Parameter = Doc.DocumentElement;

            string Result = new TimpleEngine(Template, Parameter).Process();

            Assert.AreEqual(@"prepost", Result);
        }
        
        [Test]
        public void Code_block_has_to_be_repeated_according_to_element_ocurance()
        {
            string Template = @"
/*BLOCK>>*/Block/*<<BLOCK*/
";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(@"<Timple><BLOCK/><BLOCK/></Timple>");

            XmlElement Parameter = Doc.DocumentElement;

            string Result = new TimpleEngine(Template, Parameter).Process();

            Assert.AreEqual(@"
BlockBlock
", Result);
        }
        
        [Test]
        public void Code_blocks_must_work_over_multiple_lines()
        {
            string Template = @"
/*BLOCK>>*/Bl
ock/*<<BLOCK*/
";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(@"<Timple><BLOCK/><BLOCK/></Timple>");

            XmlElement Parameter = Doc.DocumentElement;

            string Result = new TimpleEngine(Template, Parameter).Process();

            Assert.AreEqual(@"
Bl
ockBl
ock
", Result);
        }        

        [Test]
        public void Code_block_markup_that_is_written_in_a_single_line_should_be_replaced_without_crlf()
        {
            string Template = @"
/*BLOCK>>*/
Block
/*<<BLOCK*/
";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(@"<Timple><BLOCK/><BLOCK/></Timple>");

            XmlElement Parameter = Doc.DocumentElement;

            string Result = new TimpleEngine(Template, Parameter).Process();

            Assert.AreEqual(@"
Block
Block
", Result);
        }

        [Test]
        public void Code_block_markup_that_is_written_in_a_single_line_should_be_replaced_without_crlf_and_trimmed_from_whitespaces()
        {
            string Template = @"
    /*BLOCK>>*/
    Block
    /*<<BLOCK*/
";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(@"<Timple><BLOCK/><BLOCK/></Timple>");

            XmlElement Parameter = Doc.DocumentElement;

            string Result = new TimpleEngine(Template, Parameter).Process();

            Assert.AreEqual(@"
    Block
    Block
", Result);
        }

        [Test]
        public void Code_block_has_to_be_repeated_according_to_element_ocurance_and_internal_attributes_and_blocks_must_be_recursively_replaced()
        {
            string Template = @"
/*BLOCK>>*/Block __Att__ /*INNER>>*/inner /*<<INNER*/EndBlock/*<<BLOCK*/
";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(@"<Timple><BLOCK Att=""1""/><BLOCK Att=""2""><INNER/></BLOCK></Timple>");

            XmlElement Parameter = Doc.DocumentElement;

            string Result = new TimpleEngine(Template, Parameter).Process();

            Assert.AreEqual(@"
Block 1 EndBlockBlock 2 inner EndBlock
", Result);
        }

        [Test]
        public void Template_shall_be_converted_to_correct_schema()
        {
            string Template = @"
__RootAttribute__
/*BLOCK>>*/
    __BlockAttribute__
    /*INNER>>*/
        __InnerAttribute__
        /*Inner>>*/
            __InnerInnerAttribute__
        /*<<Inner*/
    /*<<INNER*/
    /*INNER>>*/
        __InnerAttribute2__
        /*Inner>>*/
            __InnerInnerAttribute2__
        /*<<Inner*/
        /*Inner2>>*/
            __InnerInnerAttribute__
        /*<<Inner2*/
    /*<<INNER*/
    __BlockAttribute__
    __BlockAttribute2__
/*<<BLOCK*/
";
            string Result = TimpleEngine.GenerateXsd(Template,"TestTemplate","urn:Test");

            Assert.AreEqual(@"<xsd:schema elementFormDefault=""qualified"" targetNamespace=""urn:Test"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""TestTemplate""><xsd:complexType><xsd:choice minOccurs=""0"" maxOccurs=""unbounded""><xsd:element name=""BLOCK""><xsd:complexType><xsd:choice minOccurs=""0"" maxOccurs=""unbounded""><xsd:element name=""INNER""><xsd:complexType><xsd:choice minOccurs=""0"" maxOccurs=""unbounded""><xsd:element name=""Inner""><xsd:complexType><xsd:attribute name=""InnerInnerAttribute"" type=""xsd:string"" use=""optional"" /><xsd:attribute name=""InnerInnerAttribute2"" type=""xsd:string"" use=""optional"" /></xsd:complexType></xsd:element><xsd:element name=""Inner2""><xsd:complexType><xsd:attribute name=""InnerInnerAttribute"" type=""xsd:string"" use=""optional"" /></xsd:complexType></xsd:element></xsd:choice><xsd:attribute name=""InnerAttribute"" type=""xsd:string"" use=""optional"" /><xsd:attribute name=""InnerAttribute2"" type=""xsd:string"" use=""optional"" /></xsd:complexType></xsd:element></xsd:choice><xsd:attribute name=""BlockAttribute"" type=""xsd:string"" use=""optional"" /><xsd:attribute name=""BlockAttribute2"" type=""xsd:string"" use=""optional"" /></xsd:complexType></xsd:element></xsd:choice><xsd:attribute name=""RootAttribute"" type=""xsd:string"" use=""optional"" /></xsd:complexType></xsd:element></xsd:schema>", Result);
        }
    }
}