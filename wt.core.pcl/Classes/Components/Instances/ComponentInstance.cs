 using System;
using System.Collections.Generic;
 using System.Diagnostics;
 using System.Linq;
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
        private static int debugIndent;

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

        #region IDisposable Members

        internal virtual void Dispose(ComponentContainer componentContainer)
        {
            this.CheckDisposed();
            this.disposed = true;
        }

        #endregion

        private object CreateWithOptimalConstructor(ComponentContainer componentContainer, Action<string> progressCallback)
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
                DateTime Start = DateTime.Now;
                Debug.WriteLine($"{new string(' ',ComponentInstance.debugIndent*3)}Start instantiation of {this.Name}");
                ComponentInstance.debugIndent++;
                try
                {
                    object[] ConstructorParameters = this.GetParametersFor(OptimalConstructor, componentContainer, progressCallback).ToArray();
                    DateTime ParameterResolved = DateTime.Now;
                    progressCallback?.Invoke(this.Descriptor.Name);
                    object Component = OptimalConstructor.Invoke(ConstructorParameters);
                    DateTime ComponentCreated = DateTime.Now;
                    ComponentInstance.debugIndent--;
                    Debug.WriteLine($"{new string(' ', ComponentInstance.debugIndent * 3)}Instantiation of {this.Name} took {ComponentInstance.Format(ComponentCreated - Start)} (params: {ComponentInstance.Format(ParameterResolved - Start)}, ctor: {ComponentInstance.Format(ComponentCreated - ParameterResolved)})");
                    return Component;
                }
                catch (Exception Error)
                {
                    ComponentInstance.debugIndent--;
                    Debug.WriteLine($"{new string(' ', ComponentInstance.debugIndent * 3)}Instantiation of {this.Name} failed with message: {Error.Message.Replace("\n",$"\n{new string(' ', ComponentInstance.debugIndent * 3)}")}");
                    throw;
                }
            }
            else
            {
                string Message = $"No valid constructor could be found for component {this.Descriptor.Name}. Make sure, that all component interfaces are marked with the ComponentInterface attribute\n\nDetailed information for the different constructors:\n{string.Join("\n", DiagnosisInformation.ToArray())}";
                throw new ResolveComponentException(Message);
            }
        }

        private static string Format(TimeSpan span) => $"{(int)span.TotalSeconds}.{span:ffff}";

        /// <summary>
        /// Gets the parameter list to call the given constructor. If a parameter could not be
        /// resolved, null is returned
        /// </summary>
        private IEnumerable<object> GetParametersFor(ConstructorInfo constructor, ComponentContainer componentContainer, Action<string> progressCallback)
        {
            foreach (ParameterInfo Parameter in constructor.GetParameters())
            {
                Type ParameterType = Parameter.ParameterType;
                if (ComponentInstance.IsValidInterfaceReference(ParameterType))
                {
                    object Component = componentContainer.InternalResolveInstance(ParameterType, true, progressCallback);
                    Component.DbC_AssureNotNull($"Could not resolve component '{this.Descriptor.Type.FullName}' even though resolver claimed that he can. Parameter '{Parameter.Name}' (type providing '{ParameterType.FullName}' implementation) could not be instanciated");
                    yield return Component;
                }
                else if (ComponentInstance.IsValidFuncToInterfaceReference(ParameterType))
                {
                    LambdaExpression Lambda = Expression.Lambda(
                        ParameterType,
                        Expression.Convert(
                            ((Expression<Func<object>>)(() => componentContainer.InternalResolveInstance(ParameterType.GenericTypeArguments[0], true, progressCallback))).Body,
                             ParameterType.GenericTypeArguments[0]));

                    yield return Lambda.Compile();
                }
                else if (ComponentInstance.IsValidInterfaceArrayReference(ParameterType))
                {
                    yield return componentContainer.InternalResolveInstancesAsArray(ParameterType.GetElementType(), progressCallback);
                }
                else if (ComponentInstance.IsValidFuncToInterfaceArrayReference(ParameterType))
                {
                    LambdaExpression Lambda = Expression.Lambda(
                        ParameterType,
                        Expression.Convert(
                            ((Expression<Func<Array>>) (() => componentContainer.InternalResolveInstancesAsArray(ParameterType.GenericTypeArguments[0].GetElementType(), progressCallback))).Body,
                            ParameterType.GenericTypeArguments[0]));

                    yield return Lambda.Compile();
                }
                else if (ParameterType == typeof(ComponentRepository))
                {
                    if (this.Descriptor.PrivateRepository != null)
                    {
                        yield return this.Descriptor.PrivateRepository;
                    }
                    else
                    {
                        yield return this.Descriptor.Repository;
                    }
                }
                else if (ParameterType == typeof(ComponentContainer))
                {
                    yield return componentContainer;
                }
                else if (ParameterType.IsAssignableFrom(this.Descriptor.ConfigType))
                {
                    yield return this.Descriptor.Config;
                }
                else
                {
                    throw new InvalidOperationException("Internal error: tried to resolve constructor parameters even though the constructor wasn't adequate");
                }
            }
        }

        private static bool IsValidInterfaceArrayReference(Type parameterType)
        {
            return parameterType.IsArray && parameterType.GetElementType().IsInterface();
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanGetParametersFor(ConstructorInfo constructor, ComponentContainer componentContainer, out string diagnosisInformation)
        {
            foreach (ParameterInfo Parameter in constructor.GetParameters())
            {
                Type ParameterType = Parameter.ParameterType;

                if (ComponentInstance.IsValidInterfaceReference(ParameterType))
                {
                    //Simple interface reference
                    if (CheckInterfaceParameter(componentContainer, ParameterType, true, $"{ParameterType.Name} {Parameter.Name}", "a", out diagnosisInformation) == false)
                    {
                        return false;
                    }
                }
                else if (ComponentInstance.IsValidFuncToInterfaceReference(ParameterType))
                {
                    //Func<> Interface reference
                    if( CheckInterfaceParameter(componentContainer, ParameterType.GenericTypeArguments[0], true, $"{ParameterType.Name} {Parameter.Name}", "Func<> returning a", out diagnosisInformation) == false)
                    {
                        return false;
                    }
                }
                else if (ComponentInstance.IsValidInterfaceArrayReference(ParameterType))
                {
                    //Array of interface
                    if( CheckInterfaceParameter(componentContainer, ParameterType.GetElementType(), false, $"{ParameterType.Name} {Parameter.Name}", "an array of a", out diagnosisInformation) == false)
                    {
                        return false;
                    }
                }
                else if (ComponentInstance.IsValidFuncToInterfaceArrayReference(ParameterType))
                {
                    //Func<> of Array of interface
                    if( CheckInterfaceParameter(componentContainer, ParameterType.GenericTypeArguments[0].GetElementType(), false, $"{ParameterType.Name} {Parameter.Name}", "a Func<> returning an array of a", out diagnosisInformation) == false)
                    {
                        return false;
                    }
                }
                else if (ParameterType == typeof (ComponentRepository))
                {
                    //Parameter is OK
                }
                else if (ParameterType == typeof(ComponentContainer))
                {
                    //Parameter is OK
                }
                else if (this.Descriptor.ConfigType != null && ParameterType.IsAssignableFrom(this.Descriptor.ConfigType))
                {
                    //Parameter is OK
                }
                else
                {
                    diagnosisInformation = $"parameter '{ParameterType.Name} {Parameter.Name}' type not supported. Except component interfaces, arrays of component interfaces, Func<> returning component interfaces or arrays of them, only ComponentRepository, ComponentContainer and the 'Config' Type using in component registration (if given) can be used.";
                    return false;
                }
            }

            diagnosisInformation = "OK";
            return true;
        }

        private static bool IsValidFuncToInterfaceArrayReference(Type parameterType)
        {
            return parameterType.IsConstructedGenericType && parameterType.GetGenericTypeDefinition() == typeof(Func<>) && parameterType.GenericTypeArguments[0].IsArray && parameterType.GenericTypeArguments[0].GetElementType().IsInterface();
        }

        private static bool IsValidFuncToInterfaceReference(Type parameterType)
        {
            return parameterType.IsConstructedGenericType && parameterType.GetGenericTypeDefinition() == typeof(Func<>) && parameterType.GenericTypeArguments[0].IsInterface();
        }

        private static bool IsValidInterfaceReference(Type parameterType)
        {
            return parameterType.IsInterface();
        }

        private bool CheckInterfaceParameter(ComponentContainer componentContainer, Type parameterType, bool mustResolve, string parameterTypeAndName, string specialTypeName, out string diagnosisInformation)
        {
            if (ComponentRepository.IsComponentInterface(parameterType))
            {
                if (mustResolve == false||componentContainer.CanResolveComponent(parameterType))
                {
                    //Parameter is OK
                }
                else
                {
                    diagnosisInformation = $"parameter '{parameterTypeAndName}' cannot be resolved. Make sure there is a registered component providing this interface.";
                    return false;
                }
            }
            else
            {
                diagnosisInformation = $"parameter '{parameterTypeAndName}' is not {specialTypeName} component interface. Make sure that the interfaces to use are marked with the [ComponentInterface] attribute.";
                return false;
            }
            diagnosisInformation = "OK";
            return true;
        }

        internal abstract object CreateInstance(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback);

        internal object DoCreateInstance(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback)
        {
            this.CheckDisposed();

            this.Descriptor.DbC_Assure(value => value.Type == interfaceType || value.ProvidesInterface(interfaceType),
                $"Requested interface type '{interfaceType.FullName}' not supported by component '{this.Descriptor.Type.FullName}'");

            return this.CastTo(this.CreateWithOptimalConstructor(componentContainer, progressCallback), interfaceType);
        }

        private object CastTo(object instance, Type interfaceType)
        {
            object Instance = this.TryCastTo(instance, interfaceType);

            Instance.DbC_AssureNotNull($"Requested interface type '{interfaceType.FullName}' not supported by component '{this.Descriptor.Type.FullName}'");

            return Instance;
        }
        
        private object TryCastTo(object instance, Type interfaceType)
        {
            return this.Descriptor.TryCastTo(instance, interfaceType);
        }

        private void CheckDisposed()
        {
            this.disposed.DbC_Assure(value => value == false, new ObjectDisposedException(""));
        }
    }
}