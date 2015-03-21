using System;
using WhileTrue.Components.CardReaderSubsystem.Base;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.CardReaderSubsystem.SCP
{
    public class SCPSmartCardSubsystem : CardReaderSubsystemBase, IDisposable
    {
        public SCPSmartCardSubsystem()
        {
            this.AddCardReader(new SCPCardReader(1));
            this.AddCardReader(new SCPCardReader(2));
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (SCPCardReader CardReader in this.Readers)
            {
                CardReader.Dispose();
            }
        }

        #endregion
    }
}