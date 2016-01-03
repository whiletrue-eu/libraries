// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Adds modal dialog features to a window
    /// </summary>
    /// <remarks>
    /// DialogWindow is a <see cref="Window"/> derived class that adds extra functionality,
    /// so that it can be easily used as a modal dialog window.
    /// 
    /// The following additons make it easy to use the window as a modal dialog:
    /// <list>
    ///     <Item>
    ///         <term><see cref="Buttons"/> dependency property</term>
    ///         <description>
    ///             lets you define a list of buttons which are used as dialog buttons.
    ///             the buttons automatically close the dialog (specify <see cref="Button.IsDefault"/> or <see cref="Button.IsCancel"/> as needed)
    ///             Additionally you can specify a <c>Result</c> for each Button
    ///         </description>
    ///     </Item>
    ///     <Item>
    ///         <term><see cref="ResultProperty">Result</see> attached property</term>
    ///         <description>
    ///             Lets you define a string that is presented in the <see cref="ResultValue"/> property once the
    ///             Dialog was closed. You can use an arbitrary string, or make use of the static members <see cref="OkResult"/>,
    ///             <see cref="CancelResult"/>, <see cref="YesResult"/> and <see cref="NoResult"/> defined in the class
    ///         </description>
    ///     </Item>
    ///     <Item>
    ///         <term>Custom dialog template</term>
    ///         <description>
    ///             The DialogWindow is equipped with a default template which positions the specified buttons on a
    ///             glass border at the bottom right of the dialog.<br/>
    ///             Additonally, the following values are set as default:
    ///             <code>
    ///             GlassMargin = 0, 0, 0, 30
    ///             ResizeMode = CanResize
    ///             WindowStartupLocation = CenterOwner
    ///             ShowInTaskbar = false
    ///             </code>
    ///         </description>
    ///     </Item>
    /// </list>
    /// 
    /// To have it even easier to create dialog windows, you can use the <see cref="DialogPanel"/> as content of the dialog.
    /// 
    /// </remarks>
    public class DialogWindow : Window
    {
        /// <summary/>
        public static readonly string CancelResult = "Cancel";

        /// <summary/>
        public static readonly string NoResult = "No";

        /// <summary/>
        public static readonly string OkResult = "OK";

        /// <summary/>
        public static readonly string YesResult = "Yes";

        #region dependency / attached properties

        /// <summary/>
        public static readonly DependencyProperty DialogTitleImageSourceProperty;
        /// <summary/>
        public static readonly DependencyProperty DialogTitleProperty;
        /// <summary/>
        public static readonly DependencyProperty DialogSubtitleProperty;


        /// <summary/>
        public static readonly DependencyProperty ExtraInformationProperty;

        /// <summary/>
        public static readonly DependencyProperty ButtonsProperty;
        private static readonly DependencyPropertyKey buttonsPropertyKey;

        /// <summary/>
        public static readonly DependencyProperty ResultProperty;


        static DialogWindow()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogWindow), new FrameworkPropertyMetadata(typeof(DialogWindow)));

            DialogWindow.DialogTitleImageSourceProperty = DependencyProperty.Register(
                "DialogTitleImage",
                typeof (ImageSource),
                typeof (DialogWindow),
                new PropertyMetadata(null)
                );

            DialogWindow.DialogTitleProperty = DependencyProperty.Register(
                "DialogTitle",
                typeof(object),
                typeof(DialogWindow),
                new PropertyMetadata(null)
                );

            DialogWindow.DialogSubtitleProperty = DependencyProperty.Register(
                "DialogSubtitle",
                typeof(object),
                typeof(DialogWindow),
                new PropertyMetadata(null)
                );

            DialogWindow.ExtraInformationProperty = DependencyProperty.Register(
                "ExtraInformation",
                typeof(object),
                typeof(DialogWindow),
                new PropertyMetadata(null)
                );

            DialogWindow.buttonsPropertyKey = DependencyProperty.RegisterReadOnly(
                "Buttons",
                typeof (ObservableCollection<Button>),
                typeof (DialogWindow),
                new FrameworkPropertyMetadata(new ObservableCollection<Button>(), new PropertyChangedCallback(DialogWindow.ButtonsChanged))
                );
            DialogWindow.ButtonsProperty = DialogWindow.buttonsPropertyKey.DependencyProperty;

            DialogWindow.ResultProperty = DependencyProperty.RegisterAttached(
                "Result",
                typeof (string),
                typeof (DialogWindow),
                new FrameworkPropertyMetadata(
                    null
                    )
                );
        }

        private static void ButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogWindow Window = (DialogWindow) d;
            Window.OnButtonsChanged((ObservableCollection<Button>) e.OldValue, (ObservableCollection<Button>) e.NewValue);
        }

        #endregion

        /// <summary/>
        public DialogWindow()
        {
            this.SetValue(DialogWindow.buttonsPropertyKey, new ObservableCollection<Button>());

            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }


        /// <summary>
        /// Gets the value of the <see cref="ResultProperty">Result</see> attached property of the button that
        /// was used to close the dialog. <br/>
        /// If the dialog was closed using the windows close button, the value of the Button marked as <see cref="Button.IsCancel"/>
        /// is used. If no such button is deifned, <c>null</c> is returned.
        /// </summary>
        public string ResultValue { get; private set; }

        /// <summary>
        /// Gets the list of dialog buttons.
        /// </summary>
        public ObservableCollection<Button> Buttons => (ObservableCollection<Button>) this.GetValue(DialogWindow.ButtonsProperty);

        /// <summary>
        /// Sets/Gets the title image
        /// </summary>
        public ImageSource DialogTitleImage
        {
            get { return (ImageSource) this.GetValue(DialogWindow.DialogTitleImageSourceProperty); }
            set { this.SetValue(DialogWindow.DialogTitleImageSourceProperty, value); }
        }

        /// <summary>
        /// Sets/Gets the title
        /// </summary>
        public object DialogTitle
        {
            get { return this.GetValue(DialogWindow.DialogTitleProperty); }
            set { this.SetValue(DialogWindow.DialogTitleProperty, value); }
        }

        /// <summary>
        /// Sets/Gets the subtitle
        /// </summary>
        public object DialogSubtitle
        {
            get { return this.GetValue(DialogWindow.DialogSubtitleProperty); }
            set { this.SetValue(DialogWindow.DialogSubtitleProperty, value); }
        }

        /// <summary>
        /// Sets/Gets the extra information displayed in the lower-left edge of the buttons-bar
        /// </summary>
        public object ExtraInformation
        {
            get { return this.GetValue(DialogWindow.ExtraInformationProperty); }
            set { this.SetValue(DialogWindow.ExtraInformationProperty, value); }
        }


        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Button Button = (Button) sender;

            string Result =DialogWindow.GetResult(Button);

            if (Result != null)
            {
                this.ResultValue = Result;

                if (Button.IsCancel)
                {
                    //Dialog result is set by 'IsCancel' property
                }
                else
                {
                    this.DialogResult = true;
                }
            }
            else
            {
                //No result -> Not a close button -> Ignore click
            }
        }

        /// <summary/>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (this.ResultValue == null)
            {
                foreach (Button Button in this.Buttons)
                {
                    if (Button.IsCancel)
                    {
                        this.ResultValue = DialogWindow.GetResult(Button);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the <c>Result</c> attached property on a Button in the <see cref="Buttons"/> collection.
        /// </summary>
        public static void SetResult(DependencyObject dependencyObject, string element)
        {
            dependencyObject.SetValue(DialogWindow.ResultProperty, element);
        }

        /// <summary>
        /// Gets the <c>Result</c> attached property on a Button in the <see cref="Buttons"/> collection.
        /// </summary>
        public static string GetResult(DependencyObject dependencyObject)
        {
            return (string) dependencyObject.GetValue(DialogWindow.ResultProperty);
        }

        private void ButtonsCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RegisterClickEvents(e.NewItems);
            this.UnregisterClickEvents(e.OldItems);
        }

        private void UnregisterClickEvents(IList items)
        {
            if (items != null)
            {
                foreach (Button Button in items)
                {
                    Button.Click -= this.ButtonClick;
                }
            }
        }

        private void RegisterClickEvents(IList items)
        {
            if (items != null)
            {
                foreach (Button Button in items)
                {
                    Button.Click += this.ButtonClick;
                }
            }
        }

        /// <summary>
        /// Is called when the buttons collection (the complete collection, not a single item!) is changed
        /// </summary>
        protected void OnButtonsChanged(ObservableCollection<Button> oldValue, ObservableCollection<Button> newValue)
        {
            if (newValue != null)
            {
                newValue.CollectionChanged += this.ButtonsCollectionCollectionChanged;
                this.RegisterClickEvents(newValue);
            }
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= this.ButtonsCollectionCollectionChanged;
                this.UnregisterClickEvents(oldValue);
            }
        }
    }
}