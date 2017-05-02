using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    internal class SingletonComponentInstance : ComponentInstance
    {
         private static readonly Dictionary<Type, SingletonInstanceWrapper> singletonInstances = new Dictionary<Type, SingletonInstanceWrapper>();

         internal SingletonComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        private SingletonInstanceWrapper InstanceReference
        {
            get
            {
                if (SingletonComponentInstance.singletonInstances.ContainsKey(this.Descriptor.Type))
                {
                    return SingletonComponentInstance.singletonInstances[this.Descriptor.Type];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    SingletonComponentInstance.singletonInstances.Add(this.Descriptor.Type, value);
                }
                else
                {
                    SingletonComponentInstance.singletonInstances.Remove(this.Descriptor.Type);
                }
            }
        }

        internal override object CreateInstance(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback)
        {
            if (this.InstanceReference == null)
            {
                this.InstanceReference = new SingletonInstanceWrapper(this.DoCreateInstance(interfaceType, componentContainer, progressCallback));
            }
            return this.InstanceReference.AddReference(componentContainer);
        }

        internal override void Dispose(ComponentContainer componentContainer)
        {
            if (this.InstanceReference != null &&
                this.InstanceReference.ReleaseReference(componentContainer))
            {
                (this.InstanceReference.Target as IDisposable)?.Dispose();
                this.InstanceReference = null;
                base.Dispose(componentContainer);
            }
        }

        #region Nested type: SharedInstanceWrapper

        private class SingletonInstanceWrapper
        {
            private readonly List<ComponentContainer> references = new List<ComponentContainer>();

            public SingletonInstanceWrapper(object instance)
            {
                this.Target = instance;
            }

            public object Target { get; }

            [UsedImplicitly]
            public object AddReference(ComponentContainer componentContainer)
            {
                if (this.references.Contains(componentContainer) == false)
                {
                    this.references.Add(componentContainer);
                }
                return this.Target;
            }

            public bool ReleaseReference(ComponentContainer componentContainer)
            {
                DbC.Assure(this.references.Contains(componentContainer));
                this.references.Remove(componentContainer);
                return this.references.Count == 0;
            }
        }

        #endregion
    }
}