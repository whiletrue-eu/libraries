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
        private ContentAvailability(bool isAvailable)
        {
            this.IsAvailable = isAvailable;
        }

        ///<summary/>
        public bool IsAvailable { get; }


        private static readonly ContentAvailability available = new ContentAvailability(true);
        private static readonly ContentAvailability unavailable = new ContentAvailability(false);

        ///<summary/>
        public static ContentAvailability Available => ContentAvailability.available;

        ///<summary/>
        public static ContentAvailability Unavailable => ContentAvailability.unavailable;
    }
}