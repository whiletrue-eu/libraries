namespace WhileTrue.Classes.DragNDrop.DragDropUIHandler
{
    internal class DummyUiHandler : IDragDropUiTargetHandlerInstance
    {
        public void Dispose() { }
        public void NotifyDragStarted(DragDropEffect effect) { }
        public void NotifyDragEnded() { }
        public void NotifyDragChanged(DragDropEffect effect, DragPosition position) { }
        public AdditionalDropInfo GetAdditionalDropInfo(DragPosition position) { return new AdditionalDropInfo(); }
    }
}