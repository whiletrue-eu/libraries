using System;
using System.Collections.Generic;
using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    /// <summary>
    /// Factory class for the <see cref="DragDropTarget"/> helper implementation.
    /// </summary>
    /// <seealso cref="DragDropTarget.GetFactory()"/>
    public class DragDropTargetFactory
    {
        private readonly List<DragDropTarget.DragDropItemHandlerBase> handlers = new List<DragDropTarget.DragDropItemHandlerBase>();

        internal DragDropTargetFactory()
        {
        }

        // ReSharper disable MemberCanBePrivate.Global
        /// <summary>
        /// Add handling routines for the given <c>ItemType</c>. Allowed effects are given as concrete values.
        /// </summary>
        public DragDropTargetFactory AddTypeHandler<TItemType>(DragDropEffects allowedEffects, DragDropEffect defaultEffect, Action<TItemType, DragDropEffect, AdditionalDropInfo> doDropAction)
        {
            return this.AddTypeHandler(_ => allowedEffects, _ => defaultEffect, doDropAction);
        }

        /// <summary>
        /// Add handling routines for the given <c>ItemType</c>. Allowed effects are given as delegates that can dynamically generate allowed drop effects.
        /// </summary>
        public DragDropTargetFactory AddTypeHandler<TItemType>(Func<TItemType, DragDropEffects> allowedEffects, Func<TItemType, DragDropEffect> defaultEffect, Action<TItemType, DragDropEffect, AdditionalDropInfo> doDropAction)
        {
            this.handlers.Add(new DragDropTarget.DragDropItemHandler<TItemType>(allowedEffects, defaultEffect, doDropAction));
            return this;
        }

        /// <summary>
        /// Finalises addition of handlers and creates the <see cref="DragDropTarget"/>
        /// </summary>
        public DragDropTarget Create()
        {
            return new DragDropTarget(this.handlers.ToArray());
        }
 
        // ReSharper restore MemberCanBePrivate.Global
   }
}