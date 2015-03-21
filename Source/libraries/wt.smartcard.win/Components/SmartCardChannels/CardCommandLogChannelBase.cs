using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Facades.SmartCardChannels;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Components.SmartCardChannels
{
    public class CardCommandLogChannelBase<TSmartCardChannelType> where TSmartCardChannelType : ISmartCardChannel
    {
        private readonly ICardCommandLogger logger;
        private readonly string streamName;

        protected CardCommandLogChannelBase(TSmartCardChannelType channel, ICardCommandLogger logger, string streamName)
        {
            this.Channel = channel;
            this.logger = logger;
            this.streamName = streamName;
            this.Channel.DbC_AssureArgumentNotNull("channel");

            this.Channel.SmartCard.RemovedFromReader += this.SmartCard_RemovedFromReader;
        }

        protected TSmartCardChannelType Channel { get; }

        private void SmartCard_RemovedFromReader(object sender, SmartCardEventArgs e)
        {
            this.logger.LogRemoval(e.SmartCard, this.streamName, "");
        }

        protected void LogPowerOff(string additionalInformation)
        {
            this.logger.LogPowerOff(this.Channel.SmartCard, this.streamName, additionalInformation);
        }

        protected void LogPowerOn(string additionalInformation)
        {
            this.logger.LogPowerOn(this.Channel.SmartCard, this.streamName, additionalInformation);
        }

        protected void LogCommand(CardCommand command, string additionalInformation)
        {
            this.logger.LogCommand(this.Channel.SmartCard, this.streamName, command, additionalInformation);
        }

        protected void LogResponse(CardResponse response, string additionalInformation)
        {
            this.logger.LogResponse(this.Channel.SmartCard, this.streamName, response, additionalInformation);
        }
    }

    internal class TpduCommandLogChannel : CardCommandLogChannelBase<ITpduChannel>, ITpduChannel
    {
        public TpduCommandLogChannel(ITpduChannel channel, ICardCommandLogger logger, string streamName)
            : base(channel, logger, streamName)
        {
        }

        #region ITPDUChannel Members

        public ISmartCard SmartCard => this.Channel.SmartCard;

        public void Connect(Protocol protocol)
        {
            this.Channel.Connect(protocol);
            this.LogPowerOn($"Protocol: {protocol}");
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