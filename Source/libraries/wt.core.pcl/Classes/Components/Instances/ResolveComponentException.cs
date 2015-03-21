using System;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// Thrown if a component could not be resolved
    /// </summary>
    public class ResolveComponentException : Exception
    {
        /// <summary/>
        public ResolveComponentException(string message)
            :base(message)
        {
            
        }
    }
}