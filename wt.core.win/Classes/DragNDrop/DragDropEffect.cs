using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    /// <summary>
    /// Single Drop effect. Assignment compatible with <see cref="DragDropEffects"/>
    /// </summary>
    public enum DragDropEffect
    {
        /// <summary/>
        Copy = DragDropEffects.Copy,
        /// <summary/>
        Move = DragDropEffects.Move,
        /// <summary/>
        Link = DragDropEffects.Link,
        /// <summary/>
        None = DragDropEffects.None,
    }
}