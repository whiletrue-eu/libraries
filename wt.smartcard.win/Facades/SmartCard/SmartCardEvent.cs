using System;

namespace WhileTrue.Facades.SmartCard
{
    /// <supportingClass/>
    /// <summary/>
    public class SmartCardEventArgs : EventArgs
    {
        /// <summary/>
        public SmartCardEventArgs(ISmartCard smartCard)
        {
            this.SmartCard = smartCard;
        }

        /// <summary>
        /// Gets the smart card the event was fired for.
        /// </summary>
        public ISmartCard SmartCard { get; }
    }
}