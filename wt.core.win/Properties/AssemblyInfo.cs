using System.Reflection;
using System.Runtime.CompilerServices;
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

[assembly: InternalsVisibleTo("wt.core-test, PublicKey=" +
    "00240000048000009400000006020000002400005253413100040000010001003f60df4257cd72" +
    "864fd20018aba54f9130ea2a3b9cc5ac54668d56566fc19f4bcdaf0426681a992661c282c7340c" +
    "e85c7186019711ca93616c413cb612920539fe1131f904988d9a522f720106c6efb007f22a4a08" +
    "fb18eae209ae7786a2237bae19452c770a7202c18faa9cd83d1fa0f53cc12f1f7967919710b6c4" +
    "b48182d7")]