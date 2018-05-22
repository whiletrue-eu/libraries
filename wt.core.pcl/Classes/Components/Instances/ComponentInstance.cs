 using System;
using System.Collections.Generic;
 using System.Diagnostics;
 using System.Linq;
 using System.Linq.Expressions;
 using System.Reflection;
 using System.Threading.Tasks;
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

        private async Task<object> CreateWithOptimalConstructorAsync(ComponentContainer componentContainer, Action<string> progressCallback, ComponentDescriptor[] resolveStack)
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
                    object[] ConstructorParameters = await this.GetParametersForAsync(OptimalConstructor, componentContainer, progressCallback, resolveStack);
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
        private async Task<object[]> GetParametersForAsync(ConstructorInfo constructor, ComponentContainer componentContainer, Action<string> progressCallback, ComponentDescriptor[] resolveStack)
        {
            async Task<object> CreateInstanceAsync(Type parameterType, string parameterName)
            {
                object Instance = await componentContainer.InternalResolveInstanceAsync(parameterType, true, progressCallback,resolveStack);
                Instance.DbC_AssureNotNull($"Could not resolve component '{this.Descriptor.Type.FullName}' even though resolver claimed that he can. Parameter '{parameterName}' (type providing '{parameterType.FullName}' implementation) could not be instanciated");
                return Instance;
            }

            async Task<object> CreateInstanceArrayAsync(Type parameterType)
            {
                Array Instances = await componentContainer.InternalResolveInstancesAsArrayAsync(parameterType, progressCallback,resolveStack);
                return Instances;
            }

            List<Task<object>> Parameters = new List<Task<object>>();
            foreach (ParameterInfo Parameter in constructor.GetParameters())
            {
                Type ParameterType = Parameter.ParameterType;
                if (ComponentInstance.IsValidInterfaceReference(ParameterType))
                {
                    Task<object> Component = CreateInstanceAsync(ParameterType, Parameter.Name);
                    Parameters.Add(Component);
                }
                else if (ComponentInstance.IsValidInterfaceTaskReference(ParameterType))
                {
                    object Component = ComponentContainerUtils.DoDynamicCastAsync( ParameterType, CreateInstanceAsync(ParameterType.GenericTypeArguments[0], Parameter.Name));
                    Parameters.Add(Task.FromResult(Component));
                }
                else if (ComponentInstance.IsValidFuncToInterfaceReference(ParameterType))
                {
                    LambdaExpression Lambda = Expression.Lambda(
                        ParameterType,
                        Expression.Convert(
                            ((Expression<Func<object>>)(() => componentContainer.InternalResolveInstanceAsync(ParameterType.GenericTypeArguments[0], true, progressCallback, new ComponentDescriptor[0]).Result)).Body,
                             ParameterType.GenericTypeArguments[0]));

                    Parameters.Add(Task.FromResult((Object)Lambda.Compile()));
                }
                else if (ComponentInstance.IsValidInterfaceArrayReference(ParameterType))
                {
                    Parameters.Add(componentContainer.InternalResolveInstancesAsArrayAsync(ParameterType.GetElementType(), progressCallback, resolveStack).ContinueWith(_=>(object)_.Result));
                }
                else if (ComponentInstance.IsValidInterfaceArrayTaskReference(ParameterType))
                {
                    object Component = ComponentContainerUtils.DoDynamicCastAsync(ParameterType, CreateInstanceArrayAsync(ParameterType.GenericTypeArguments[0].GetElementType()));
                    Parameters.Add(Task.FromResult(Component));
                }
                else if (ComponentInstance.IsValidFuncToInterfaceArrayReference(ParameterType))
                {
                    LambdaExpression Lambda = Expression.Lambda(
                        ParameterType,
                        Expression.Convert(
                            ((Expression<Func<Array>>) (() => componentContainer.InternalResolveInstancesAsArrayAsync(ParameterType.GenericTypeArguments[0].GetElementType(), progressCallback, new ComponentDescriptor[0]).Result)).Body,
                            ParameterType.GenericTypeArguments[0]));

                    Parameters.Add(Task.FromResult((Object)Lambda.Compile()));
                }
                else if (ParameterType == typeof(ComponentRepository))
                {
                    Parameters.Add(Task.FromResult((object) this.Descriptor.PrivateRepository ?? this.Descriptor.Repository));
                }
                else if (ParameterType == typeof(ComponentContainer))
                {
                    Parameters.Add(Task.FromResult((object)componentContainer));
                }
                else if (ParameterType.IsAssignableFrom(this.Descriptor.ConfigType))
                {
                    Parameters.Add(Task.FromResult((object)this.Descriptor.Config));
                }
                else
                {
                    throw new InvalidOperationException("Internal error: tried to resolve constructor parameters even though the constructor wasn't adequate");
                }
            }
            return await Task.WhenAll(Parameters);
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
                else if (ComponentInstance.IsValidInterfaceTaskReference(ParameterType))
                {
                    //Task to Simple interface reference
                    if (CheckInterfaceParameter(componentContainer, ParameterType.GenericTypeArguments[0], true, $"{ParameterType.Name} {Parameter.Name}", "a", out diagnosisInformation) == false)
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
                else if (ComponentInstance.IsValidInterfaceArrayTaskReference(ParameterType))
                {
                    //Task to Array of interface
                    if (CheckInterfaceParameter(componentContainer, ParameterType.GenericTypeArguments[0].GetElementType(), false, $"{ParameterType.Name} {Parameter.Name}", "an array of a", out diagnosisInformation) == false)
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
                    diagnosisInformation = $"parameter '{ParameterType.Name} {Parameter.Name}' type not supported. Except component interfaces, arrays of component interfaces, Func<> returning component interfaces or arrays of them or Tasks returning the above, only ComponentRepository, ComponentContainer and the 'Config' Type using in component registration (if given) can be used.";
                    return false;
                }
            }

            diagnosisInformation = "OK";
            return true;
        }

        private static bool IsValidFuncToInterfaceArrayReference(Type parameterType)
        {
            return parameterType.IsConstructedGenericType &&
                   parameterType.GetGenericTypeDefinition() == typeof(Func<>) &&
                   parameterType.GenericTypeArguments[0].IsArray &&
                   parameterType.GenericTypeArguments[0].GetElementType().IsInterface();
        }

        private static bool IsValidFuncToInterfaceReference(Type parameterType)
        {
            return parameterType.IsConstructedGenericType &&
                   parameterType.GetGenericTypeDefinition() == typeof(Func<>) &&
                   parameterType.GenericTypeArguments[0].IsInterface();
        }

        private static bool IsValidInterfaceReference(Type parameterType)
        {
            return parameterType.IsInterface();
        }

        private static bool IsValidInterfaceTaskReference(Type parameterType)
        {
            return parameterType.IsConstructedGenericType &&
                   parameterType.GetGenericTypeDefinition() == typeof(Task<>) &&
                   parameterType.GenericTypeArguments[0].IsInterface();
        }

        private static bool IsValidInterfaceArrayReference(Type parameterType)
        {
            return parameterType.IsArray &&
                   parameterType.GetElementType().IsInterface();
        }

        private static bool IsValidInterfaceArrayTaskReference(Type parameterType)
        {
            return parameterType.IsConstructedGenericType &&
                   parameterType.GetGenericTypeDefinition() == typeof(Task<>) &&
                   parameterType.GenericTypeArguments[0].IsArray &&
                   parameterType.GenericTypeArguments[0].GetElementType().IsInterface();
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

        internal abstract Task<object> CreateInstanceAsync(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback, ComponentDescriptor[] resolveStack);

        internal async Task<object> DoCreateInstanceAsync(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback, ComponentDescriptor[] resolveStack)
        {
            this.CheckDisposed();

            this.Descriptor.DbC_Assure(value => value.Type == interfaceType || value.ProvidesInterface(interfaceType),
                $"Requested interface type '{interfaceType.FullName}' not supported by component '{this.Descriptor.Type.FullName}'");

            return this.CastTo(await this.CreateWithOptimalConstructorAsync(componentContainer, progressCallback,resolveStack), interfaceType);
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