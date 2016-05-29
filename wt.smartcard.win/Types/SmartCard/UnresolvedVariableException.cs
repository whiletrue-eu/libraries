using System;

namespace WhileTrue.Types.SmartCard
{
    /// <summary>
    /// <see cref="VariableCardCommand"/>/<see cref="Variable"/> specific.
    /// Used, if the variable was not resolved before the card command is serialised.
    /// </summary>
    public class UnresolvedVariableException : Exception
    {
        /// <summary>
        /// Creates the exception
        /// </summary>
        public UnresolvedVariableException(VariableCardCommand command, Variable variable)
            : base($"Unresolved variable: {variable.Name}")
        {
            this.Variable = variable;
            this.Command = command;
        }

        /// <summary>
        /// gets the variable that was not resolved
        /// </summary>
        public Variable Variable { get; }

        /// <summary>
        /// Gets the command the variable is defined in
        /// </summary>
        public VariableCardCommand Command { get; }
    }
}