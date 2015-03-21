using System.Collections;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.Principal;

namespace WhileTrue.Classes.Utilities
{
    public static class IpcChannelHelper
    {
        public static void RegisterIpcChannel(string name)
        {
            IDictionary ChannelProperties = new Hashtable();
            ChannelProperties["portName"] = name;
            ChannelProperties["tokenImpersonationLevel"] = TokenImpersonationLevel.Identification;
            ChannelProperties["includeVersions"] = false;
            ChannelProperties["strictBinding"] = false;
            ChannelProperties["secure"] = true;
            ChannelProperties["authorizedGroup"] = new SecurityIdentifier(WellKnownSidType.WorldSid, null ).Translate(typeof(NTAccount)).Value;

            BinaryServerFormatterSinkProvider ServerSink = new BinaryServerFormatterSinkProvider();
            ServerSink.TypeFilterLevel = TypeFilterLevel.Full;

            BinaryClientFormatterSinkProvider ClientSink = new BinaryClientFormatterSinkProvider();
            IpcChannel ServerChannel = new IpcChannel(ChannelProperties, ClientSink, ServerSink);
            ChannelServices.RegisterChannel(ServerChannel, true);
        }
    }
}