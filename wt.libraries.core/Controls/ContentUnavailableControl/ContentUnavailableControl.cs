﻿// ReSharper disable MemberCanBePrivate.Global

using Xamarin.Forms;

namespace WhileTrue.Controls
{
    /// <summary>
    ///     provides default content in case the 'real' content is not availiable. This can be handy
    ///     for messages as long as the real content is not yet available
    /// </summary>
    public class ContentUnavailableControl : ContentView
    {
        /// <summary />
        public static readonly BindableProperty ContentAvailableProperty;

        private View originalContent;


        static ContentUnavailableControl()
        {
            ContentAvailableProperty = BindableProperty.Create(
                "ContentAvailable",
                typeof(ContentAvailability),
                typeof(ContentUnavailableControl),
                ContentAvailability.Available,
                propertyChanged: ContentAvailableChanged
            );
        }

        /// <summary>
        ///     Defines whether the content is shown, or the default content which is defined with the
        ///     <see cref="DefaultContent" /> property.
        /// </summary>
        /// <remarks>
        ///     The ContentAvailability type is equpped with a type converter which resolves the availability from very different
        ///     types.
        ///     Refer to the <see cref="ContentAvailability" /> class for details.
        /// </remarks>
        public ContentAvailability ContentAvailable
        {
            get => (ContentAvailability) GetValue(ContentAvailableProperty);
            set => SetValue(ContentAvailableProperty, value);
        }

        /// <summary>
        ///     Defines the default content which is shown in place of the regular content if <see cref="ContentAvailable" />
        ///     resolves to <see cref="ContentAvailability.IsAvailable" />.
        /// </summary>
        public View DefaultContent { get; set; }

        private static void ContentAvailableChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ((ContentUnavailableControl) bindable).UpdateContent();
        }

        private void UpdateContent()
        {
            if (ContentAvailable?.IsAvailable == true)
            {
                Content = originalContent;
            }
            else
            {
                originalContent = Content;
                Content = DefaultContent;
            }
        }
    }
}