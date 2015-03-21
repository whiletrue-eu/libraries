 using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// Container for an instance of a specific component
    /// </summary>
    [PublicAPI]
    public abstract class ComponentInstance
    {
        private bool disposed;
         
        internal ComponentInstance(ComponentDescriptor componentDescriptor)
        {
            this.Descriptor = componentDescriptor;
        }

        /// <summary>
        /// Returns the name of the component as specified in the Component Attribute
        /// </summary>
        public string Name => this.Descriptor.Name;

        /// <summary>
        /// Retruns the component descriptor
        /// </summary>
        public ComponentDescriptor Descriptor { get; }

        /// <summary>
        /// Returns the wrapped instance
        /// </summary>
        protected abstract object Instance { get; }

        #region IDisposable Members

        internal virtual void Dispose(ComponentContainer componentContainer)
        {
            this.CheckDisposed();
            this.disposed = true;
        }

        #endregion

        private Expression CreateWithOptimalConstructor(ComponentContainer componentContainer, Expression progressCallback)
        {
            ConstructorInfo OptimalConstructor = null;
            List<string> DiagnosisInformation = new List<string>();

            foreach (ConstructorInfo Constructor in this.Descriptor.Type.GetConstructors())
            {
                string ConstructorDiagnosisInformation;
                if (this.CanGetParametersFor(Constructor, componentContainer, out ConstructorDiagnosisInformation))
                {
                    if (OptimalConstructor == null ||
                        Constructor.GetParameters().Length > OptimalConstructor.GetParameters().Length)
                    {
                        OptimalConstructor = Constructor;
                    }
                }
                DiagnosisInformation.Add($"{Constructor}: {ConstructorDiagnosisInformation}");
            }

            if (OptimalConstructor != null)
            {
                IEnumerable<Expression> ConstructorParameters = this.GetParametersFor(OptimalConstructor, componentContainer, progressCallback);
                return Expression.Block(
                    Expression.Invoke(progressCallback,Expression.Constant(this.Descriptor.Name)),
                    Expression.New(OptimalConstructor,ConstructorParameters)
                    );
            }
            else
            {
                string Message = $"No valid constructor could be found for component {this.Descriptor.Name}. Make sure, that all component interfaces are marked with the ComponentInterface attribute\n\nDetailed information for the different constructors:\n{string.Join("\n", DiagnosisInformation.ToArray())}";
                throw new ResolveComponentException(Message);
            }
        }

        /// <summary>
        /// Gets the parameter list to call the given constructor. If a parameter could not be
        /// resolved, null is returned
        /// </summary>
        private IEnumerable<Expression> GetParametersFor(ConstructorInfo constructor, ComponentContainer componentContainer, Expression progressCallback)
        {
            foreach (ParameterInfo Parameter in constructor.GetParameters())
            {
                if (Parameter.ParameterType.IsInterface())
                {
                    Expression Component = componentContainer.InternalResolveInstance(Parameter.ParameterType, true, progressCallback);
                    Component.DbC_AssureNotNull($"Could not resolve component '{this.Descriptor.Type.FullName}' even though resolver claimed that he can. Parameter '{Parameter.Name}' (type providing '{Parameter.ParameterType.FullName}' implementation) could not be instanciated");
                    yield return Component;
                }
                else if (Parameter.ParameterType.IsArray && Parameter.ParameterType.GetElementType().IsInterface())
                {
                    IEnumerable<Expression> Components = componentContainer.InternalResolveInstances(Parameter.ParameterType.GetElementType(), progressCallback);
                    yield return Expression.NewArrayInit(Parameter.ParameterType.GetElementType(), Components);
                }
                else if (Parameter.ParameterType == typeof(ComponentRepository))
                {
                    if (this.Descriptor.PrivateRepository != null)
                    {
                        yield return Expression.Constant(this.Descriptor.PrivateRepository);
                    }
                    else
                    {
                        yield return Expression.Constant(this.Descriptor.Repository);
                    }
                }
                else if (Parameter.ParameterType == typeof(ComponentContainer))
                {
                    yield return Expression.Constant(componentContainer);
                }
                else if (Parameter.ParameterType.IsAssignableFrom(this.Descriptor.ConfigType))
                {
                    yield return Expression.Constant(this.Descriptor.Config);
                }
                else
                {
                    throw new InvalidOperationException("Internal error: tried to resolve constructor parameters even though the constructor wasn't adequate");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanGetParametersFor(ConstructorInfo constructor, ComponentContainer componentContainer, out string diagnosisInformation)
        {
            foreach (ParameterInfo Parameter in constructor.GetParameters())
            {
                if (Parameter.ParameterType.IsInterface())
                {
                    if (ComponentRepository.IsComponentInterface(Parameter.ParameterType))
                    {
                        if (!componentContainer.CanResolveComponent(Parameter.ParameterType))
                        {
                            diagnosisInformation = $"parameter '{Parameter.ParameterType.Name} {Parameter.Name}' cannot be resolved. Make sure there is a registered component providing this interface.";
                            return false;
                        }
                        else
                        {
                            //Parameter is OK
                        }
                    }
                    else
                    {
                        diagnosisInformation = $"parameter '{Parameter.ParameterType.Name} {Parameter.Name}' interface type is not a component interface. Make sure that the interfaces to use are marked with the [ComponentInterface] attribute.";
                        return false;
                    }
                }
                else if (Parameter.ParameterType.IsArray && Parameter.ParameterType.GetElementType().IsInterface())
                {
                    //Array parameter can also be an empty array, so it is OK even if there is no providing compontent found
                }
                else if (Parameter.ParameterType == typeof (ComponentRepository))
                {
                    //Parameter is OK
                }
                else if (Parameter.ParameterType == typeof(ComponentContainer))
                {
                    //Parameter is OK
                }
                else if (this.Descriptor.ConfigType != null && Parameter.ParameterType.IsAssignableFrom(this.Descriptor.ConfigType))
                {
                    //Parameter is OK
                }
                else
                {
                    diagnosisInformation = $"parameter '{Parameter.ParameterType.Name} {Parameter.Name}' type not supported. Except component interfaces, only ComponentRepository, ComponentContainer and the 'Config' Type (if given) can be used.";
                    return false;
                }
            }

            diagnosisInformation = "OK";
            return true;
        }

        internal abstract Expression CreateInstance(Type interfaceType, ComponentContainer componentContainer, Expression progressCallback);

        internal Expression DoCreateInstance(Type interfaceType, ComponentContainer componentContainer, Expression progressCallback)
        {
            this.CheckDisposed();

            this.Descriptor.DbC_Assure(value => value.Type == interfaceType || value.ProvidesInterface(interfaceType),
                $"Requested interface type '{interfaceType.FullName}' not supported by component '{this.Descriptor.Type.FullName}'");

            Expression Instance = this.CreateWithOptimalConstructor(componentContainer, progressCallback);
            return this.CastTo(Instance, interfaceType);
        }

        private bool CanCastTo(object instance, Type interfaceType)
        {
            return this.TryCastTo(instance, interfaceType) != null;
        }


        private Expression CastTo(Expression instance, Type interfaceType)
        {
            Expression Instance = this.TryCastTo(instance, interfaceType);

            Instance.DbC_AssureNotNull($"Requested interface type '{interfaceType.FullName}' not supported by component '{this.Descriptor.Type.FullName}'");

            return Instance;
        }

        private object CastTo(object instance, Type interfaceType)
        {
            object Instance = this.TryCastTo(instance, interfaceType);

            Instance.DbC_AssureNotNull($"Requested interface type '{interfaceType.FullName}' not supported by component '{this.Descriptor.Type.FullName}'");

            return Instance;
        }

        private Expression TryCastTo(Expression instance, Type interfaceType)
        {
            return this.Descriptor.TryCastExpressionTo(instance, interfaceType);
        }  
        
        private object TryCastTo(object instance, Type interfaceType)
        {
            return this.Descriptor.TryCastTo(instance, interfaceType);
        }

        private void CheckDisposed()
        {
            this.disposed.DbC_Assure(value => value == false, new ObjectDisposedException(""));
        }

        internal void NotifyInstanceCreated(ComponentInstance componentInstance, object instance)
        {
            foreach (PropertyInfo Property in this.Descriptor.GetLazyInitializeProperties())
            {
                if (componentInstance.CanCastTo(instance, Property.PropertyType))
                {
                    this.LazyInitialize(Property, componentInstance.CastTo(instance, Property.PropertyType));
                }
            }
        }

        internal void LazyInitializeWithInstancesAlreadyExisting(ComponentContainer componentContainer)
        {
            foreach (PropertyInfo Property in this.Descriptor.GetLazyInitializeProperties())
            {
                foreach (ComponentInstance ComponentInstance in componentContainer.ComponentInstances)
                {
                    object Instance = ComponentInstance.Instance;
                    if (ComponentInstance.CanCastTo(Instance, Property.PropertyType))
                    {
                        this.LazyInitialize(Property, ComponentInstance.CastTo(Instance, Property.PropertyType));
                    }
                }
            }
        }


        internal abstract void LazyInitialize(PropertyInfo property, object instance);
    }
}