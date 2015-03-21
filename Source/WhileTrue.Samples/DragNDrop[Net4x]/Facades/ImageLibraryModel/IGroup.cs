namespace WhileTrue.DragNDrop.Facades.ImageLibraryModel
{
    internal interface IGroup
    {
        IGroupCollection Groups { get; }
        IImageCollection Images { get; }
        string Name { get; set; }
    }
}