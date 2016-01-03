using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WhileTrue.Classes.SCard;
using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Components.CardReaderSubsystem.PCSC
{
    /// <summary/>
    internal class Orgamkt5CardReader : PcscCardReader
    {
        private readonly SCardApi scardApi;

        public Orgamkt5CardReader( PcscSmartCardSubsystem subsystem, SCardApi scardApi, string name)
            : base(subsystem, scardApi, name)
        {
            this.scardApi = scardApi;
        }

        protected override void ResolveVariable(Variable variable, IVariableResolver resolver)
        {
            if (variable.Format == VariableFormat.Ascii)
            {
                if (variable.VerifyEntry == false)
                {
                    string Message = Orgamkt5CardReader.LayoutDisplayMessage("Enter {0}\n(Len:{1}-{2}) ", variable.Length, variable.Name, variable.MinLength, variable.Length);
                    variable.Value = this.GetVariableValue(Message, variable, resolver);
                }
                else
                {
                    do
                    {
                        string Message = Orgamkt5CardReader.LayoutDisplayMessage("Enter {0}\n(Len:{1}-{2}) ", variable.Length, variable.Name, variable.MinLength, variable.Length);
                        byte[] FirstPin = this.GetVariableValue(Message, variable, resolver);
                        Message = Orgamkt5CardReader.LayoutDisplayMessage("Re-Enter {0}\n(Len:{1}-{2}) ", variable.Length, variable.Name, variable.MinLength, variable.Length);
                        byte[] SecondPin = this.GetVariableValue(Message, variable, resolver);

                        if (FirstPin.HasEqualValue(SecondPin))
                        {
                            variable.Value = FirstPin;
                        }
                        else
                        {
                            this.DisplayMessage(Orgamkt5CardReader.LayoutDisplayMessage("Error: Value mismatch\nPlease try again.", 0), TimeSpan.FromSeconds(5));
                        }
                    } while (true);
                }
            }
            else
            {
                base.ResolveVariable(variable, resolver);
            }
        }

        private byte[] GetVariableValue(string message, Variable variable, IVariableResolver resolver)
        {
            byte[] Data = null;
            do
            {
                CardCommand Command = new CardCommand();
                Command.Cla = 0x20;
                Command.Ins = 0x16;
                Command.P1 = 0x50; // Keyboard
                Command.P2 = 0x02; // Display '*'
                Command.AppendData(0x50, Encoding.ASCII.GetBytes(message)); //Display Message
                Command.AppendData(0x80, 0x60); //Timeout
                //For variable timeout use: Command.Append(0x80, (byte) (timeout.TotalSeconds <= 60 ? timeout.TotalSeconds : 60)); //Timeout
                Command.Le = 0x00;

                CardResponse Response;
                try
                {
                    resolver.NotifyVariableEntryBegins(variable.Name);
                    Response = this.SendCommandToReader(Command);
                }
                finally
                {
                    resolver.NotifyVariableEntryEnded();
                }

                switch (Response.Status)
                {
                    case 0x9000:
                        Data = Response.Data;
                        break;
                    case 0x6401:
                        throw new UserCancelException();
                    case 0x6410:
                        //TODO: auslagern in UIHandler
                        if (MessageBox.Show("The card readers pinpad is disabled.\n Please turn it on.", "Message", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            continue;
                        }
                        else
                        {
                            throw new UserCancelException();
                        }
                    case 0x6400:
                        //TODO: auslagern in UIHandler
                        if (MessageBox.Show("There was a timeout entering the PIN at the card reader.\nDo you want to retry?", "Question", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            continue;
                        }
                        else
                        {
                            throw new UserCancelException();
                        }
                    default:
                        throw new UnableToResolveVariableException(variable, $"Card reader returned unknown code 0x{Response.Status:4X}");
                }

                if (Data != null && (Data.Length < variable.MinLength || Data.Length > variable.Length))
                {
                    this.DisplayMessage(Orgamkt5CardReader.LayoutDisplayMessage("Pin entry error!\nPlease retry!", 0), TimeSpan.FromSeconds(2));
                    Data = null;
                }
            } while (Data == null);

            this.ClearDisplay();

            return Data;
        }

        private void DisplayMessage(string message, TimeSpan duration)
        {
            int Duration = (duration.TotalSeconds <= 60 ? (int) duration.TotalSeconds : 60);
            CardCommand Command = new CardCommand();
            Command.Cla = 0x20;
            Command.Ins = 0x17;
            Command.P1 = 0x40; // Display
            Command.P2 = 0x00;
            Command.AppendData(0x50, Encoding.ASCII.GetBytes(message)); //Display Message
            if (duration != TimeSpan.Zero)
            {
                Command.AppendData(0x80, 0x60); //timeout: take 60: the real timeout is performed by 'sleep'
            }

            this.SendCommandToReader(Command);
            Thread.Sleep(Duration*1000);
        }

        private void ClearDisplay()
        {
            CardCommand Command = new CardCommand();
            Command.Cla = 0x20;
            Command.Ins = 0x17;
            Command.P1 = 0x40; // Display
            Command.P2 = 0x00;
            Command.AppendData(0x50, 0x00); //Display empty message (->reset to default message)

            this.SendCommandToReader(Command);
        }

        private CardResponse SendCommandToReader(CardCommand command)
        {
            // Write command to a buffer that contains a SCARD_CONTROL_REQUEST header needed by the MKT
            byte[] SerializedCommand = command.Serialize();
            byte[] ReaderCommand = new byte[20 + SerializedCommand.Length];
            ReaderCommand[0] = 0x01; //int value!
            ReaderCommand[4] = 0x02; //int value!
            ReaderCommand[8] = (byte) (SerializedCommand.Length); //int value!
            ReaderCommand[13] = 4;
            Array.Copy(SerializedCommand, 0, ReaderCommand, 20, SerializedCommand.Length);

            //Call card reader
            byte[] ReaderResponse = this.scardApi.ControlCardReader(this.CardHandle, ReaderCommand);

            //Extract Response (strip header)
            byte[] SerializedResponse = new byte[ReaderResponse.Length - 20];
            Array.Copy(ReaderResponse, 20, SerializedResponse, 0, SerializedResponse.Length);
            return new CardResponse(SerializedResponse);
        }

        private static string LayoutDisplayMessage(string format, byte reserveCharsInLine2, params object[] parameter)
        {
            string Line1Format;
            string Line2Format;
            if (format.IndexOf('\n') == -1)
            {
                Line1Format = format;
                Line2Format = "";
            }
            else
            {
                Line1Format = format.Substring(0, format.IndexOf('\n'));
                Line2Format = format.Substring(format.IndexOf('\n') + 1);
            }

            string Line1 = string.Format(Line1Format, parameter);
            string Line2 = string.Format(Line2Format, parameter);

            Line1 = Line1.PadRight(20, ' ').Substring(0, 20);
            Line2 = Line2.PadRight(20, ' ').Substring(0, 20 - reserveCharsInLine2);

            return Line1 + Line2;
        }
    }
}