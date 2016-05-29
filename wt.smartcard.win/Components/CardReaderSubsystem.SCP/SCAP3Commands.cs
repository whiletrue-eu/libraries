using System;

namespace WhileTrue.Components.CardReaderSubsystem.SCP
{
    internal class Scap3Commands : ScapCommands
    {
        public override byte[] PowerOn(IntPtr sctp, bool reset)
        {
            if (reset)
            {
                return ScpCommands.DoIO(sctp, 0x01, new byte[] {0x04}, 0x82);
            }
            else
            {
                return ScpCommands.DoIO(sctp, 0x01, 0x82);
            }
        }

        public override byte[] SendApdu(IntPtr sctp, byte[] data)
        {
            byte[] Command = new byte[data.Length + 2];
            Array.Copy(data, 0, Command, 0, 5); //Copy header
            if (data.Length > 5)
            {
                //Data to send
                Command[5] = data[4]; //Set length of command data
                Array.Copy(data, 5, Command, 7, data.Length - 5); //Copy data
            }
            else
            {
                Command[6] = data[4]; //Set length of excpected data
            }

            byte[] Response = ScpCommands.DoIO(sctp, 0x04, Command, 0x82);

            byte[] ReturnData = new byte[Response.Length - 1];
            Array.Copy(Response, 3, ReturnData, 0, Response.Length - 3); //Copy response data
            Array.Copy(Response, 0, ReturnData, ReturnData.Length - 2, 2); //Copy status word

            return ReturnData;
        }
    }
}