using System.Collections;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Components.SmartCardChannels
{
    public class AutoResponseTriggerCollection : CollectionBase
    {
        internal AutoResponseTriggerCollection()
        {
        }

        public AutoResponseTrigger this[int index] => (AutoResponseTrigger) this.InnerList[index];

        public void Add(AutoResponseTrigger autoResponseTrigger)
        {
            this.InnerList.Add(autoResponseTrigger);
        }

        internal void Remove(AutoResponseTrigger autoResponseTrigger)
        {
            this.InnerList.Remove(autoResponseTrigger);
        }

        internal CardCommand GetNextCommand(CardCommand command, CardResponse response)
        {
            foreach (AutoResponseTrigger Trigger in this.InnerList)
            {
                if (Trigger.IsMatch(command, response))
                {
                    return Trigger.GetCommand(command, response);
                }
            }
            return null;
        }

        public void AddRange(AutoResponseTrigger[] autoResponseTrigger)
        {
            this.InnerList.AddRange(autoResponseTrigger);
        }
    }
}