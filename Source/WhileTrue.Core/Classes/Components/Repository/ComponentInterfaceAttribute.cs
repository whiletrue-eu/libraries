using System;
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// identifies an interface as a component interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface),MeansImplicitUse]
    public class ComponentInterfaceAttribute : Attribute
    {
    }
}