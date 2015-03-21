using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WhileTrue.Classes.SCard
{
    public class SCardAPI : IDisposable
    {
        private IntPtr context;

        // ReSharper disable InconsistentNaming
        private static readonly IntPtr SCARD_PCI_RAW;
        private static readonly IntPtr SCARD_PCI_T0;
        private static readonly IntPtr SCARD_PCI_T1;
        // ReSharper restore InconsistentNaming
        private static readonly Hashtable sessionProtocols = new Hashtable();

        private IntPtr Context
        {
            get
            {
                if (SCardAPI.SCardIsValidContext(this.context) == SCardError.NoError)
                {
                    return this.context;
                }
                else
                {
                    SCardError Result = SCardEstablishContext(SCardScope.System, IntPtr.Zero, IntPtr.Zero, out this.context);
                    if( SCardAPI.IsError(Result) )
                    {
                        throw new SCardException(Result);
                    }
                    return this.context;
                }
            }
        }

        static SCardAPI()
        {
            IntPtr Lib = LoadLibrary("winscard.dll");
            SCARD_PCI_T0 = GetProcAddress(Lib, "g_rgSCardT0Pci");
            SCARD_PCI_T1 = GetProcAddress(Lib, "g_rgSCardT1Pci");
            SCARD_PCI_RAW = GetProcAddress(Lib, "g_rgSCardRawPci");
            FreeLibrary(Lib);
        }

        // LONG SCardEstablishContext( DWORD dwScope, LPCVOID pvReserved1, LPCVOID pvReserved2, LPSCARDCONTEXT phContext );
        [DllImport("winscard.dll", EntryPoint = "SCardEstablishContext")]
        private static extern SCardError SCardEstablishContext(SCardScope scope, IntPtr reserved1, IntPtr reserved2, out IntPtr context);

        // LONG	SCardReleaseContext( SCARDCONTEXT hContext );
        [DllImport("winscard.dll", EntryPoint = "SCardReleaseContext")]
        private static extern SCardError SCardReleaseContext(IntPtr context);

        // LONG WINAPI SCardIsValidContext(__in  SCARDCONTEXT hContext);
        [DllImport("winscard.dll", EntryPoint = "SCardIsValidContext")]
        private static extern SCardError SCardIsValidContext(IntPtr context);


        // LONG SCardListCardReaders( SCARDCONTEXT hContext, LPCTSTR mszGroups, LPTSTR mszCardReaders, LPDWORD pcchCardReaders );
        [DllImport("winscard.dll", EntryPoint = "SCardListReaders")]
        private static extern SCardError SCardListReaders(IntPtr context, char[] groups, [In, Out] char[] mszCardReaders, ref uint pcchCardReaders);

        // LONG SCardGetStatusChange( SCARDCONTEXT hContext, DWORD dwTimeout, LPSCARD_READERSTATEW rgReaderStates, DWORD cReaders);
        [DllImport("winscard.dll", EntryPoint = "SCardGetStatusChange", CharSet = CharSet.Auto)]
        private static extern SCardError SCardGetStatusChange(IntPtr hContext, int dwTimeout, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] SCardCardReaderState[] rgCardReaderState,
                                                                 int cCardReaders);

        // LONG SCardConnect( SCARDCONTEXT hContext, LPCWSTR szReader, DWORD dwShareMode, DWORD dwPreferredProtocols, LPSCARDHANDLE phCard, LPDWORD pdwActiveProtocol);
        [DllImport("winscard.dll", EntryPoint = "SCardConnectW", CharSet = CharSet.Unicode)]
        private static extern SCardError SCardConnect(IntPtr hContext, string szReader, SCardShareMode dwShareMode, SCardProtocol dwPreferredProtocols, [Out] out IntPtr phCard,
                                                         [Out] out SCardProtocol pdwActiveProtocol);      
        
        // LONG SCardReconnect( SCARDHANDLE hCard, DWORD dwShareMode, DWORD dwPreferredProtocols, DWORD dwInitialization, LPDWORD pdwActiveProtocol);
        [DllImport("winscard.dll", EntryPoint = "SCardReconnect", CharSet = CharSet.Unicode)]
        private static extern SCardError SCardReconnect(IntPtr hCard, SCardShareMode dwShareMode, SCardProtocol dwPreferredProtocols, SCardDisposition dwDisposition, [Out] out SCardProtocol pdwActiveProtocol);

        // LONG SCardDisconnect( SCARDHANDLE hCard, DWORD dwDisposition);
        [DllImport("winscard.dll", EntryPoint = "SCardDisconnect", CharSet = CharSet.Auto)]
        private static extern SCardError SCardDisconnect(IntPtr hCard, SCardDisposition dwDisposition);

        // LONG SCardTransmit( SCARDHANDLE hCard, LPCSCARD_IO_REQUEST pioSendPci, LPCBYTE pbSendBuffer, DWORD cbSendLength, LPSCARD_IO_REQUEST pioRecvPci, LPBYTE pbRecvBuffer, LPDWORD pcbRecvLength)
        [DllImport("winscard.dll", EntryPoint = "SCardTransmit", CharSet = CharSet.Auto)]
        private static extern SCardError SCardTransmit(IntPtr hCard, IntPtr pioSendPci, byte[] pbSendBuffer, uint cbSendLength, [In, Out] IntPtr pioRecvPci, [In, Out] byte[] pbRecvBuffer,
                                                          [In, Out] ref uint pcbRecvLength);

        // LONG SCardGetAttrib(	SCARDHANDLE hCard, DWORD dwAttrId, LPBYTE pbAttr, LPDWORD pcbAttrLen );
        [DllImport("winscard.dll", EntryPoint = "SCardGetAttrib", CharSet = CharSet.Auto)]
        private static extern SCardError SCardGetAttrib(IntPtr hCard, uint dwAttrId, [In, Out] byte[] pbAttr, ref uint pcbAttrLen);

        // LONG SCardControl( SCARDHANDLE hCard, DWORD dwControlCode, LPCVOID lpInBuffer, DWORD nInBufferSize, LPVOID lpOutBuffer, DWORD nOutBufferSize, LPDWORD lpBytesReturned );
        [DllImport("winscard.dll", EntryPoint = "SCardControl", CharSet = CharSet.Auto)]
        private static extern SCardError SCardControl(IntPtr hCard, uint dwControlCode, [In, Out] byte[] lpInBuffer, int nInBufferSize, [In, Out] byte[] lpOutBuffer, int nOutBufferSize,
                                                         ref int lpBytesReturned);


        //LONG SCardCancel( SCARDCONTEXT hContext );
        [DllImport("winscard.dll", EntryPoint = "SCardCancel", CharSet = CharSet.Auto)]
        private static extern SCardError SCardCancel(IntPtr hContext);

        private static string[] ToStringArray(char[] data)
        {
            char[] Data = new char[data.Length - 2];
            Array.Copy(data, 0, Data, 0, data.Length - 2);
            for (int Index = 0; Index < Data.Length - 2; Index++)
            {
                if (Data[Index] == '\0')
                {
                    Data[Index] = '\n';
                }
            }
            string CardReaders = new string(Data);
            return CardReaders.Split('\n');
        }

        public string[] ListReaders()
        {
            uint CardReadersSize = 0;
            char[] CardReaders = null;

            SCardError Error = SCardListReaders(Context, null, null, ref CardReadersSize);
            if (!IsError(Error))
            {
                CardReaders = new char[CardReadersSize];
                Error = SCardListReaders(Context, null, CardReaders, ref CardReadersSize);
            }

            if (IsError(Error))
            {
                switch (Error)
                {
                    case SCardError.NoReadersAvailable:
                        return new string[0];
                    default:
                        throw new SCardException(Error);
                }
            }
            else
            {
                return ToStringArray(CardReaders);
            }
        }

        /// <summary>
        /// Waits for status change on the given list of card readers. 
        /// If cancelled by a call to <see cref="Cancel"/> or due to timeout, it returns <c>false</c>, otherwise <c>true</c>
        /// </summary>
        public bool GetStatusChange( int dwTimeout, SCardCardReaderState[] cardReaderStates )
        {
            SCardError Error = SCardGetStatusChange(Context, dwTimeout, cardReaderStates, cardReaderStates.Length);
            if( Error == SCardError.Cancelled || Error == SCardError.Timeout)
            {
                return false;
            }
            else if (IsError(Error))
            {
                throw new SCardException(Error);
            }
            else
            {
                return true;
            }
        }

        public IntPtr Connect(string readerName, SCardShareMode shareMode, SCardProtocol protocol)
        {
            IntPtr CardHandle;
            SCardProtocol ActiveProtocol;
            SCardError Error = SCardConnect(Context, readerName, shareMode, protocol, out CardHandle, out ActiveProtocol);
            if (IsError(Error))
            {
                throw new SCardException(Error);
            }

            sessionProtocols.Add(CardHandle, protocol);
            return CardHandle;
        }

        public void Reconnect(IntPtr cardHandle, SCardDisposition disposition, SCardShareMode shareMode, SCardProtocol protocol)
        {
            if( disposition == SCardDisposition.Eject )
            {
                throw new ArgumentException("Disposition.Eject not possible in reconnect", "disposition");
            }
            SCardProtocol ActiveProtocol;
            SCardError Error = SCardReconnect(cardHandle, shareMode, protocol, disposition, out ActiveProtocol);
            if (IsError(Error))
            {
                throw new SCardException(Error);
            }

            sessionProtocols.Remove(cardHandle);
            sessionProtocols.Add(cardHandle, protocol);
        }

        public void Disconnect(IntPtr cardHandle, SCardDisposition dwDisposition)
        {
            SCardError Error = SCardDisconnect(cardHandle, dwDisposition);
            if (IsError(Error))
            {
                throw new SCardException(Error);
            }

            sessionProtocols.Remove(cardHandle);
        }

        public byte[] Transmit(IntPtr hCard, byte[] pbSendBuffer)
        {
            Trace.Assert(sessionProtocols.ContainsKey(hCard));
            IntPtr SendPci = IntPtr.Zero;
            IntPtr ResponsePci = IntPtr.Zero;
            uint ResponseBufferLength = 512;
            byte[] ResponseBuffer = new byte[ResponseBufferLength];
            switch ((SCardProtocol) sessionProtocols[hCard])
            {
                case SCardProtocol.T0:
                    SendPci = SCARD_PCI_T0;
                    break;
                case SCardProtocol.T1:
                    SendPci = SCARD_PCI_T1;
                    break;
                case SCardProtocol.Raw:
                    SendPci = SCARD_PCI_RAW;
                    break;
            }

            SCardError Error = SCardTransmit(hCard, SendPci, pbSendBuffer, (uint) pbSendBuffer.Length, ResponsePci, ResponseBuffer, ref ResponseBufferLength);
            if (IsError(Error))
            {
                throw new SCardException(Error);
            }

            byte[] Response = new byte[ResponseBufferLength];
            Array.Copy(ResponseBuffer, 0, Response, 0, ResponseBufferLength);

            return Response;
        }

        public byte[] GetAttribute(IntPtr hCard, SCardAttributes attribute)
        {
            byte[] Attribute = null;
            uint AttributeLength = 0;
            SCardError Error = SCardGetAttrib(hCard, (uint) attribute, Attribute, ref AttributeLength);
            if( Error == (SCardError) 22 )
            {
                return null;
            }
            if (IsError(Error))
            {
                throw new SCardException(Error);
            }

            Attribute = new byte[AttributeLength];

            Error = SCardGetAttrib(hCard, (uint) attribute, Attribute, ref AttributeLength);
            if (IsError(Error))
            {
                throw new SCardException(Error);
            }

            return Attribute;
        }

        public byte[] ControlCardReader(IntPtr hCard, byte[] command)
        {
            byte[] Response = new byte[1024];
            int ResponseLength = 0;
            SCardError Error = SCardControl(hCard, 0x00312000, command, command.Length, Response, Response.Length, ref ResponseLength);
            if (IsError(Error))
            {
                throw new SCardException(Error);
            }

            byte[] RealResponse = new byte[ResponseLength];
            Array.Copy(Response, 0, RealResponse, 0, ResponseLength);

            return RealResponse;
        }

        public void Cancel()
        {
            SCardCancel(Context);
        }

        private static bool IsError(SCardError error)
        {
            return error != SCardError.NoError;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string libName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr module);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr module, string export);

        public void Dispose()
        {
            if (this.Context != IntPtr.Zero)
            {
                SCardAPI.SCardReleaseContext(Context);
            }
        }
    }
}