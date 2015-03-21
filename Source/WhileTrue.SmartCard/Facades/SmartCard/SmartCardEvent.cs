using System;

namespace WhileTrue.Facades.SmartCard
{
    /// <supportingClass/>
    /// <summary/>
    public class SmartCardEventArgs : EventArgs
    {
        private readonly ISmartCard smartCard;

        /// <summary/>
        public SmartCardEventArgs(ISmartCard smartCard)
        {
            this.smartCard = smartCard;
        }

        /// <summary>
        /// Gets the smart card the event was fired for.
        /// </summary>
        public ISmartCard SmartCard
        {
            get { return this.smartCard; }
        }
    }
}