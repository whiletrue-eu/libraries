namespace WhileTrue.Modules.ModelInspector
{
    internal interface IModelGroup
    {
        IModelInfoCollection Models { get; }
        string Name { get; set; }
    }
}