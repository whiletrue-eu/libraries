using System;

namespace WhileTrue.Types.SmartCard
{
    /// <summary>
    /// <see cref="VariableCardCommand"/>/<see cref="Variable"/> specific.
    /// Used, if the variable was not resolved before the card command is serialised.
    /// </summary>
    public class UnresolvedVariableException : Exception
    {
        private readonly VariableCardCommand command;
        private readonly Variable variable;

        /// <summary>
        /// Creates the exception
        /// </summary>
        public UnresolvedVariableException(VariableCardCommand command, Variable variable)
            : base(string.Format("Unresolved variable: {0}", variable.Name))
        {
            this.variable = variable;
            this.command = command;
        }

        /// <summary>
        /// gets the variable that was not resolved
        /// </summary>
        public Variable Variable
        {
            get { return this.variable; }
        }

        /// <summary>
        /// Gets the command the variable is defined in
        /// </summary>
        public VariableCardCommand Command
        {
            get { return this.command; }
        }
    }
}