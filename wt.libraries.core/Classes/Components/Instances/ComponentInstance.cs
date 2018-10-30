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
    ///     Container for an instance of a specific component
    /// </summary>
    [PublicAPI]
    public abstract class ComponentInstance
    {
        private static int debugIndent;
        private bool disposed;

        internal ComponentInstance(ComponentDescriptor componentDescriptor)
        {
            Descriptor = componentDescriptor;
        }

        /// <summary>
        ///     Returns the name of the component as specified in the Component Attribute
        /// </summary>
        public string Name => Descriptor.Name;

        /// <summary>
        ///     Retruns the component descriptor
        /// </summary>
        public ComponentDescriptor Descriptor { get; }

        #region IDisposable Members

        internal virtual void Dispose(ComponentContainer componentContainer)
        {
            CheckDisposed();
            disposed = true;
        }

        #endregion

        private object CreateWithOptimalConstructor(ComponentContainer componentContainer,
            Action<string> progressCallback)
        {
            ConstructorInfo OptimalConstructor = null;
            var DiagnosisInformation = new List<string>();

            foreach (var Constructor in Descriptor.Type.GetConstructors())
            {
                string ConstructorDiagnosisInformation;
                if (CanGetParametersFor(Constructor, componentContainer, out ConstructorDiagnosisInformation))
                    if (OptimalConstructor == null ||
                        Constructor.GetParameters().Length > OptimalConstructor.GetParameters().Length)
                        OptimalConstructor = Constructor;
                DiagnosisInformation.Add($"{Constructor}: {ConstructorDiagnosisInformation}");
            }

            if (OptimalConstructor != null)
            {
                var Start = DateTime.UtcNow;
                Debug.WriteLine($"{new string(' ', debugIndent * 3)}Start instantiation of {Name}");
                debugIndent++;
                try
                {
                    var ConstructorParameters =
                        GetParametersFor(OptimalConstructor, componentContainer, progressCallback).ToArray();
                    var ParameterResolved = DateTime.UtcNow;
                    progressCallback?.Invoke(Descriptor.Name);
                    var Component = OptimalConstructor.Invoke(ConstructorParameters);
                    var ComponentCreated = DateTime.UtcNow;
                    debugIndent--;
                    Debug.WriteLine(
                        $"{new string(' ', debugIndent * 3)}Instantiation of {Name} took {Format(ComponentCreated - Start)} (params: {Format(ParameterResolved - Start)}, ctor: {Format(ComponentCreated - ParameterResolved)})");
                    return Component;
                }
                catch (Exception Error)
                {
                    debugIndent--;
                    Debug.WriteLine(
                        $"{new string(' ', debugIndent * 3)}Instantiation of {Name} failed with message: {Error.Message.Replace("\n", $"\n{new string(' ', debugIndent * 3)}")}");
                    throw;
                }
            }

            var Message =
                $"No valid constructor could be found for component {Descriptor.Name}. Make sure, that all component interfaces are marked with the ComponentInterface attribute\n\nDetailed information for the different constructors:\n{string.Join("\n", DiagnosisInformation.ToArray())}";
            throw new ResolveComponentException(Message);
        }

        private static string Format(TimeSpan span)
        {
            return $"{(int) span.TotalSeconds}.{span:ffff}";
        }

        /// <summary>
        ///     Gets the parameter list to call the given constructor. If a parameter could not be
        ///     resolved, null is returned
        /// </summary>
        private IEnumerable<object> GetParametersFor(ConstructorInfo constructor, ComponentContainer componentContainer,
            Action<string> progressCallback)
        {
            foreach (var Parameter in constructor.GetParameters())
            {
                var ParameterType = Parameter.ParameterType;
                if (IsValidInterfaceReference(ParameterType))
                {
                    var Component = componentContainer.InternalResolveInstance(ParameterType, true, progressCallback);
                    Component.DbC_AssureNotNull(
                        $"Could not resolve component '{Descriptor.Type.FullName}' even though resolver claimed that he can. Parameter '{Parameter.Name}' (type providing '{ParameterType.FullName}' implementation) could not be instanciated");
                    yield return Component;
                }
                else if (IsValidFuncToInterfaceReference(ParameterType))
                {
                    var Lambda = Expression.Lambda(
                        ParameterType,
                        Expression.Convert(
                            ((Expression<Func<object>>) (() =>
                                componentContainer.InternalResolveInstance(ParameterType.GenericTypeArguments[0], true,
                                    progressCallback))).Body,
                            ParameterType.GenericTypeArguments[0]));

                    yield return Lambda.Compile();
                }
                else if (IsValidInterfaceArrayReference(ParameterType))
                {
                    yield return componentContainer.InternalResolveInstancesAsArray(ParameterType.GetElementType(),
                        progressCallback);
                }
                else if (IsValidFuncToInterfaceArrayReference(ParameterType))
                {
                    var Lambda = Expression.Lambda(
                        ParameterType,
                        Expression.Convert(
                            ((Expression<Func<Array>>) (() =>
                                componentContainer.InternalResolveInstancesAsArray(
                                    ParameterType.GenericTypeArguments[0].GetElementType(), progressCallback))).Body,
                            ParameterType.GenericTypeArguments[0]));

                    yield return Lambda.Compile();
                }
                else if (ParameterType == typeof(ComponentRepository))
                {
                    if (Descriptor.PrivateRepository != null)
                        yield return Descriptor.PrivateRepository;
                    else
                        yield return Descriptor.Repository;
                }
                else if (ParameterType == typeof(ComponentContainer))
                {
                    yield return componentContainer;
                }
                else if (ParameterType.IsAssignableFrom(Descriptor.ConfigType))
                {
                    yield return Descriptor.Config;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Internal error: tried to resolve constructor parameters even though the constructor wasn't adequate");
                }
            }
        }

        private static bool IsValidInterfaceArrayReference(Type parameterType)
        {
            return parameterType.IsArray && parameterType.GetElementType().IsInterface();
        }

        /// <summary>
        /// </summary>
        private bool CanGetParametersFor(ConstructorInfo constructor, ComponentContainer componentContainer,
            out string diagnosisInformation)
        {
            foreach (var Parameter in constructor.GetParameters())
            {
                var ParameterType = Parameter.ParameterType;

                if (IsValidInterfaceReference(ParameterType))
                {
                    //Simple interface reference
                    if (CheckInterfaceParameter(componentContainer, ParameterType, true,
                            $"{ParameterType.Name} {Parameter.Name}", "a", out diagnosisInformation) ==
                        false) return false;
                }
                else if (IsValidFuncToInterfaceReference(ParameterType))
                {
                    //Func<> Interface reference
                    if (CheckInterfaceParameter(componentContainer, ParameterType.GenericTypeArguments[0], true,
                            $"{ParameterType.Name} {Parameter.Name}", "Func<> returning a", out diagnosisInformation) ==
                        false) return false;
                }
                else if (IsValidInterfaceArrayReference(ParameterType))
                {
                    //Array of interface
                    if (CheckInterfaceParameter(componentContainer, ParameterType.GetElementType(), false,
                            $"{ParameterType.Name} {Parameter.Name}", "an array of a", out diagnosisInformation) ==
                        false) return false;
                }
                else if (IsValidFuncToInterfaceArrayReference(ParameterType))
                {
                    //Func<> of Array of interface
                    if (CheckInterfaceParameter(componentContainer,
                            ParameterType.GenericTypeArguments[0].GetElementType(), false,
                            $"{ParameterType.Name} {Parameter.Name}", "a Func<> returning an array of a",
                            out diagnosisInformation) == false) return false;
                }
                else if (ParameterType == typeof(ComponentRepository))
                {
                    //Parameter is OK
                }
                else if (ParameterType == typeof(ComponentContainer))
                {
                    //Parameter is OK
                }
                else if (Descriptor.ConfigType != null && ParameterType.IsAssignableFrom(Descriptor.ConfigType))
                {
                    //Parameter is OK
                }
                else
                {
                    diagnosisInformation =
                        $"parameter '{ParameterType.Name} {Parameter.Name}' type not supported. Except component interfaces, arrays of component interfaces, Func<> returning component interfaces or arrays of them, only ComponentRepository, ComponentContainer and the 'Config' Type using in component registration (if given) can be used.";
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

        private bool CheckInterfaceParameter(ComponentContainer componentContainer, Type parameterType,
            bool mustResolve, string parameterTypeAndName, string specialTypeName, out string diagnosisInformation)
        {
            if (ComponentRepository.IsComponentInterface(parameterType))
            {
                if (mustResolve == false || componentContainer.CanResolveComponent(parameterType))
                {
                    //Parameter is OK
                }
                else
                {
                    diagnosisInformation =
                        $"parameter '{parameterTypeAndName}' cannot be resolved. Make sure there is a registered component providing this interface.";
                    return false;
                }
            }
            else
            {
                diagnosisInformation =
                    $"parameter '{parameterTypeAndName}' is not {specialTypeName} component interface. Make sure that the interfaces to use are marked with the [ComponentInterface] attribute.";
                return false;
            }

            diagnosisInformation = "OK";
            return true;
        }

        internal abstract object CreateInstance(Type interfaceType, ComponentContainer componentContainer,
            Action<string> progressCallback);

        internal object DoCreateInstance(Type interfaceType, ComponentContainer componentContainer,
            Action<string> progressCallback)
        {
            CheckDisposed();

            Descriptor.DbC_Assure(value => value.Type == interfaceType || value.ProvidesInterface(interfaceType),
                $"Requested interface type '{interfaceType.FullName}' not supported by component '{Descriptor.Type.FullName}'");

            return CastTo(CreateWithOptimalConstructor(componentContainer, progressCallback), interfaceType);
        }

        private object CastTo(object instance, Type interfaceType)
        {
            var Instance = TryCastTo(instance, interfaceType);

            Instance.DbC_AssureNotNull(
                $"Requested interface type '{interfaceType.FullName}' not supported by component '{Descriptor.Type.FullName}'");

            return Instance;
        }

        private object TryCastTo(object instance, Type interfaceType)
        {
            return Descriptor.TryCastTo(instance, interfaceType);
        }

        private void CheckDisposed()
        {
            disposed.DbC_Assure(value => value == false, new ObjectDisposedException(""));
        }
    }
}