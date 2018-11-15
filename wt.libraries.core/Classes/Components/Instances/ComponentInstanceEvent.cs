using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Components
{
    /// <summary />
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public class ComponentInstanceEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates the event args.
        /// </summary>
        /// <param name="componentInstance">
        ///     componentDescriptor that is accessible via the
        ///     <see cref="ComponentInstance" /> property
        /// </param>
        public ComponentInstanceEventArgs(ComponentInstance componentInstance)
        {
            ComponentInstance = componentInstance;
        }

        /// <summary>
        ///     Gets the componentInstance of the event
        /// </summary>
        public ComponentInstance ComponentInstance { get; }
    }
}