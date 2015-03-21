using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Facades.SmartCard.Channels;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Components.SmartCardChannels
{
    public class CardCommandLogChannelBase<SmartCardChannelType> where SmartCardChannelType : ISmartCardChannel
    {
        private readonly SmartCardChannelType channel;
        private readonly ICardCommandLogger logger;
        private readonly string streamName;

        protected CardCommandLogChannelBase(SmartCardChannelType channel, ICardCommandLogger logger, string streamName)
        {
            this.channel = channel;
            this.logger = logger;
            this.streamName = streamName;
            this.channel.DbC_AssureArgumentNotNull("channel");

            this.channel.SmartCard.RemovedFromReader += this.SmartCard_RemovedFromReader;
        }

        protected SmartCardChannelType Channel
        {
            get { return this.channel; }
        }

        private void SmartCard_RemovedFromReader(object sender, SmartCardEventArgs e)
        {
            this.logger.LogRemoval(e.SmartCard, this.streamName, "");
        }

        protected void LogPowerOff(string additionalInformation)
        {
            this.logger.LogPowerOff(this.channel.SmartCard, this.streamName, additionalInformation);
        }

        protected void LogPowerOn(string additionalInformation)
        {
            this.logger.LogPowerOn(this.channel.SmartCard, this.streamName, additionalInformation);
        }

        protected void LogCommand(CardCommand command, string additionalInformation)
        {
            this.logger.LogCommand(this.channel.SmartCard, this.streamName, command, additionalInformation);
        }

        protected void LogResponse(CardResponse response, string additionalInformation)
        {
            this.logger.LogResponse(this.channel.SmartCard, this.streamName, response, additionalInformation);
        }
    }

    internal class TPDUCommandLogChannel : CardCommandLogChannelBase<ITPDUChannel>, ITPDUChannel
    {
        public TPDUCommandLogChannel(ITPDUChannel channel, ICardCommandLogger logger, string streamName)
            : base(channel, logger, streamName)
        {
        }

        #region ITPDUChannel Members

        public ISmartCard SmartCard
        {
            get { return this.Channel.SmartCard; }
        }

        public void Connect(Protocol protocol)
        {
            this.Channel.Connect(protocol);
            this.LogPowerOn(string.Format("Protocol: {0}", protocol));
        }

        public void Disconnect()
        {
            this.Channel.Disconnect();
            this.LogPowerOff("");
        }

        public void Eject()
        {
            this.Channel.Eject();
            this.LogPowerOff("");
        }

        public CardResponse Transmit(CardCommand command)
        {
            this.LogCommand(command, "");
            CardResponse Response = this.Channel.Transmit(command);
            this.LogResponse(Response, "");
            return Response;
        }

        #endregion
    }

    public interface ICardCommandLogger
    {
        void LogRemoval(ISmartCard smartCard, string streamName, string additionalInformation);
        void LogPowerOff(ISmartCard smartCard, string streamName, string additionalInformation);
        void LogPowerOn(ISmartCard smartCard, string streamName, string additionalInformation);
        void LogCommand(ISmartCard smartCard, string streamName, CardCommand command, string additionalInformation);
        void LogResponse(ISmartCard smartCard, string streamName, CardResponse response, string additionalInformation);
    }
}