using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;


// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("WhileTrue.ModelInspector")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("WhileTrue.ModelInspector")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

#if DEBUG
[assembly: NUnit.Framework.RequiresSTA]
#endif