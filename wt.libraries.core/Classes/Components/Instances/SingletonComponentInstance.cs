using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    internal class SingletonComponentInstance : ComponentInstance
    {
        private static readonly Dictionary<Type, SingletonInstanceWrapper> singletonInstances =
            new Dictionary<Type, SingletonInstanceWrapper>();

        internal SingletonComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        private SingletonInstanceWrapper InstanceReference
        {
            get
            {
                if (singletonInstances.ContainsKey(Descriptor.Type))
                    return singletonInstances[Descriptor.Type];
                return null;
            }
            set
            {
                if (value != null)
                    singletonInstances.Add(Descriptor.Type, value);
                else
                    singletonInstances.Remove(Descriptor.Type);
            }
        }

        internal override object CreateInstance(Type interfaceType, ComponentContainer componentContainer,
            Action<string> progressCallback)
        {
            if (InstanceReference == null)
                InstanceReference =
                    new SingletonInstanceWrapper(DoCreateInstance(interfaceType, componentContainer, progressCallback));
            return InstanceReference.AddReference(componentContainer);
        }

        internal override void Dispose(ComponentContainer componentContainer)
        {
            if (InstanceReference != null &&
                InstanceReference.ReleaseReference(componentContainer))
            {
                (InstanceReference.Target as IDisposable)?.Dispose();
                InstanceReference = null;
                Debug.WriteLine($"Disposing singleton component instance {Name}");
                base.Dispose(componentContainer);
            }
        }

        #region Nested type: SharedInstanceWrapper

        private class SingletonInstanceWrapper
        {
            private readonly List<ComponentContainer> references = new List<ComponentContainer>();

            public SingletonInstanceWrapper(object instance)
            {
                Target = instance;
            }

            public object Target { get; }

            [UsedImplicitly]
            public object AddReference(ComponentContainer componentContainer)
            {
                if (references.Contains(componentContainer) == false) references.Add(componentContainer);
                return Target;
            }

            public bool ReleaseReference(ComponentContainer componentContainer)
            {
                DbC.Assure(references.Contains(componentContainer));
                references.Remove(componentContainer);
                return references.Count == 0;
            }
        }

        #endregion
    }
}