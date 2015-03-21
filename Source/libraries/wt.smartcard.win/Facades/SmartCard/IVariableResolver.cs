using WhileTrue.Types.SmartCard;

namespace WhileTrue.Facades.SmartCard
{
    ///<summary>
    /// Used to resolve variable parts of APDU commands
    ///</summary>
    public interface IVariableResolver
    {
        ///<summary>
        /// Resolve the variable and store back the resolved value in the variable object
        ///</summary>
        void ResolveVariable(Variable variable);

        /// <summary>
        /// notfies that variable entry begins on the card reader
        /// </summary>
        void NotifyVariableEntryBegins(string name);

        /// <summary>
        /// notifies that variable entry has ended
        /// </summary>
        void NotifyVariableEntryEnded();
    }
}