// ReSharper disable MemberCanBePrivate.Global
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WhileTrue.Classes.CodeInspection;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// Implements a repository of component implementations. The components can be used in conjunction with a <see cref="ComponentContainer"/>
    /// which uses this repository as the source for implementations
    /// </summary>
    public class ComponentRepository
    {
        private readonly ComponentRepository parentRepository;
        private readonly Func<Type, bool> getMustRunOnUiThreadFunc;
        private readonly Collection<ComponentDescriptor> componentDescriptors = new Collection<ComponentDescriptor>();

        /// <summary/>
        public ComponentRepository(Func<Type,bool> getMustRunOnUiThreadFunc=null, Func<Func<Task<object>>,Task<object>> runOnUiThreadFunc=null)
            :this(null, getMustRunOnUiThreadFunc, runOnUiThreadFunc)
        {
        }

        /// <summary>
        /// The parent repository is used when no matching component is found in the current repository
        /// </summary>
        public ComponentRepository(ComponentRepository parentRepository, Func<Type, bool> getMustRunOnUiThreadFunc = null, Func<Func<Task<object>>, Task<object>> runOnUiThreadFunc=null)
        {
            this.parentRepository = parentRepository;
            this.getMustRunOnUiThreadFunc = getMustRunOnUiThreadFunc;
            this.RunOnUiThread = runOnUiThreadFunc??(async _=> await _());
        }
        

        #region AddComponent

        /// <summary>
        /// Add component with <see cref="ComponentInstanceScope.Repository"/> scope
        /// </summary>
        public void AddComponent<TComponentType>() where TComponentType:class
        {
            this.AddComponent<TComponentType>(null, null, ComponentInstanceScope.Repository);
        }

        /// <summary>
        /// Add component with the given scope
        /// </summary>
        public void AddComponent<TComponentType>(ComponentInstanceScope scope) where TComponentType : class
        {
            this.AddComponent<TComponentType>(null, null, scope);
        }

        /// <summary>
        /// Add component with <see cref="ComponentInstanceScope.Repository"/> scope and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration) where TComponentType : class
        {
            this.AddComponent <TComponentType>(configuration, null, ComponentInstanceScope.Repository);
        }

        /// <summary>
        /// Add component with the given scope and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration, ComponentInstanceScope scope) where TComponentType : class
        {
            this.AddComponent<TComponentType>(configuration, null, scope);
        }

        /// <summary>
        /// Add component with <see cref="ComponentInstanceScope.Repository"/> scope using a private
        /// repository which can be used by the component istance
        /// </summary>
        public void AddComponent<TComponentType>(ComponentRepository privateRepository) where TComponentType : class
        {
            this.AddComponent<TComponentType>(null, privateRepository, ComponentInstanceScope.Repository);
        }

        /// <summary>
        /// Add component with the given scope using a private
        /// repository which can be used by the component istance
        /// </summary>
        public void AddComponent<TComponentType>(ComponentRepository privateRepository, ComponentInstanceScope scope) where TComponentType : class
        {
            this.AddComponent<TComponentType>(null, privateRepository, scope);
        }

        /// <summary>
        /// Add component with <see cref="ComponentInstanceScope.Repository"/> scope using a private
        /// repository which can be used by the component instance and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration, ComponentRepository privateRepository) where TComponentType : class
        {
            this.AddComponent<TComponentType>(configuration, privateRepository, ComponentInstanceScope.Repository);
        }

        /// <summary>
        /// Add component with the given scope using a private
        /// repository which can be used by the component instance and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration, ComponentRepository privateRepository, ComponentInstanceScope scope) where TComponentType : class
        {
            switch (scope)
            {
                case ComponentInstanceScope.Container:
                    this.AddComponent(new SimpleComponentDescriptor(this, typeof(TComponentType), configuration, privateRepository));
                    break;
                case ComponentInstanceScope.Repository:
                    this.AddComponent(new SharedComponentDescriptor(this, typeof(TComponentType), configuration, privateRepository));
                    break;
                case ComponentInstanceScope.Global:
                    this.AddComponent(new SingletonComponentDescriptor(this, typeof(TComponentType), configuration, privateRepository));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope));
            }
        }

        #endregion

        
        #region Resolvers
        #endregion


        private void AddComponent(ComponentDescriptor descriptor)
        {
            DbC.Assure((from ComponentDescriptor ComponentDescriptor in this.componentDescriptors
                        where ComponentDescriptor.Type == descriptor.Type
                        select ComponentDescriptor).Any() == false, "Type already registered as a component: {0}", descriptor.Name);

            this.componentDescriptors.Add(descriptor);
        }


        internal ComponentDescriptor[] GetComponentDescriptors(Type interfaceType) 
        {
            ComponentDescriptor[] ComponentDescriptors = (
                                                             from ComponentDescriptor ComponentDescriptor in this.componentDescriptors
                                                             where ComponentDescriptor.ProvidesInterface(interfaceType)
                                                             select ComponentDescriptor
                                                         ).ToArray();

            // Try to resolve in this repository
            if (ComponentDescriptors.Length > 0)
            {
                return ComponentDescriptors;
            }
            // if nothing is found, resolve from parent repository, if given
            else if(this.parentRepository != null )
            {
                return this.parentRepository.GetComponentDescriptors(interfaceType);
            }
            // else return an empty list
            else
            {
                return new ComponentDescriptor[0];
            }
        }

        internal static bool IsComponentInterface(Type interfaceType)
        {
            return interfaceType.GetCustomAttributes<ComponentInterfaceAttribute>().Any();
        }

        internal Func<Func<Task<object>>, Task<object>> RunOnUiThread { get; }


        /// <summary>
        /// Returns the components and interface dependencies as an Eclipe UML2 model
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void CopyAsUml2ModelToStream(Stream output)
        {
            string XmiNs = "http://www.omg.org/spec/XMI/20131001";
            string UmlNs = "http://www.eclipse.org/uml2/5.0.0/UML";
            using (XmlWriter Writer = XmlWriter.Create(output, new XmlWriterSettings {Indent = true, Encoding = Encoding.UTF8}))
            {
                IEnumerable<Type> AllInterfaces = this.componentDescriptors.SelectMany(_ => _.GetProvidedInterfaces());

                Writer.WriteStartDocument();
                Writer.WriteStartElement("uml","Model", UmlNs);
                Writer.WriteAttributeString("xmi","version", XmiNs , "20131001");
                Writer.WriteAttributeString("xmi","id", XmiNs, "root");
                Writer.WriteAttributeString("name", "Root");
                foreach( IGrouping<string, ComponentDescriptor> GroupedComponents in this.componentDescriptors.GroupBy(_=>_.Type.Namespace) )
                {
                    Writer.WriteStartElement("packagedElement");
                    Writer.WriteAttributeString("xmi", "type", XmiNs, "uml:Package");
                    Writer.WriteAttributeString("xmi", "id", XmiNs, Encoding.UTF8.GetBytes(GroupedComponents.Key).ToHexString());
                    Writer.WriteAttributeString("name", GroupedComponents.Key.Substring(GroupedComponents.Key.LastIndexOf('.')+1));
                    foreach (ComponentDescriptor Component in GroupedComponents)
                    {
                        Writer.WriteStartElement("packagedElement");
                        Writer.WriteAttributeString("xmi", "type", XmiNs, "uml:Component");
                        Writer.WriteAttributeString("xmi", "id", XmiNs, Encoding.UTF8.GetBytes(Component.Type.FullName).ToHexString());
                        Writer.WriteAttributeString("name", Component.Name);
                        foreach (Type ProvidedInterface in Component.GetProvidedInterfaces())
                        {
                            Writer.WriteStartElement("interfaceRealization");
                            Writer.WriteAttributeString("xmi", "type", XmiNs, "uml:InterfaceRealization");
                            Writer.WriteAttributeString("xmi", "id", XmiNs, Encoding.UTF8.GetBytes($"{Component.Type.FullName}-provides-{ProvidedInterface.FullName}").ToHexString());
                            Writer.WriteAttributeString("client", Encoding.UTF8.GetBytes(Component.Type.FullName).ToHexString());
                            Writer.WriteAttributeString("supplier", Encoding.UTF8.GetBytes(ProvidedInterface.FullName).ToHexString());
                            Writer.WriteAttributeString("contract", Encoding.UTF8.GetBytes(ProvidedInterface.FullName).ToHexString());
                            Writer.WriteEndElement();
                        }
                        foreach (Type RequiredInterface in Component.GetRequiredInterfaces().Where(_=>AllInterfaces.Contains(_)))
                        {
                            Writer.WriteStartElement("packagedElement");
                            Writer.WriteAttributeString("xmi", "type", XmiNs, "uml:Dependency");
                            Writer.WriteAttributeString("xmi", "id", XmiNs, Encoding.UTF8.GetBytes($"{Component.Type.FullName}-requires-{RequiredInterface.FullName}").ToHexString());
                            Writer.WriteAttributeString("client", Encoding.UTF8.GetBytes(Component.Type.FullName).ToHexString());
                            Writer.WriteAttributeString("supplier", Encoding.UTF8.GetBytes(RequiredInterface.FullName).ToHexString());
                            Writer.WriteEndElement();
                        }
                        Writer.WriteEndElement();
                    }
                    Writer.WriteEndElement();
                }
                foreach (Type Interface in AllInterfaces)
                {
                    Writer.WriteStartElement("packagedElement");
                    Writer.WriteAttributeString("xmi", "type", XmiNs, "uml:Interface");
                    Writer.WriteAttributeString("xmi", "id", XmiNs, Encoding.UTF8.GetBytes(Interface.FullName).ToHexString());
                    Writer.WriteAttributeString("name", Interface.Name);
                    Writer.WriteEndElement();
                }
                Writer.WriteEndElement();
                Writer.WriteEndDocument();
            }
        }

        internal bool GetMustCreateOnUiThread(Type type)
        {
            return this.getMustRunOnUiThreadFunc?.Invoke(type) ?? false;
        }
    }
}