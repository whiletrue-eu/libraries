using System;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// <see cref="Variable"/> specific.
    /// Used, if the variable could not be resolved.
    /// </summary>
    public class UnableToResolveVariableException : SmartCardExceptionBase
    {
        private readonly string cause;
        private readonly Variable variable;

        /// <summary>
        /// Creates the exception
        /// </summary>
        public UnableToResolveVariableException(Variable variable, string cause)
            : base(string.Format("Unresolved variable {0}: {1}", variable.Name, cause))
        {
            this.cause = cause;
            this.variable = variable;
        }


        /// <summary>
        /// Gets the variable that was not resolved
        /// </summary>
        public Variable Variable
        {
            get { return this.variable; }
        }

        /// <summary>
        /// Gets the cause, why the variable could not be resolved.
        /// </summary>
        public string Cause
        {
            get { return this.cause; }
        }
    }
}