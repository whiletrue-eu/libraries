using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;


// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("WhileTrue.core")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("WhileTrue.core")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("7f9ae03d-67f7-48d6-8e98-23ccfd360156")]

[assembly: XmlnsPrefixAttribute("http://schemas.whiletrue.eu/xaml", "wt")]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", "WhileTrue.Classes.Commanding")]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", "WhileTrue.Classes.DragNDrop")]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", "WhileTrue.Classes.Framework")]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", "WhileTrue.Classes.Wpf")]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", "WhileTrue.Classes.UIFeatures")]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", "WhileTrue.Controls")]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", "WhileTrue.Controls.SplashScreen")]
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
    )]

#if DEBUG
[assembly: NUnit.Framework.RequiresSTA]
#endif