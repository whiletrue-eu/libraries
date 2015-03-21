// ReSharper disable UnusedMember.Global
using System;

namespace WhileTrue.Classes.SCard
{
    [Flags]
    public enum SCardReaderState : uint
    {
        // The application is unaware of the
        // current state, and would like to
        // know.  The use of this value
        // results in an immediate return
        // from state transition monitoring
        // services.  This is represented by
        // all bits set to zero.
        Unaware = 0x00000000,
        // The application requested that
        // this CardReader be ignored.  No other
        // bits will be set.
        Ignore = 0x00000001,
        // This implies that there is a
        // difference between the state
        // believed by the application, and
        // the state known by the Service
        // Manager.  When this bit is set,
        // the application may assume a
        // significant state change has
        // occurred on this CardReader.
        Changed = 0x00000002,
        // This implies that the given
        // CardReader name is not recognized by
        // the Service Manager.  If this bit
        // is set, then SCARD_STATE_CHANGED
        // and SCARD_STATE_IGNORE will also
        // be set.
        Unknown = 0x00000004,
        // This implies that the actual
        // state of this CardReader is not
        // available.  If this bit is set,
        // then all the following bits are
        // clear.
        Unavailable = 0x00000008,
        // This implies that there is not
        // card in the CardReader.  If this bit
        // is set, all the following bits
        // will be clear.
        Empty = 0x00000010,
        // This implies that there is a card
        // in the CardReader.
        Present = 0x00000020,
        // This implies that there is a card
        // in the CardReader with an ATR
        // matching one of the target cards.
        // If this bit is set,
        // SCARD_STATE_PRESENT will also be
        // set.  This bit is only returned
        // on the SCardLocateCard() service.
        AtrMatch = 0x00000040,
        // This implies that the card in the
        // CardReader is allocated for exclusive
        // use by another application.  If
        // this bit is set,
        // SCARD_STATE_PRESENT will also be
        // set.
        Exclusive = 0x00000080,
        // This implies that the card in the
        // CardReader is in use by one or more
        // other applications, but may be
        // connected to in shared mode.  If
        // this bit is set,
        // SCARD_STATE_PRESENT will also be
        // set.
        InUse = 0x00000100,
        // This implies that the card in the
        // CardReader is unresponsive or not
        // supported by the CardReader or
        // software.
        Mute = 0x00000200,
        // This implies that the card in the
        // CardReader has not been powered up.
        Unpowered = 0x00000400,
        Mask=0x0000FFFF^SCardReaderState.Unpowered,
    }
}