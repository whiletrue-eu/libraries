using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Facades.SmartCard.Channels;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Components.SmartCardChannels
{
    public class T0APDUChannel : IAPDUChannel
    {
        private readonly AutoResponseTriggerCollection autoResponseTrigger = new AutoResponseTriggerCollection();
        private readonly ITPDUChannel channel;

        public T0APDUChannel(ITPDUChannel channel, AutoResponseTrigger[] autoResponseTrigger)
        {
            this.channel.DbC_AssureNotNull();
            this.channel = channel;
            this.autoResponseTrigger.AddRange(autoResponseTrigger);
        }

        #region IAPDUChannel Members

        public ISmartCard SmartCard
        {
            get { return this.channel.SmartCard; }
        }

        public void Connect()
        {
            this.channel.Connect(Protocol.T0);
        }

        public void Disconnect()
        {
            this.channel.Disconnect();
        }

        public void Eject()
        {
            this.channel.Eject();
        }

        public CardResponse Transmit(CardCommand command)
        {
            CardResponse Response = this.channel.Transmit(command);
            CardCommand AutoResponseCommand = this.autoResponseTrigger.GetNextCommand(command, Response);
            if (AutoResponseCommand != null)
            {
                return this.channel.Transmit(AutoResponseCommand);
            }
            else
            {
                return Response;
            }
        }

        #endregion
    }
}