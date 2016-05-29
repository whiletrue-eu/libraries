using WhileTrue.Types.SmartCard;

namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// <see cref="Variable"/> specific.
    /// Used, if the variable could not be resolved.
    /// </summary>
    public class UnableToResolveVariableException : SmartCardExceptionBase
    {
        /// <summary>
        /// Creates the exception
        /// </summary>
        public UnableToResolveVariableException(Variable variable, string cause)
            : base($"Unresolved variable {variable.Name}: {cause}")
        {
            this.Cause = cause;
            this.Variable = variable;
        }


        /// <summary>
        /// Gets the variable that was not resolved
        /// </summary>
        public Variable Variable { get; }

        /// <summary>
        /// Gets the cause, why the variable could not be resolved.
        /// </summary>
        public string Cause { get; }
    }
}