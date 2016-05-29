using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Facades.SmartCardChannels;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Components.SmartCardChannels
{
    public class SmartCardTpduChannel : ITpduChannel
    {
        public SmartCardTpduChannel(ISmartCard smartCard)
        {
            this.SmartCard.DbC_AssureNotNull();
            this.SmartCard = smartCard;
        }

        #region ITPDUChannel Members

        public ISmartCard SmartCard { get; }

        public void Connect(Protocol protocol)
        {
            this.SmartCard.Connect(protocol);
        }

        public void Disconnect()
        {
            this.SmartCard.Disconnect();
        }

        public void Eject()
        {
            this.SmartCard.Eject();
        }

        public CardResponse Transmit(CardCommand command)
        {
            return this.SmartCard.Transmit(command);
        }

        #endregion
    }
}