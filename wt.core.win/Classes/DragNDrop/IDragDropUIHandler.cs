using System;
using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    /// <summary>
    /// Implemented by handler of UI specific behaviour for drag and drop sources.
    /// </summary>
    public interface IDragDropUiSourceHandler
    {
        /// <summary>
        /// Type this handler is usable for
        /// </summary>
        Type Type { get; }
        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        IDragDropUiSourceHandlerInstance Create(DependencyObject element, IDragDropSourceAdapter adapter);
    }

    /// <summary>
    /// Implemented by UI drag and drop source handler instance.
    /// </summary>
    /// <remarks>
    /// On creation, it is responsible to wire up drag and drop specific events that have
    /// to be forwarded to the source adapter. ON dispose, it must unhook the events for cleanup.
    /// </remarks>
    public interface IDragDropUiSourceHandlerInstance : IDisposable
    {
    }

    /// <summary>
    /// Implemented by handler of UI specific behaviour for drag and drop targets.
    /// </summary>
    public interface IDragDropUiTargetHandler
    {
        /// <summary>
        /// Type this handler is usable for
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        /// <param name="adapter">Adapter of the controls underlying model</param>
        /// <param name="makeDroppable">if set to <c>true</c>, the control must be changed to accept drop actions.</param>
        IDragDropUiTargetHandlerInstance Create(DependencyObject element, IDragDropTargetAdapter adapter, bool makeDroppable);
    }

    /// <summary>
    /// Implemented by UI drag and drop source handler instance.
    /// </summary>
    public interface IDragDropUiTargetHandlerInstance : IDisposable
    {
        /// <summary>
        /// Notifies the handler, that a drag operation has started above the control
        /// </summary>
        void NotifyDragStarted(DragDropEffect effect);

        /// <summary>
        /// Notifies the handler, that a drag operation has ended above the control, either because 
        /// the item was dropped, or because the mouse left the controls area
        /// </summary>
        void NotifyDragEnded();
        /// <summary>
        /// Notifies the handler of an update of the mouse position on the control
        /// </summary>
        void NotifyDragChanged(DragDropEffect effect, DragPosition position);
        /// <summary>
        /// Gets additonal drop information that can be provided by the handler for the 
        /// target handler implemented in the model.
        /// </summary>
        AdditionalDropInfo GetAdditionalDropInfo(DragPosition position);
    }

    /// <summary>
    /// Implemented by handler of UI specific behaviour for drag and drop sources.
    /// </summary>
    public interface IDragDropUiHelper
    {
        /// <summary>
        /// Type this handler is usable for
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Creates an instance of a handler for a specifc control
        /// </summary>
        /// <param name="element">Control the handler shall be created for</param>
        IDragDropUiHelperInstance Create(UIElement element);
    }

    /// <summary>
    /// Implemented by UI drag and drop source handler instance.
    /// </summary>
    public interface IDragDropUiHelperInstance : IDisposable
    {
        /// <summary>
        /// Notifies the handler of an update of the mouse position on the control
        /// </summary>
        void NotifyDrag(DragPosition position);
    }

    /// <summary>
    /// Container for additional drop information. Drop information can be freely defined
    /// by the handler in form of different data types.
    /// </summary>
    public class AdditionalDropInfo
    {
        private readonly object[] info;

        /// <summary>
        /// Creates the drop info and adds the given objects per data type.
        /// </summary>
        /// <remarks>
        /// If a data type is given multiple times,only the first occurence will be returned
        /// </remarks>
        public AdditionalDropInfo( params object[] info)
        {
            this.info = info;
        }

        /// <summary>
        /// Returns the data stored in the cotaioner, or the default value given if data
        /// with the given <c>InfoType</c> is not found
        /// </summary>
        public TInfoType GetInfoOrDefault<TInfoType>(TInfoType defaultValue)
        {
            foreach (object Info in this.info)
            {
                if (Info.GetType().IsAssignableFrom(typeof(TInfoType)))
                {
                    return (TInfoType) Info;
                }
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// Data Type that can carry additional drop information about the index into a list.
    /// </summary>
    public class DropIndex
    {
        /// <summary/>
        public DropIndex(int index)
        {
            this.Index = index;
        }

        /// <summary/>
// ReSharper disable MemberCanBePrivate.Global
        public int Index { get; set; }
// ReSharper restore MemberCanBePrivate.Global

        /// <summary/>
        public static implicit operator int(DropIndex value)
        {
            return value.Index;
        }

        /// <summary/>
        public static implicit operator DropIndex(int value)
        {
            return new DropIndex(value);
        }
    }
}