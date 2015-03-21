// ReSharper disable MemberCanBePrivate.Global
using System.Windows;
using System.Windows.Controls;

namespace WhileTrue.Controls
{
    ///<summary>
    /// provides default content in case the 'real' content is not availiable. This can be handy
    /// for messages as long as the real content is not yet available
    ///</summary>
    public class ContentUnavailableControl : ContentControl
    {
        /// <summary/>
        public static readonly DependencyProperty ContentAvailableProperty;
        /// <summary/>
        public static readonly DependencyProperty DefaultContentProperty;

        
        static ContentUnavailableControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentUnavailableControl), new FrameworkPropertyMetadata(typeof(ContentUnavailableControl)));

            ContentAvailableProperty = DependencyProperty.Register(
                "ContentAvailable",
                typeof(ContentAvailability),
                typeof(ContentUnavailableControl),
                new FrameworkPropertyMetadata( ContentAvailability.Available, FrameworkPropertyMetadataOptions.AffectsRender)
                );

            DefaultContentProperty = DependencyProperty.Register(
                "DefaultContent",
                typeof(object),
                typeof(ContentUnavailableControl),
                new FrameworkPropertyMetadata(null)
                );
        }

        ///<summary>
        /// Defines whether the content is shown, or the default content which is defined with the <see cref="DefaultContent"/> property.
        ///</summary>
        /// <remarks>
        /// The ContentAvailability type is equpped with a type converter which resolves the availability from very different types.
        /// Refer to the <see cref="ContentAvailability"/> class for details.
        /// </remarks>
        public ContentAvailability ContentAvailable
        { 
            get
            {
                return (ContentAvailability)GetValue(ContentAvailableProperty);
            }
            set
            {
                SetValue(ContentAvailableProperty,value);
            }
        }    
        
        /// <summary>
        /// Defines the default content which is shown in place of the regular content if <see cref="ContentAvailable"/>
        /// resolves to <see cref="ContentAvailability.IsAvailable"/>.
        /// </summary>
        public object DefaultContent
        { 
            get
            {
                return GetValue(DefaultContentProperty);
            }
            set
            {
                SetValue(DefaultContentProperty, value);
            }
        }


    }
}
