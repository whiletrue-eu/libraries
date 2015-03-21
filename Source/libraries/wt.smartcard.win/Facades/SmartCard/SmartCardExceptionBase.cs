using System;

namespace WhileTrue.Facades.SmartCard
{
    public class SmartCardExceptionBase : ApplicationException
    {
        public SmartCardExceptionBase(string message)
            :base(message)
        {
        }
    }
}