// ReSharper disable UnusedMember.Global

using Xamarin.Forms;

namespace WhileTrue.Controls
{
    /// <summary>
    ///     Wraps the information whether content is available in a <see cref="ContentUnavailableControl" />
    /// </summary>
    [TypeConverter(typeof(ContentAvailabilityConverter))]
    public class ContentAvailability
    {
        private ContentAvailability(bool isAvailable)
        {
            IsAvailable = isAvailable;
        }

        /// <summary />
        public bool IsAvailable { get; }

        /// <summary />
        public static ContentAvailability Available { get; } = new ContentAvailability(true);

        /// <summary />
        public static ContentAvailability Unavailable { get; } = new ContentAvailability(false);
    }
}