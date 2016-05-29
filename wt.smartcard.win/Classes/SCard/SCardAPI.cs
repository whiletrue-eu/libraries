using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WhileTrue.Classes.SCard
{
    public class SCardApi : IDisposable
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
                if (SCardApi.SCardIsValidContext(this.context) == SCardError.NoError)
                {
                    return this.context;
                }
                else
                {
                    SCardError Result = SCardApi.SCardEstablishContext(SCardScope.System, IntPtr.Zero, IntPtr.Zero, out this.context);
                    if( SCardApi.IsError(Result) )
                    {
                        throw new SCardException(Result);
                    }
                    return this.context;
                }
            }
        }

        static SCardApi()
        {
            IntPtr Lib = SCardApi.LoadLibrary("winscard.dll");
            SCardApi.SCARD_PCI_T0 = SCardApi.GetProcAddress(Lib, "g_rgSCardT0Pci");
            SCardApi.SCARD_PCI_T1 = SCardApi.GetProcAddress(Lib, "g_rgSCardT1Pci");
            SCardApi.SCARD_PCI_RAW = SCardApi.GetProcAddress(Lib, "g_rgSCardRawPci");
            SCardApi.FreeLibrary(Lib);
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

            SCardError Error = SCardApi.SCardListReaders(this.Context, null, null, ref CardReadersSize);
            if (!SCardApi.IsError(Error))
            {
                CardReaders = new char[CardReadersSize];
                Error = SCardApi.SCardListReaders(this.Context, null, CardReaders, ref CardReadersSize);
            }

            if (SCardApi.IsError(Error))
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
                return SCardApi.ToStringArray(CardReaders);
            }
        }

        /// <summary>
        /// Waits for status change on the given list of card readers. 
        /// If cancelled by a call to <see cref="Cancel"/> or due to timeout, it returns <c>false</c>, otherwise <c>true</c>
        /// </summary>
        public bool GetStatusChange( int dwTimeout, SCardCardReaderState[] cardReaderStates )
        {
            SCardError Error = SCardApi.SCardGetStatusChange(this.Context, dwTimeout, cardReaderStates, cardReaderStates.Length);
            if( Error == SCardError.Cancelled || Error == SCardError.Timeout)
            {
                return false;
            }
            else if (SCardApi.IsError(Error))
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
            SCardError Error = SCardApi.SCardConnect(this.Context, readerName, shareMode, protocol, out CardHandle, out ActiveProtocol);
            if (SCardApi.IsError(Error))
            {
                throw new SCardException(Error);
            }

            SCardApi.sessionProtocols.Add(CardHandle, protocol);
            return CardHandle;
        }

        public void Reconnect(IntPtr cardHandle, SCardDisposition disposition, SCardShareMode shareMode, SCardProtocol protocol)
        {
            if( disposition == SCardDisposition.Eject )
            {
                throw new ArgumentException("Disposition.Eject not possible in reconnect", nameof(disposition));
            }
            SCardProtocol ActiveProtocol;
            SCardError Error = SCardApi.SCardReconnect(cardHandle, shareMode, protocol, disposition, out ActiveProtocol);
            if (SCardApi.IsError(Error))
            {
                throw new SCardException(Error);
            }

            SCardApi.sessionProtocols.Remove(cardHandle);
            SCardApi.sessionProtocols.Add(cardHandle, protocol);
        }

        public void Disconnect(IntPtr cardHandle, SCardDisposition dwDisposition)
        {
            SCardError Error = SCardApi.SCardDisconnect(cardHandle, dwDisposition);
            if (SCardApi.IsError(Error))
            {
                throw new SCardException(Error);
            }

            SCardApi.sessionProtocols.Remove(cardHandle);
        }

        public byte[] Transmit(IntPtr hCard, byte[] pbSendBuffer)
        {
            Trace.Assert(SCardApi.sessionProtocols.ContainsKey(hCard));
            IntPtr SendPci = IntPtr.Zero;
            IntPtr ResponsePci = IntPtr.Zero;
            uint ResponseBufferLength = 512;
            byte[] ResponseBuffer = new byte[ResponseBufferLength];
            switch ((SCardProtocol) SCardApi.sessionProtocols[hCard])
            {
                case SCardProtocol.T0:
                    SendPci = SCardApi.SCARD_PCI_T0;
                    break;
                case SCardProtocol.T1:
                    SendPci = SCardApi.SCARD_PCI_T1;
                    break;
                case SCardProtocol.Raw:
                    SendPci = SCardApi.SCARD_PCI_RAW;
                    break;
            }

            SCardError Error = SCardApi.SCardTransmit(hCard, SendPci, pbSendBuffer, (uint) pbSendBuffer.Length, ResponsePci, ResponseBuffer, ref ResponseBufferLength);
            if (SCardApi.IsError(Error))
            {
                throw new SCardException(Error);
            }

            byte[] Response = new byte[ResponseBufferLength];
            Array.Copy(ResponseBuffer, 0, Response, 0, ResponseBufferLength);

            return Response;
        }

        public byte[] GetAttribute(IntPtr hCard, SCardAttributes attribute)
        {
            uint AttributeLength = 0;
            SCardError Error = SCardApi.SCardGetAttrib(hCard, (uint) attribute, null, ref AttributeLength);
            if( Error == (SCardError) 22 )
            {
                return null;
            }
            if (SCardApi.IsError(Error))
            {
                throw new SCardException(Error);
            }

            byte[] Attribute = new byte[AttributeLength];

            Error = SCardApi.SCardGetAttrib(hCard, (uint) attribute, Attribute, ref AttributeLength);
            if (SCardApi.IsError(Error))
            {
                throw new SCardException(Error);
            }

            return Attribute;
        }

        public byte[] ControlCardReader(IntPtr hCard, byte[] command)
        {
            byte[] Response = new byte[1024];
            int ResponseLength = 0;
            SCardError Error = SCardApi.SCardControl(hCard, 0x00312000, command, command.Length, Response, Response.Length, ref ResponseLength);
            if (SCardApi.IsError(Error))
            {
                throw new SCardException(Error);
            }

            byte[] RealResponse = new byte[ResponseLength];
            Array.Copy(Response, 0, RealResponse, 0, ResponseLength);

            return RealResponse;
        }

        public void Cancel()
        {
            SCardApi.SCardCancel(this.Context);
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
                SCardApi.SCardReleaseContext(this.Context);
            }
        }
    }
}