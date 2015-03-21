using System;

namespace WhileTrue.Classes.Components
{
    /// <summary/>
    public class ComponentInstanceEventArgs : EventArgs
    {
        private readonly ComponentInstance componentInstance;

        /// <summary>
        /// Creates the event args.
        /// </summary>
        /// <param name="componentInstance">componentDescriptor that is accessible via the 
        /// <see cref="ComponentInstance"/> property</param>
        public ComponentInstanceEventArgs(ComponentInstance componentInstance)
        {
            this.componentInstance = componentInstance;
        }

        /// <summary>
        /// Gets the componentInstance of the event
        /// </summary>
        public ComponentInstance ComponentInstance
        {
            get { return this.componentInstance; }
        }
    }
}