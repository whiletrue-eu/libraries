namespace WhileTrue.Modules.ModelInspector
{
    internal interface IModelInfo
    {
        IModelNodeBase Root { get; }
        string Name { get; set; }
        bool NonClosable { get; }
    }
}