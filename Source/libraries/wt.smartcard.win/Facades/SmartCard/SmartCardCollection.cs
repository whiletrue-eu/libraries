using System;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Facades.SmartCard
{
    /// <supportingClass/>
    /// <summary>
    /// Collection of <see cref="ISmartCard"/> implementing objects
    /// </summary>
    public class SmartCardCollection : ObservableReadOnlyCollection<ISmartCard>
    {
        /// <summary>
        /// Gets the smart card inserted in the given card reader
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thown if no smart card is inserted in the given reader</exception>
        public ISmartCard this[ICardReader cardReader]
        {
            get
            {
                foreach (ISmartCard SmartCard in this.InnerList)
                {
                    if (SmartCard.CardReader == cardReader)
                    {
                        return SmartCard;
                    }
                }
                throw new IndexOutOfRangeException(cardReader.FriendlyName);
            }
        }

        ///<summary>
        /// Adds the smartcard to the collection
        ///</summary>
        public void Add(ISmartCard smartCard)
        {
            this.InnerList.Add(smartCard);
        }

        ///<summary>
        /// Removes the smartcard from the collection
        ///</summary>
        public void Remove(ISmartCard smartCard)
        {
            this.InnerList.Remove(smartCard);
        }
    }
}