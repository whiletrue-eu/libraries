using System;
using WhileTrue.Components.CardReaderSubsystem.Base;

namespace WhileTrue.Components.CardReaderSubsystem.SCP
{
    public class ScpSmartCardSubsystem : CardReaderSubsystemBase, IDisposable
    {
        public ScpSmartCardSubsystem()
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