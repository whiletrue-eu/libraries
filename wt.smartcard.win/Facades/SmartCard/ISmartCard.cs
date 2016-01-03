// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global
using System;
using System.Threading;
using System.Threading.Tasks;
using WhileTrue.Types.SmartCard;


namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// Instance that is implemented by smart card wrapper classes
    /// </summary>
    public interface ISmartCard
    {
        /// <summary>
        /// Gets the <see cref="SmartCardUnavailableException"/> wrapper for the card reader the smart acrd is inserted in.
        /// </summary>
        /// <exception cref="ICardReader">Thrown if the smart card was removed from the card reader</exception>
        ICardReader CardReader { get; }

        /// <summary>
        /// Gets the ATR of the smart card
        /// </summary>
        /// <exception cref="SmartCardUnavailableException">Thrown if the smart card was removed from the card reader</exception>
        byte[] Atr { get; }

        /// <summary>
        /// Gets the Protocol with which communication with the smart card was established using the <see cref="Connect(WhileTrue.Facades.SmartCard.Protocol)"/> method
        /// </summary>
        /// <exception cref="SmartCardUnavailableException">Thrown if the smart card was removed from the card reader</exception>
        /// <exception cref="SmartCardNotConnectedException">Thrown if no communication with the smart card is established</exception>
        Protocol Protocol { get; }

        /// <summary>
        /// Gets the current state of the smart card
        /// </summary>
        /// <exception cref="SmartCardUnavailableException">Thrown if the smart card was removed from the card reader</exception>
        CardReaderState State { get; }

        /// <summary>
        /// Gets if the smart card was removed from its card reader.
        /// </summary>
        bool IsRemoved { get; }

        /// <summary>
        /// Gets the name of the smart card
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns, whether the card is connected or not.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Ejects the smart card from its card reader.
        /// </summary>
        /// <remarks>
        /// If not supported, the card is only disconnected
        /// </remarks>
        void Eject();

        /// <summary>
        /// Fired, if the state of the card changes
        /// </summary>
        /// <remarks>
        /// This event may be fired from another thread
        /// </remarks>
        /// <exception cref="SmartCardUnavailableException">Thrown if the smart card was removed from the card reader</exception>
        event EventHandler<SmartCardEventArgs> StateChanged;

        /// <summary>
        /// Fired, if the smart card is removed from the card reader
        /// </summary>
        /// <remarks>
        /// This event may be fired from another thread
        /// </remarks>
        event EventHandler<SmartCardEventArgs> RemovedFromReader;

        /// <summary>
        /// Connect to the card. The connection will be exclusive
        /// </summary>
        /// <param name="protocol">Protocol to use for the communication with the card</param>
        void Connect(Protocol protocol);

        /// <summary>
        /// Connect to the card. The connection will be exclusive
        /// </summary>
        /// <param name="protocol">Protocol to use for the communication with the card</param>
        /// <param name="timeout">timeout (in ms) for the connection. If set the connection will be retried in case of
        /// other programs occupying the card reader (sharing violation) for the given amount of time</param>
        void Connect(Protocol protocol, int timeout);
        

        /// <summary>
        /// Connect to the card. The connection will be exclusive
        /// </summary>
        /// <param name="protocol">Protocol to use for the communication with the card</param>
        Task ConnectAsync(Protocol protocol);   
     
        /// <summary>
        /// Connect to the card. The connection will be exclusive
        /// </summary>
        /// <param name="protocol">Protocol to use for the communication with the card</param>
        /// <param name="timeout">timeout (in ms) for the connection. If set the connection will be retried in case of
        /// other programs occupying the card reader (sharing violation) for the given amount of time</param>
        Task ConnectAsync(Protocol protocol, int timeout);

        /// <summary>
        /// Connect to the card. The connection will be exclusive
        /// </summary>
        /// <param name="protocol">Protocol to use for the communication with the card</param>
        /// <param name="timeout">timeout (in ms) for the connection. If set the connection will be retried in case of
        /// other programs occupying the card reader (sharing violation) for the given amount of time</param>
        /// <param name="cancellationToken"/>
        Task ConnectAsync(Protocol protocol, int timeout, CancellationToken cancellationToken);


        /// <summary>
        /// Reconnects to the card performing a reset. The connection will be exclusive
        /// </summary>
        /// <param name="protocol">Protocol to use for the communication with the card</param>
        void ResetCard(Protocol protocol);

        /// <summary>
        /// Disconnect from the card
        /// </summary>
        /// <exception cref="SmartCardNotConnectedException">Thrown, if card was not connected</exception>
        void Disconnect();

        /// <summary>
        /// Sends the given command to the card.
        /// </summary>
        /// <exception cref="SmartCardNotConnectedException">Thrown, if card was not connected</exception>
        /// <exception cref="InvalidOperationException">Thrown, if the command contains variable data that needs to be resolved</exception>
        CardResponse Transmit(CardCommand command);

        /// <summary>
        /// Sends the given command to the card. May contain variable data (<see cref="IVariableResolver"/>)
        /// </summary>
        /// <exception cref="SmartCardNotConnectedException">Thrown, if card was not connected</exception>
        CardResponse Transmit(CardCommand command, IVariableResolver resolver);


        /// <summary>
        /// Sends the given command to the card.
        /// </summary>
        /// <exception cref="SmartCardNotConnectedException">Thrown, if card was not connected</exception>
        /// <exception cref="InvalidOperationException">Thrown, if the command contains variable data that needs to be resolved</exception>
        Task<CardResponse> TransmitAsync(CardCommand command);

        /// <summary>
        /// Sends the given command to the card. May contain variable data (<see cref="IVariableResolver"/>)
        /// </summary>
        /// <exception cref="SmartCardNotConnectedException">Thrown, if card was not connected</exception>
        Task<CardResponse> TransmitAsync(CardCommand command, IVariableResolver resolver);
    }
}