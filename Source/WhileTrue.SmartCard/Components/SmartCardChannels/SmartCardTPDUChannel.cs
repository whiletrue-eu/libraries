using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Facades.SmartCard.Channels;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Components.SmartCardChannels
{
    public class SmartCardTPDUChannel : ITPDUChannel
    {
        private readonly ISmartCard smartCard;

        public SmartCardTPDUChannel(ISmartCard smartCard)
        {
            this.smartCard.DbC_AssureNotNull();
            this.smartCard = smartCard;
        }

        #region ITPDUChannel Members

        public ISmartCard SmartCard
        {
            get { return this.smartCard; }
        }

        public void Connect(Protocol protocol)
        {
            this.smartCard.Connect(protocol);
        }

        public void Disconnect()
        {
            this.smartCard.Disconnect();
        }

        public void Eject()
        {
            this.smartCard.Eject();
        }

        public CardResponse Transmit(CardCommand command)
        {
            return this.smartCard.Transmit(command);
        }

        #endregion
    }
}