using WhileTrue.Classes.Components;

namespace WhileTrue.DragNDrop.Facades.ImageLibraryStore
{
    [ComponentInterface]
    public interface IImageLibraryStore
    {
        string[] GetGroups(string group);
        string[] GetImages(string group);
        void SetGroups(string group, string[] groups);
        void SetImages(string group, string[] images);
    }
}