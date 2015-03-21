using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Xml;

namespace WhileTrue.Classes.Utilities
{
    ///<summary>
    /// Extends the <see cref="XmlUrlResolver"/> by adding support for OPC file content (Pack URIs),
    /// application resources (project file type 'Resource') and dynamic content
    ///</summary>
    /// <remarks>
    /// <para>
    /// For application reosurces, the protocol 'resource://' must be used, followed by the host name
    /// 'Application', followed by the resource name. Example: 'resource://Application/Subdir/Resource.ext'
    /// </para>
    /// <para>
    /// For dynamic content, the protocol 'dynamic://' must be used, followed by the identifier of the
    /// content (see <see cref="AddDynamicContent"/> method) as the host name
    /// </para>
    /// </remarks>
    public class XmlUrlResolverEx : XmlUrlResolver
    {
        private readonly Dictionary<string,string> dynamicContent = new Dictionary<string, string>();

        ///<summary>
        /// <see cref="XmlUrlResolver.GetEntity"/>
        ///</summary>
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            ofObjectToReturn.DbC_Assure(value=>value == typeof (Stream));

            switch(absoluteUri.Scheme)
            {
                case "resource":
                    return Resources.GetStream(absoluteUri.AbsolutePath);
                case "dynamic":
                    string ResourceName = absoluteUri.Host;
                    if( this.dynamicContent.ContainsKey(ResourceName) )
                    {
                        return new MemoryStream(Encoding.Unicode.GetBytes(this.dynamicContent[ResourceName]));
                    }
                    else
                    {
                        throw new ArgumentException($"Dynamic content not found: {ResourceName}");
                    }
                case "pack":
                    return Package
                        .Open((Stream) this.GetEntity(PackUriHelper.GetPackageUri(absoluteUri), null, typeof (Stream)))
                        .GetPart(PackUriHelper.GetPartUri(absoluteUri)).GetStream();
                default:
                    return base.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
        }

        ///<summary>
        /// Adds dynamic content with the given id. IDs are case insensitive!
        ///</summary>
        public void AddDynamicContent(string id, string content)
        {
            this.dynamicContent.Add(id.ToLower(), content);
        }
    }
}