using System;

namespace WhileTrue.Types.SmartCard
{
    public class InvalidAtrCodingException : ApplicationException
    {
        public InvalidAtrCodingException(string message):base(message)
        {
        }

        public InvalidAtrCodingException(string message, params object[] parameters)
            :base(string.Format(message,parameters))
        {
        }

    }
}