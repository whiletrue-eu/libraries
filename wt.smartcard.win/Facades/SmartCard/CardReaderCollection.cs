// ReSharper disable UnusedMember.Global
using System;
using System.Linq;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Facades.SmartCard
{
    /// <supportingClass/>
    /// <summary>
    /// Read-only Collection of <see cref="ICardReader"/> objects.
    /// </summary>
    public class CardReaderCollection : ObservableReadOnlyCollection<ICardReader>
    {
        /// <summary>
        /// Gets a card reader object by its name
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown if no card reader with this name exists</exception>
        public ICardReader this[string name]
        {
            get
            {
                foreach (ICardReader Reader in this.InnerList)
                {
                    if (Reader.Name == name)
                    {
                        return Reader;
                    }
                }
                throw new ArgumentException($"card reader '{name}' is unknown", nameof(name));
            }
        }

        ///<summary>
        /// Adds a card reader to the collection
        ///</summary>
        public void Add(ICardReader reader)
        {
            this.InnerList.Add(reader);
        }

        /// <summary>
        /// Removes the card reader from the collection
        /// </summary>
        public void Remove(ICardReader reader)
        {
            this.InnerList.Remove(reader);
        }

        /// <summary>
        /// Checks, whether a card reader witht he given name is a part of the collection or not
        /// </summary>
        /// <param name="name">name of the card reader to check</param>
        /// <returns><c>true</c>if a card reader with the given name is part of the colection</returns>
        public bool ContainsFriendlyName(string name)
        {
            return this.InnerList.Any(reader => reader.FriendlyName == name);
        }

        /// <summary>
        /// Checks, whether a card reader witht he given name is a part of the collection or not
        /// </summary>
        /// <param name="name">name of the card reader to check</param>
        /// <returns><c>true</c>if a card reader with the given name is part of the colection</returns>
        public bool ContainsName(string name)
        {
            return this.InnerList.Any(reader => reader.Name == name);
        }
    }
}