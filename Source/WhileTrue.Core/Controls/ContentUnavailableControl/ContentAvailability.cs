// ReSharper disable UnusedMember.Global
using System.ComponentModel;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Wraps the information whether content is available in a <see cref="ContentUnavailableControl"/>
    /// </summary>
    [TypeConverter(typeof (ContentAvailabilityConverter))]
    public class ContentAvailability
    {
        private readonly bool isAvailable;

        private ContentAvailability(bool isAvailable)
        {
            this.isAvailable = isAvailable;
        }

        ///<summary/>
        public bool IsAvailable
        {
            get { return this.isAvailable; }
        }


        private static readonly ContentAvailability available = new ContentAvailability(true);
        private static readonly ContentAvailability unavailable = new ContentAvailability(false);

        ///<summary/>
        public static ContentAvailability Available
        {
            get { return available; }
        }

        ///<summary/>
        public static ContentAvailability Unavailable
        {
            get { return unavailable; }
        }
    }
}