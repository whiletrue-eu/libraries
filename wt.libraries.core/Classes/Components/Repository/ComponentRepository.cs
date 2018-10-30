// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    ///     Implements a repository of component implementations. The components can be used in conjunction with a
    ///     <see cref="ComponentContainer" />
    ///     which uses this repository as the source for implementations
    /// </summary>
    public class ComponentRepository
    {
        private readonly Collection<ComponentDescriptor> componentDescriptors = new Collection<ComponentDescriptor>();
        private readonly ComponentRepository parentRepository;

        /// <summary />
        public ComponentRepository()
            : this(null)
        {
        }

        /// <summary>
        ///     The parent repository is used when no matching component is found in the current repository
        /// </summary>
        public ComponentRepository(ComponentRepository parentRepository)
        {
            this.parentRepository = parentRepository;
        }


        private void AddComponent(ComponentDescriptor descriptor)
        {
            DbC.Assure((from ComponentDescriptor ComponentDescriptor in componentDescriptors
                           where ComponentDescriptor.Type == descriptor.Type
                           select ComponentDescriptor).Any() == false, "Type already registered as a component: {0}",
                descriptor.Name);

            componentDescriptors.Add(descriptor);
        }


        internal ComponentDescriptor[] GetComponentDescriptors(Type interfaceType)
        {
            var ComponentDescriptors = (
                from ComponentDescriptor ComponentDescriptor in componentDescriptors
                where ComponentDescriptor.ProvidesInterface(interfaceType)
                select ComponentDescriptor
            ).ToArray();

            // Try to resolve in this repository
            if (ComponentDescriptors.Length > 0)
                return ComponentDescriptors;
            // if nothing is found, resolve from parent repository, if given
            if (parentRepository != null)
                return parentRepository.GetComponentDescriptors(interfaceType);
            // else return an empty list
            return new ComponentDescriptor[0];
        }

        internal static bool IsComponentInterface(Type interfaceType)
        {
            return interfaceType.GetCustomAttributes<ComponentInterfaceAttribute>().Any();
        }

        /// <summary>
        ///     Returns the components and interface dependencies as an Eclipe UML2 model
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void CopyAsUml2ModelToStream(Stream output)
        {
            var XmiNs = "http://www.omg.org/spec/XMI/20131001";
            var UmlNs = "http://www.eclipse.org/uml2/5.0.0/UML";
            using (var Writer =
                XmlWriter.Create(output, new XmlWriterSettings {Indent = true, Encoding = Encoding.UTF8}))
            {
                var AllInterfaces = componentDescriptors.SelectMany(_ => _.GetProvidedInterfaces());

                Writer.WriteStartDocument();
                Writer.WriteStartElement("uml", "Model", UmlNs);
                Writer.WriteAttributeString("xmi", "version", XmiNs, "20131001");
                Writer.WriteAttributeString("xmi", "id", XmiNs, "root");
                Writer.WriteAttributeString("name", "Root");
                foreach (var GroupedComponents in componentDescriptors.GroupBy(
                    _ => _.Type.Namespace))
                {
                    Writer.WriteStartElement("packagedElement");
                    Writer.WriteAttributeString("xmi", "type", XmiNs, "uml:Package");
                    Writer.WriteAttributeString("xmi", "id", XmiNs,
                        Encoding.UTF8.GetBytes(GroupedComponents.Key).ToHexString());
                    Writer.WriteAttributeString("name",
                        GroupedComponents.Key.Substring(GroupedComponents.Key.LastIndexOf('.') + 1));
                    foreach (var Component in GroupedComponents)
                    {
                        Writer.WriteStartElement("packagedElement");
                        Writer.WriteAttributeString("xmi", "type", XmiNs, "uml:Component");
                        Writer.WriteAttributeString("xmi", "id", XmiNs,
                            Encoding.UTF8.GetBytes(Component.Type.FullName).ToHexString());
                        Writer.WriteAttributeString("name", Component.Name);
                        foreach (var ProvidedInterface in Component.GetProvidedInterfaces())
                        {
                            Writer.WriteStartElement("interfaceRealization");
                            Writer.WriteAttributeString("xmi", "type", XmiNs, "uml:InterfaceRealization");
                            Writer.WriteAttributeString("xmi", "id", XmiNs,
                                Encoding.UTF8
                                    .GetBytes($"{Component.Type.FullName}-provides-{ProvidedInterface.FullName}")
                                    .ToHexString());
                            Writer.WriteAttributeString("client",
                                Encoding.UTF8.GetBytes(Component.Type.FullName).ToHexString());
                            Writer.WriteAttributeString("supplier",
                                Encoding.UTF8.GetBytes(ProvidedInterface.FullName).ToHexString());
                            Writer.WriteAttributeString("contract",
                                Encoding.UTF8.GetBytes(ProvidedInterface.FullName).ToHexString());
                            Writer.WriteEndElement();
                        }

                        foreach (var RequiredInterface in Component.GetRequiredInterfaces()
                            .Where(_ => AllInterfaces.Contains(_)))
                        {
                            Writer.WriteStartElement("packagedElement");
                            Writer.WriteAttributeString("xmi", "type", XmiNs, "uml:Dependency");
                            Writer.WriteAttributeString("xmi", "id", XmiNs,
                                Encoding.UTF8
                                    .GetBytes($"{Component.Type.FullName}-requires-{RequiredInterface.FullName}")
                                    .ToHexString());
                            Writer.WriteAttributeString("client",
                                Encoding.UTF8.GetBytes(Component.Type.FullName).ToHexString());
                            Writer.WriteAttributeString("supplier",
                                Encoding.UTF8.GetBytes(RequiredInterface.FullName).ToHexString());
                            Writer.WriteEndElement();
                        }

                        Writer.WriteEndElement();
                    }

                    Writer.WriteEndElement();
                }

                foreach (var Interface in AllInterfaces)
                {
                    Writer.WriteStartElement("packagedElement");
                    Writer.WriteAttributeString("xmi", "type", XmiNs, "uml:Interface");
                    Writer.WriteAttributeString("xmi", "id", XmiNs,
                        Encoding.UTF8.GetBytes(Interface.FullName).ToHexString());
                    Writer.WriteAttributeString("name", Interface.Name);
                    Writer.WriteEndElement();
                }

                Writer.WriteEndElement();
                Writer.WriteEndDocument();
            }
        }


        #region AddComponent

        /// <summary>
        ///     Add component with <see cref="ComponentInstanceScope.Repository" /> scope
        /// </summary>
        public void AddComponent<TComponentType>() where TComponentType : class
        {
            AddComponent<TComponentType>(null, null, ComponentInstanceScope.Repository);
        }

        /// <summary>
        ///     Add component with the given scope
        /// </summary>
        public void AddComponent<TComponentType>(ComponentInstanceScope scope) where TComponentType : class
        {
            AddComponent<TComponentType>(null, null, scope);
        }

        /// <summary>
        ///     Add component with <see cref="ComponentInstanceScope.Repository" /> scope and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration) where TComponentType : class
        {
            AddComponent<TComponentType>(configuration, null, ComponentInstanceScope.Repository);
        }

        /// <summary>
        ///     Add component with the given scope and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration, ComponentInstanceScope scope)
            where TComponentType : class
        {
            AddComponent<TComponentType>(configuration, null, scope);
        }

        /// <summary>
        ///     Add component with <see cref="ComponentInstanceScope.Repository" /> scope using a private
        ///     repository which can be used by the component istance
        /// </summary>
        public void AddComponent<TComponentType>(ComponentRepository privateRepository) where TComponentType : class
        {
            AddComponent<TComponentType>(null, privateRepository, ComponentInstanceScope.Repository);
        }

        /// <summary>
        ///     Add component with the given scope using a private
        ///     repository which can be used by the component istance
        /// </summary>
        public void AddComponent<TComponentType>(ComponentRepository privateRepository, ComponentInstanceScope scope)
            where TComponentType : class
        {
            AddComponent<TComponentType>(null, privateRepository, scope);
        }

        /// <summary>
        ///     Add component with <see cref="ComponentInstanceScope.Repository" /> scope using a private
        ///     repository which can be used by the component instance and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration, ComponentRepository privateRepository)
            where TComponentType : class
        {
            AddComponent<TComponentType>(configuration, privateRepository, ComponentInstanceScope.Repository);
        }

        /// <summary>
        ///     Add component with the given scope using a private
        ///     repository which can be used by the component instance and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration, ComponentRepository privateRepository,
            ComponentInstanceScope scope) where TComponentType : class
        {
            switch (scope)
            {
                case ComponentInstanceScope.Container:
                    AddComponent(new SimpleComponentDescriptor(this, typeof(TComponentType), configuration,
                        privateRepository));
                    break;
                case ComponentInstanceScope.Repository:
                    AddComponent(new SharedComponentDescriptor(this, typeof(TComponentType), configuration,
                        privateRepository));
                    break;
                case ComponentInstanceScope.Global:
                    AddComponent(new SingletonComponentDescriptor(this, typeof(TComponentType), configuration,
                        privateRepository));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope));
            }
        }

        #endregion


        #region Resolvers

        #endregion
    }
}