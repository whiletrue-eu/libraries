using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    public abstract class ComponentInstance
    {
        private readonly ComponentDescriptor componentDescriptor;
        private bool disposed;

        internal ComponentInstance(ComponentDescriptor componentDescriptor)
        {
            this.componentDescriptor = componentDescriptor;
        }

        public string Name
        {
            get { return this.componentDescriptor.Name; }
        }

        public ComponentDescriptor Descriptor
        {
            get { return this.componentDescriptor; }
        }

        protected abstract object Instance { get; }

        #region IDisposable Members

        public virtual void Dispose(ComponentContainer componentContainer)
        {
            this.CheckDisposed();
            this.disposed = true;
        }

        #endregion

        private Expression CreateWithOptimalConstructor(ComponentContainer componentContainer, Expression progressCallback)
        {
            ConstructorInfo OptimalConstructor = null;
            List<string> DiagnosisInformation = new List<string>();

            foreach (ConstructorInfo Constructor in this.componentDescriptor.Type.GetConstructors())
            {
                string ConstructorDiagnosisInformation;
                if (CanGetParametersFor(Constructor, componentContainer, out ConstructorDiagnosisInformation))
                {
                    if (OptimalConstructor == null ||
                        Constructor.GetParameters().Length > OptimalConstructor.GetParameters().Length)
                    {
                        OptimalConstructor = Constructor;
                    }
                }
                DiagnosisInformation.Add(string.Format("{0}: {1}",Constructor, ConstructorDiagnosisInformation));
            }

            if (OptimalConstructor != null)
            {
                IEnumerable<Expression> ConstructorParameters = this.GetParametersFor(OptimalConstructor, componentContainer, progressCallback);
                return Expression.Block(
                    Expression.Invoke(progressCallback,Expression.Constant(this.componentDescriptor.Name)),
                    Expression.New(OptimalConstructor,ConstructorParameters)
                    );
            }
            else
            {
                string Message = string.Format("No valid constructor could be found for component {0}. Make sure, that all component interfaces are marked with the ComponentInterface attribute\n\nDetailed information for the different constructors:\n{1}", 
                    this.componentDescriptor.Name,
                    string.Join("\n", DiagnosisInformation.ToArray()));
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
                if (Parameter.ParameterType.IsInterface)
                {
                    Expression Component = componentContainer.InternalResolveInstance(Parameter.ParameterType, true, null, progressCallback);
                    Component.DbC_AssureNotNull(string.Format("Could not resolve component '{0}' even though resolver claimed that he can. Parameter '{1}' (type providing '{2}' implementation) could not be instanciated", this.componentDescriptor.Type.FullName, Parameter.Name,Parameter.ParameterType.FullName));
                    yield return Component;
                }
                else if (Parameter.ParameterType.IsArray && Parameter.ParameterType.GetElementType().IsInterface)
                {
                    IEnumerable<Expression> Components = componentContainer.InternalResolveInstances(Parameter.ParameterType.GetElementType(), null, progressCallback);
                    yield return Expression.NewArrayInit(Parameter.ParameterType.GetElementType(), Components);
                }
                else if (Parameter.ParameterType == typeof(ComponentRepository))
                {
                    if (this.componentDescriptor.PrivateRepository != null)
                    {
                        yield return Expression.Constant(this.componentDescriptor.PrivateRepository);
                    }
                    else
                    {
                        yield return Expression.Constant(this.componentDescriptor.Repository);
                    }
                }
                else if (Parameter.ParameterType == typeof(ComponentContainer))
                {
                    yield return Expression.Constant(componentContainer);
                }
                else if (Parameter.ParameterType.IsAssignableFrom(this.componentDescriptor.ConfigType))
                {
                    yield return Expression.Constant(this.componentDescriptor.Config);
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
                if (Parameter.ParameterType.IsInterface)
                {
                    if (ComponentRepository.IsComponentInterface(Parameter.ParameterType))
                    {
                        if (!componentContainer.CanResolveComponent(Parameter.ParameterType))
                        {
                            diagnosisInformation = string.Format("parameter '{0} {1}' cannot be resolved. Make sure there is a registered component providing this interface.", Parameter.ParameterType.Name, Parameter.Name);
                            return false;
                        }
                        else
                        {
                            //Parameter is OK
                        }
                    }
                    else
                    {
                        diagnosisInformation = string.Format("parameter '{0} {1}' interface type is not a component interface. Make sure that the interfaces to use are marked with the [ComponentInterface] attribute.", Parameter.ParameterType.Name, Parameter.Name);
                        return false;
                    }
                }
                else if (Parameter.ParameterType.IsArray && Parameter.ParameterType.GetElementType().IsInterface)
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
                else if (Parameter.ParameterType.IsAssignableFrom(this.componentDescriptor.ConfigType))
                {
                    //Parameter is OK
                }
                else
                {
                    diagnosisInformation = string.Format("parameter '{0} {1}' type not supported. Except component interfaces, only ComponentRepository, ComponentContainer and the 'Config' Type (if given) can be used.", Parameter.ParameterType.Name, Parameter.Name);
                    return false;
                }
            }

            diagnosisInformation = "OK";
            return true;
        }

        internal abstract Expression CreateInstance(Type interfaceType, ComponentContainer componentContainer, Expression progressCallback);

        protected Expression DoCreateInstance(Type interfaceType, ComponentContainer componentContainer, Expression progressCallback)
        {
            this.CheckDisposed();

            this.Descriptor.DbC_Assure(value => value.Type == interfaceType || value.ProvidesInterface(interfaceType),
                                       string.Format("Requested interface type '{0}' not supported by component '{1}'", interfaceType.FullName, this.componentDescriptor.Type.FullName));

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

            Instance.DbC_AssureNotNull(string.Format("Requested interface type '{0}' not supported by component '{1}'", interfaceType.FullName, this.componentDescriptor.Type.FullName));

            return Instance;
        }

        private object CastTo(object instance, Type interfaceType)
        {
            object Instance = this.TryCastTo(instance, interfaceType);

            Instance.DbC_AssureNotNull(string.Format("Requested interface type '{0}' not supported by component '{1}'", interfaceType.FullName, this.componentDescriptor.Type.FullName));

            return Instance;
        }

        private Expression TryCastTo(Expression instance, Type interfaceType)
        {
            return this.componentDescriptor.TryCastExpressionTo(instance, interfaceType);
        }  
        
        private object TryCastTo(object instance, Type interfaceType)
        {
            return this.componentDescriptor.TryCastTo(instance, interfaceType);
        }

        private void CheckDisposed()
        {
            this.disposed.DbC_Assure(value => value == false, new ObjectDisposedException(""));
        }

        internal void NotifyInstanceCreated(ComponentInstance componentInstance, object instance)
        {
            foreach (PropertyInfo Property in this.componentDescriptor.GetLazyInitializeProperties())
            {
                if (componentInstance.CanCastTo(instance, Property.PropertyType))
                {
                    this.LazyInitialize(Property, componentInstance.CastTo(instance, Property.PropertyType));
                }
            }
        }

        internal void LazyInitializeWithInstancesAlreadyExisting(ComponentContainer componentContainer)
        {
            foreach (PropertyInfo Property in this.componentDescriptor.GetLazyInitializeProperties())
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


        protected abstract void LazyInitialize(PropertyInfo property, object instance);
    }
}