using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace WhileTrue.Components.CardReaderSubsystem.SCP
{
    internal class ScpCommands
    {
        #region CardStatus enum

        public enum CardStatus : byte
        {
            Unknown = 255,
            NoCard = 0,
            CardInsertedPowerOff = 1,
            CardInsertedPowerOn = 2,
        }

        #endregion

        private static readonly Hashtable scapsByHandle = new Hashtable();

        private ScpCommands()
        {
            //Do not instanciate
        }

        // extern SCTP * sccp_open ( int receiver, int module_no, int address_id, int reset_mode );
        /// <summary>
        /// Open connection. The function allocates a SCTP structure and then calls the sccp_init function
        /// e.g. sccp_open ( 0x31, 0, 0x3f8, 0)
        /// </summary>
        /// <param name="receiver">SCP address of the slave</param>
        /// <param name="moduleNo">0: COM, 4: PCR 320</param>
        /// <param name="addressId">hardware base address</param>
        /// <param name="resetMode">0: no</param>
        [DllImport("scpclientdll.dll", EntryPoint = "sccp_open", CharSet = CharSet.Auto)]
        private static extern IntPtr sccp_open(int receiver, int moduleNo, int addressId, int resetMode);

        //extern void sccp_close (SCTP * sctp);
        [DllImport("scpclientdll.dll", EntryPoint = "sccp_close", CharSet = CharSet.Auto)]
        private static extern void sccp_close(IntPtr sctp);

        //extern sccp_init ( SCTP * SCP_DLL_FAR sctp, int receiver, int module_no,	int address_id,	int reset_mode );
        /// <summary>
        /// Initialise connection, e.g. sccp_init (&amp;sctp, 0x31, 0, 0x3f8, 0)
        /// </summary>
        /// <param name="sctp">pointer to SCTP structure</param>
        /// <param name="receiver">SCP address of the slave</param>
        /// <param name="moduleNo">0: COM, 4: PCR 320</param>
        /// <param name="addressId">hardware base address</param>
        /// <param name="resetMode">0: no</param>
        [DllImport("scpclientdll.dll", EntryPoint = "sccp_init", CharSet = CharSet.Auto)]
        private static extern LowLevelError sccp_init(IntPtr sctp, int receiver, int moduleNo, int addressId, int resetMode);


        /** --------------------------------------------------------------------- **
		 **         un initialise
		 ** --------------------------------------------------------------------- **/

        // extern void sccp_uninit (SCTP * sctp);
        [DllImport("scpclientdll.dll", EntryPoint = "sccp_uninit", CharSet = CharSet.Auto)]
        private static extern void sccp_uninit(IntPtr sctp);

        // extern  sccp_select_scap ( SCTP * SCP_DLL_FAR sctp, int application_id, int * SCP_DLL_FAR ret, unsigned char * SCP_DLL_FAR rblock, unsigned * SCP_DLL_FAR rlen );
        /// <summary>
        /// Select application
        /// </summary>
        /// <param name="sctp">pointer to SCTP structure</param>
        /// <param name="applicationId">Application number which is defined in the SCAP description</param>
        /// <param name="ret">return code</param>
        /// <param name="rblock">response data</param>
        /// <param name="rlen">length of response data</param>
        /// <returns></returns>
        [DllImport("scpclientdll.dll", EntryPoint = "sccp_select_scap", CharSet = CharSet.Auto)]
        private static extern LowLevelError sccp_select_scap(IntPtr sctp, int applicationId, out int ret, [In, Out] byte[] rblock, ref int rlen);

        // extern  SCP_DLL_API_ATTR sccp_io ( SCTP * SCP_DLL_FAR sctp, int com, unsigned char * SCP_DLL_FAR wblock, unsigned wlen, int * SCP_DLL_FAR ret, unsigned char * SCP_DLL_FAR rblock, unsigned * SCP_DLL_FAR rlen );
        /// <summary>
        /// Execute application command
        /// </summary>
        /// <param name="sctp">pointer to SCTP structure</param>
        /// <param name="com">command (defined in the SCAP description)</param>
        /// <param name="wblock">request data</param>
        /// <param name="wlen">length of request data</param>
        /// <param name="ret">return code</param>
        /// <param name="rblock">response data</param>
        /// <param name="rlen">length of response data</param>
        /// <returns></returns>
        [DllImport("scpclientdll.dll", EntryPoint = "sccp_io", CharSet = CharSet.Auto)]
        private static extern LowLevelError sccp_io(IntPtr sctp, int com, byte[] wblock, int wlen, ref int ret, [In, Out] byte[] rblock, ref int rlen);


        public static IntPtr Open(int port)
        {
            IntPtr Sctp = ScpCommands.sccp_open(0x31, 0, /*port*/port, 0);
            if (Sctp == IntPtr.Zero)
            {
                throw new ScpException(LowLevelError.NoCardReader);
            }
            return Sctp;
        }

        public static void Close(IntPtr sctp)
        {
            ScpCommands.sccp_close(sctp);
            ScpCommands.scapsByHandle.Remove(sctp);
        }

        public static Size InitDisplay(IntPtr sctp)
        {
            byte[] Response = ScpCommands.DoIO(sctp, 0xF8, new byte[] {0x00, 0x00}, 0xF0);
            return new Size(Response[1], Response[0]);
        }

        public static void SelectApplicationProtocol(IntPtr sctp, Scap scap)
        {
            ScpCommands.DoIO(sctp, 0xF1, (byte) scap, new[] {0x80, 0x81, 0x82});

            ScapCommands Scap;
            switch (scap)
            {
                case SCP.Scap.T0:
                    Scap = new Scap3Commands();
                    break;
                    /*case SCAP.T1:
					Scap = new SCAPxCommands();
					break;*/
                default:
                    throw new ScpException(LowLevelError.CardNotSupported);
            }

            ScpCommands.scapsByHandle[sctp] = Scap;
        }


        public static CardStatus GetStatus(IntPtr sctp)
        {
            byte[] Response = ScpCommands.DoIO(sctp, 0xF8, new byte[] {0x04, 0x00}, 0xF0);
            if (Response[0] == 0x00)
            {
                return CardStatus.NoCard;
            }
            else if (Response[0] == 0x01)
            {
                return CardStatus.CardInsertedPowerOff;
            }
            else if (Response[0] == 0x02)
            {
                return CardStatus.CardInsertedPowerOn;
            }
            else
            {
                return CardStatus.Unknown;
            }
        }

        public static void PowerOff(IntPtr sctp)
        {
            ScpCommands.DoIO(sctp, 0xF8, new byte[] {0x04, 0x02}, 0xF0);
        }

        public static byte[] PowerOn(IntPtr sctp, bool reset)
        {
            if (ScpCommands.scapsByHandle.Contains(sctp))
            {
                return ((ScapCommands) ScpCommands.scapsByHandle[sctp]).PowerOn(sctp, reset);
            }
            else
            {
                throw new Exception("No application selected");
            }
        }

        public static byte[] SendApdu(IntPtr sctp, byte[] data)
        {
            if (ScpCommands.scapsByHandle.Contains(sctp))
            {
                return ((ScapCommands) ScpCommands.scapsByHandle[sctp]).SendApdu(sctp, data);
            }
            else
            {
                throw new Exception("No application selected");
            }
        }


        private static void HideCursor(IntPtr sctp)
        {
            Size Size = ScpCommands.InitDisplay(sctp);
            ScpCommands.SetCursor(sctp, new Point(Size.Width, 0));
        }

        public static void SetCursor(IntPtr sctp, Point position)
        {
            byte[] Command = new byte[4];

            Command[0] = 0x00; //Display commands
            Command[1] = 0x02; //set cursor position
            Command[2] = (byte) position.Y;
            Command[3] = (byte) position.X;

            ScpCommands.DoIO(sctp, 0xF8, Command, 0xF0);
        }

        public static Point GetCursor(IntPtr sctp)
        {
            byte[] Command = new byte[2];

            Command[0] = 0x00; //Display commands
            Command[1] = 0x04; //get cursor position

            byte[] Response = ScpCommands.DoIO(sctp, 0xF8, Command, 0xF0);

            return new Point(Response[1], Response[0]);
        }

        public static void Display(IntPtr sctp, string message)
        {
            Size Size = ScpCommands.InitDisplay(sctp);
            string[] Strings = (message + new string('\n', Size.Height)).Split('\n');
            ScpCommands.HideCursor(sctp);

            for (byte RowIndex = 0; RowIndex < Size.Height; RowIndex++)
            {
                string Message = Strings[RowIndex];
                Message.PadRight(Size.Width, ' ');

                ScpCommands.Display(sctp, RowIndex, Message);
            }
        }

        public static void WriteOnDisplay(IntPtr sctp, string message)
        {
            byte[] Message = Encoding.ASCII.GetBytes(message);
            byte[] Command = new byte[Message.Length + 3];

            Command[0] = 0x00; //Display commands
            Command[1] = 0x03; //Display text at cursor
            Array.Copy(Message, 0, Command, 2, Message.Length);

            ScpCommands.DoIO(sctp, 0xF8, Command, 0xF0);
        }

        public static void ClearDisplay(IntPtr sctp)
        {
            ScpCommands.HideCursor(sctp);
        }

        private static void Display(IntPtr sctp, byte row, string message)
        {
            byte[] Message = Encoding.ASCII.GetBytes(message);
            byte[] Command = new byte[Message.Length + 4];

            Command[0] = 0x00; //Display commands
            Command[1] = 0x01; //Display text
            Command[2] = row;
            Array.Copy(Message, 0, Command, 3, Message.Length);

            ScpCommands.DoIO(sctp, 0xF8, Command, 0xF0);
        }

        public static string GetVersionString(IntPtr sctp)
        {
            byte[] Response = ScpCommands.DoIO(sctp, 0xF6, 0xF0);
            string VersionString = new string(Encoding.ASCII.GetChars(Response));
            Regex VersionStringExpression = new Regex(@"^(?<Version>[VX]\d\.\d\d) (?<Id>.*?) \((?<Text>.*?)\) (?<Copyright>.*?)$");

            if (VersionStringExpression.IsMatch(VersionString))
            {
                return VersionStringExpression.Match(VersionString).Groups["Text"].Value;
            }
            else
            {
                return "unknown SCP device";
            }
        }

        public static string GetVersionString(IntPtr sctp, Scap scap)
        {
            byte[] Response = ScpCommands.DoIO(sctp, 0xF6, new[] {(byte) scap}, 0xF0);
            return new string(Encoding.ASCII.GetChars(Response));
        }

        private static void CheckError(LowLevelError error)
        {
            if (error != LowLevelError.NoError)
            {
                throw new ScpException(error);
            }
        }

        public static int InitKeyboard(IntPtr sctp)
        {
            byte[] Response = ScpCommands.DoIO(sctp, 0xF8, new byte[] {0x01, 0x00}, 0xF0);
            return Response[0];
        }

        public static void StartInput(IntPtr sctp, byte minLength, byte length)
        {
            ScpCommands.InitKeyboard(sctp);
            byte[] Command = new byte[9];

            Command[0] = 0x01; //keyboard commands
            Command[1] = 0x01; //init buffer
            Command[2] = 0x00; //buffer no
            Command[3] = 0x00; //no security lock
            Command[4] = minLength;
            Command[5] = length;
            Command[6] = 0x23; //# character as 'confirm'
            Command[7] = 0x2a; //* character as 'backspace'
            Command[8] = 0x2a; //* character as echo char

            ScpCommands.DoIO(sctp, 0xF8, Command, 0xF0);
        }

        public static bool GetInputHasEnded(IntPtr sctp)
        {
            byte[] Command = new byte[3];

            Command[0] = 0x01; //keyboard commands
            Command[1] = 0x02; //check keyboard status

            byte[] Response = ScpCommands.DoIO(sctp, 0xF8, Command, new[] {0xF0, 0xFF});

            return Response == null;
        }

        public static void AbortInput(IntPtr sctp)
        {
            byte[] Command = new byte[3];

            Command[0] = 0x01; //keyboard commands
            Command[1] = 0x03; //stop input

            ScpCommands.DoIO(sctp, 0xF8, Command, 0xF0);
        }

        public static byte[] EndInput(IntPtr sctp)
        {
            byte[] Command = new byte[3];

            Command[0] = 0x01; //keyboard commands
            Command[1] = 0x04; //read buffer
            Command[2] = 0x00; //buffer no

            return ScpCommands.DoIO(sctp, 0xF8, Command, 0xF0);
        }

        public static byte[] DoIO(IntPtr sctp, int command, int excpectedReturnCode)
        {
            return ScpCommands.DoIO(sctp, command, new[] {excpectedReturnCode});
        }

        public static byte[] DoIO(IntPtr sctp, int command, int[] excpectedReturnCodes)
        {
            return ScpCommands.DoIO(sctp, command, new byte[0], excpectedReturnCodes);
        }

        public static byte[] DoIO(IntPtr sctp, int command, byte data, int excpectedReturnCode)
        {
            return ScpCommands.DoIO(sctp, command, data, new[] {excpectedReturnCode});
        }

        public static byte[] DoIO(IntPtr sctp, int command, byte data, int[] excpectedReturnCodes)
        {
            return ScpCommands.DoIO(sctp, command, new[] {data}, excpectedReturnCodes);
        }

        public static byte[] DoIO(IntPtr sctp, int command, byte[] data, int excpectedReturnCode)
        {
            return ScpCommands.DoIO(sctp, command, data, new[] {excpectedReturnCode});
        }

        public static byte[] DoIO(IntPtr sctp, int command, byte[] data, int[] excpectedReturnCodes)
        {
            int ReturnCode = 0;
            byte[] ResponseBuffer = new byte[512];
            int ResponseLength = ResponseBuffer.Length;

            LowLevelError Error = ScpCommands.sccp_io(sctp, command, data, data.Length, ref ReturnCode, ResponseBuffer, ref ResponseLength);
            ScpCommands.CheckError(Error);

            bool ExpectedReturnCodeFound = false;
            foreach (int ExceptedReturnCode in excpectedReturnCodes)
            {
                if (ReturnCode == ExceptedReturnCode)
                {
                    ExpectedReturnCodeFound = true;
                    break;
                }
            }
            if (ExpectedReturnCodeFound == false)
            {
                throw new ScpException((LowLevelError) ReturnCode);
            }

            if (ResponseLength > 0)
            {
                byte[] ResponseData = new byte[ResponseLength];
                Array.Copy(ResponseBuffer, 0, ResponseData, 0, ResponseLength);

                return ResponseData;
            }
            else
            {
                return null;
            }
        }
    }
}