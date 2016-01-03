using System;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    /// Exception that is thrown, if the user canceled a certain action
    /// </summary>
    public class UserCancelException : ApplicationException
    {
        /// <summary>
        /// Creates a new instance of a UserCancelException
        /// </summary>
        public UserCancelException()
            : base("The user canceled the operation")
        {
        }
    }
}