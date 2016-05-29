using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

[assembly: AssemblyTitle("wt.core.win")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

[assembly: ComVisible(false)]
[assembly: Guid("7f9ae03d-67f7-48d6-8e98-23ccfd360156")]

[assembly: XmlnsPrefix("http://schemas.whiletrue.eu/xaml", "wt")]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", nameof(WhileTrue)+"."+ nameof(WhileTrue.Classes)+ "." + nameof(WhileTrue.Classes.Commands))]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", nameof(WhileTrue) + "." + nameof(WhileTrue.Classes) + "." + nameof(WhileTrue.Classes.DragNDrop))]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", nameof(WhileTrue) + "." + nameof(WhileTrue.Classes) + "." + nameof(WhileTrue.Classes.Wpf))]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", nameof(WhileTrue) + "." + nameof(WhileTrue.Classes) + "." + nameof(WhileTrue.Classes.UIFeatures))]
[assembly: XmlnsDefinition("http://schemas.whiletrue.eu/xaml", nameof(WhileTrue) + "." + nameof(WhileTrue.Controls))]
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
    )]

