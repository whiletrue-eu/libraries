using System;

namespace WhileTrue.Classes.Framework
{
    internal class CircularDependencyException : ApplicationException
    {
        public CircularDependencyException(string message)
            :base(message)
        {
            
        }
    }
}