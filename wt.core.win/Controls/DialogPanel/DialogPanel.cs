using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using JetBrains.Annotations;
using WhileTrue.Classes.Wpf;

namespace WhileTrue.Controls
{
    ///<summary>
    /// THe dialog panel provies a two-column layout mainly used in dialog which have a caption in the first column and the control in the second.
    /// Only the controls have to be specified; the caption is specified as an attached property on the control and automatically laid out corerctly.
    /// Using the <see cref="IsSynchronisationScopeProperty"/>, it is possible to align multiple independent dialog panels to have the same column width for the caption
    ///</summary>
    [PublicAPI]
    public partial class DialogPanel : Panel
    {
        private static readonly DependencyPropertyEventManager captionChangedEventManager = new DependencyPropertyEventManager();
        private static readonly DependencyPropertyEventManager isSynchronisationRootChangedEventManager = new DependencyPropertyEventManager();

        /// <summary>
        /// Attached property to specify a caption for children of the dialog panel. The caption does not necessarily have to be a string.
        /// You can specify a <see cref="CaptionTemplate"/> to specify how to create the caption
        /// </summary>
        public static readonly DependencyProperty CaptionProperty;
        /// <summary>
        /// Data Template to use to render the caption for panel children
        /// </summary>
        public static readonly DependencyProperty CaptionTemplateProperty;
        /// <summary>
        /// Attache this property on a common ancestor of two dialog´panels and ste it to true to have the column width of the caption aligned
        /// </summary>
        public static readonly DependencyProperty IsSynchronisationScopeProperty;
        private static readonly DependencyProperty privateSynchronisationRootProperty; 

        private readonly Dictionary<UIElement, UIElement> captionControls = new Dictionary<UIElement, UIElement>();
        private PanelSynchronisationRoot synchronisationRoot;

        private PanelSynchronisationRoot SynchronisationRoot
        {
            get
            {
                this.EnsureSynchronisationRootExists();
                return this.synchronisationRoot;
            }
        }

        private void EnsureSynchronisationRootExists()
        {
            if (this.synchronisationRoot == null)
            {
                this.synchronisationRoot = new PanelSynchronisationRoot();
                this.synchronisationRoot.AddMember(this);
            }
        }

        static DialogPanel()
        {
            DialogPanel.CaptionProperty = DependencyProperty.RegisterAttached(
                "Caption",
                typeof (object),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                    DialogPanel.captionChangedEventManager.ChangedHandler
                    )
                );      
            
                DialogPanel.CaptionTemplateProperty = DependencyProperty.RegisterAttached(
                "CaptionTemplate",
                typeof (DataTemplate),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange                    )
                );

            DialogPanel.IsSynchronisationScopeProperty = DependencyProperty.RegisterAttached(
                "IsSynchronisationScope",
                typeof (bool),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                    DialogPanel.isSynchronisationRootChangedEventManager.ChangedHandler
                    )
                );
            DialogPanel.isSynchronisationRootChangedEventManager.Changed += DialogPanel.IsSynchronisationRootChangedEventManagerChanged;

            DialogPanel.privateSynchronisationRootProperty = DependencyProperty.RegisterAttached(
                "PrivateSynchronisationRoot",
                typeof (PanelSynchronisationRoot),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange
                    )
                );
            DialogPanel.privateSynchronisationRootProperty.OverrideMetadata(
                typeof(DialogPanel),
                new FrameworkPropertyMetadata(DialogPanel.OnPrivateSynchronisationRootPropertyChanged)
                );
        }

        private static void IsSynchronisationRootChangedEventManagerChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue.Equals(true))
            {
                ((DependencyObject)sender).ClearValue(DialogPanel.privateSynchronisationRootProperty);
            }
            if (e.NewValue != null && e.NewValue.Equals(true))
            {
                ((DependencyObject) sender).SetValue(DialogPanel.privateSynchronisationRootProperty, new PanelSynchronisationRoot());
            }
        }

        private static void OnPrivateSynchronisationRootPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            DialogPanel Panel = (DialogPanel) dependencyObject;

            PanelSynchronisationRoot SynchronisationRoot = (PanelSynchronisationRoot) e.NewValue;

            if (Panel.synchronisationRoot != null)
            {
                //  if definition is already registered And shared size scope is changing,
                //  then un-register the definition from the current shared size state object. 
                Panel.synchronisationRoot.RemoveMember(Panel);
                Panel.synchronisationRoot = null;
            }

            if ((Panel.synchronisationRoot == null) && (SynchronisationRoot != null))
            {
                //  if definition is not registered and both: shared size group id AND private shared scope
                //  are available, then register definition.
                Panel.synchronisationRoot = SynchronisationRoot;
                Panel.synchronisationRoot.AddMember(Panel);
            }
        }

        private double innerRowMargin;

        /// <summary>
        /// Sets/Gets the margin between rows
        /// </summary>
        public double InnerRowMargin
        {
            get { return this.innerRowMargin; }
            set
            {
                this.innerRowMargin = value;
                this.InvalidateMeasure();
            }
        }

        private double innerColumnMargin;
        private double calculatedCaptionWidth;

        /// <summary>
        /// Sets/Gets the margin between the captiona nd the control column
        /// </summary>
        public double InnerColumnMargin
        {
            get { return this.innerColumnMargin; }
            set
            {
                this.innerColumnMargin = value;
                this.InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the number of child <see cref="T:System.Windows.Media.Visual"/> objects in this instance of <see cref="T:System.Windows.Controls.Panel"/>.
        /// </summary>
        /// <returns>
        /// The number of child <see cref="T:System.Windows.Media.Visual"/> objects.
        /// </returns>
        protected override int VisualChildrenCount => this.Children.Count + this.captionControls.Count;

        /// <summary>
        /// Attached property to specify a caption for children of the dialog panel. The caption does not necessarily have to be a string.
        /// You can specify a <see cref="CaptionTemplate"/> to specify how to create the caption
        /// </summary>
        public static void SetCaption(DependencyObject element, object caption)
        {
            element.SetValue(DialogPanel.CaptionProperty, caption);
        }

        /// <summary>
        /// Attached property to specify a caption for children of the dialog panel. The caption does not necessarily have to be a string.
        /// You can specify a <see cref="CaptionTemplate"/> to specify how to create the caption
        /// </summary>
        public static object GetCaption(DependencyObject element)
        {
            return element.GetValue(DialogPanel.CaptionProperty);
        }

        /// <summary>
        /// Attache this property on a common ancestor of two dialog´panels and ste it to true to have the column width of the caption aligned
        /// </summary>
        public static void SetIsSynchronisationScope(DependencyObject element, bool isScope)
        {
            element.SetValue(DialogPanel.IsSynchronisationScopeProperty, isScope);
        }

        /// <summary>
        /// Attache this property on a common ancestor of two dialog´panels and ste it to true to have the column width of the caption aligned
        /// </summary>
        public static bool GetIsSynchronisationContext(DependencyObject element)
        {
            return (bool) element.GetValue(DialogPanel.IsSynchronisationScopeProperty);
        }

        private static void AddCaptionChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            DialogPanel.captionChangedEventManager.AddEventHandler(dependencyObject, handler );
        }

        private static void RemoveCaptionChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            DialogPanel.captionChangedEventManager.RemoveEventHandler(dependencyObject,handler);
        }

        private static void AddIsSynchronisationContextChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            DialogPanel.isSynchronisationRootChangedEventManager.AddEventHandler(dependencyObject, handler);
        }

        private static void RemoveIsSynchronisationContextChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            DialogPanel.isSynchronisationRootChangedEventManager.RemoveEventHandler(dependencyObject, handler);
        }


        /// <summary>
        /// Data Template to use to render the caption for panel children
        /// </summary>
        public DataTemplate CaptionTemplate
        {
            get
            {
                return (DataTemplate) this.GetValue(DialogPanel.CaptionTemplateProperty);
            }
            set 
            {
                this.SetValue(DialogPanel.CaptionTemplateProperty,value);
            }
        }

        internal double CalculatedCaptionWidth
        {
            get { return this.calculatedCaptionWidth; }
            private set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (this.calculatedCaptionWidth != value)
                {
                    this.calculatedCaptionWidth = value;
                    this.SynchronisationRoot.NotifyMeasured(this);
                }
            }
        }

        internal double UsedCaptionWidth { get; private set; }

        private void ControlCaptionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement Control = (UIElement) sender;
            this.DisposeCaptionForControl(Control);
            this.CreateCaptionForControl(Control);
        }

        private void CreateCaptionForControl(UIElement control)
        {
            DialogPanel.AddCaptionChangedEventHandler(control, this.ControlCaptionChanged);

            object Caption = DialogPanel.GetCaption(control);
            if (Caption != null)
            {
                UIElement CaptionControl;
                if (Caption is UIElement)
                {
                    CaptionControl = (UIElement) Caption;
                }
                else
                {
                    ContentPresenter Presenter = new ContentPresenter();
                    Presenter.SetBinding(ContentPresenter.ContentProperty, new Binding {Source = control, Path = new PropertyPath(DialogPanel.CaptionProperty), BindsDirectlyToSource = true});
                    Presenter.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding {BindsDirectlyToSource = true, Source = this, Path = new PropertyPath("CaptionTemplate")});
                    CaptionControl = Presenter;
                }

                this.captionControls.Add(control, CaptionControl);
                this.AddVisualChild(CaptionControl);
            }
            else
            {
                //Ignore: there is no caption, This means, the control shall have full width
            }

            this.InvalidateMeasure();
        }

        private void DisposeCaptionForControl(UIElement control)
        {
            DialogPanel.RemoveCaptionChangedEventHandler(control, this.ControlCaptionChanged);
            if (this.captionControls.ContainsKey(control))
            {
                UIElement CaptionControl = this.captionControls[control];
                this.RemoveVisualChild(CaptionControl);
                this.captionControls.Remove(control);
            }
        }

        private bool GetIsCaption(UIElement control)
        {
            return this.captionControls.ContainsValue(control);
        }

        private UIElement GetCaptionForControl(UIElement control)
        {
            if (this.captionControls.ContainsKey(control))
            {
                return this.captionControls[control];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Windows.Media.Visual"/> child of this <see cref="T:System.Windows.Controls.Panel"/> at the specified index position.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Media.Visual"/> child of the parent <see cref="T:System.Windows.Controls.Panel"/> element.
        /// </returns>
        /// <param name="index">The index position of the <see cref="T:System.Windows.Media.Visual"/> child.</param>
        protected override Visual GetVisualChild(int index)
        {
            int ChildrenCount = this.Children.Count;

            if (index < ChildrenCount)
            {
                return this.Children[index];
            }
            else if (index < this.VisualChildrenCount)
            {
                return this.captionControls.Values.ElementAt(index - ChildrenCount);
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Invoked when the <see cref="T:System.Windows.Media.VisualCollection"/> of a visual object is modified.
        /// </summary>
        /// <param name="visualAdded">The <see cref="T:System.Windows.Media.Visual"/> that was added to the collection.</param><param name="visualRemoved">The <see cref="T:System.Windows.Media.Visual"/> that was removed from the collection.</param>
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded is UIElement)
            {
                UIElement Element = (UIElement) visualAdded;
                if (!this.GetIsCaption(Element))
                {
                    this.CreateCaptionForControl(Element);
                }
            }
            if (visualRemoved is UIElement)
            {
                UIElement Element = (UIElement) visualRemoved;
                if (!this.GetIsCaption(Element))
                {
                    this.DisposeCaptionForControl(Element);
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for child elements and determines a size for the <see cref="T:System.Windows.FrameworkElement"/>-derived class. 
        /// </summary>
        /// <returns>
        /// The size that this element determines it needs during layout, based on its calculations of child element sizes.
        /// </returns>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        protected override Size MeasureOverride(Size availableSize)
        {
                UIElementCollection Controls = this.Children;
                if (Controls.Count > 0)
                {
                    //Phase 1: Measure width of captions without considering synced panels. Max width for captions is 80% of the available space, so that controls still can be seen
                    Size AvailableCaptionSize = new Size(double.IsPositiveInfinity(availableSize.Width) ? Double.PositiveInfinity : availableSize.Width * 0.8, availableSize.Height);

                    double MaxCaptionWidth=0d;
                    foreach (UIElement Control in Controls)
                    {
                        UIElement Caption = this.GetCaptionForControl(Control);

                        if (Caption != null)
                        {
                            Caption.Measure(AvailableCaptionSize);
                            MaxCaptionWidth = Math.Max(MaxCaptionWidth, Caption.DesiredSize.Width);
                        }
                    }
                    this.CalculatedCaptionWidth = MaxCaptionWidth;

                    this.UsedCaptionWidth = this.SynchronisationRoot.CaptionWidth;
                    //Phase 2: Measure with calculated width of captionto get the size on the controls and get the height needed
                    double ControlWidth = double.IsPositiveInfinity(availableSize.Width) ? Double.PositiveInfinity : availableSize.Width - this.InnerColumnMargin - this.SynchronisationRoot.CaptionWidth;
                    double CaptionWidth = this.UsedCaptionWidth;
                    Size AvailableControlSize = new Size(ControlWidth, Double.PositiveInfinity);
                    Size InfiniteSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
                    Size AvailableNoCaptionControlSize = new Size(availableSize.Width, Double.PositiveInfinity);
                    Size AvailableConsolidatedCaptionSize = new Size(CaptionWidth, Double.PositiveInfinity);

                    double Height = 0;
                    double NeededWidth = 0;
                    foreach (UIElement Control in Controls)
                    {
                        UIElement Caption = this.GetCaptionForControl(Control);
                        if (Caption != null)
                        {
                            Caption.Measure(AvailableConsolidatedCaptionSize);
                            Control.Measure(AvailableControlSize);

                            //We have to call measure on the caption again, otherwise we will not receive onChildDesiredChange
                            //events anymore :-(
                            //we cannot call on the control, because text boxes do their word-wrap on basis on the measure and
                            //not the arrange - which is rather strange. Maybe resizing the control will not work if the text is resized..
                            Caption.Measure(InfiniteSize);
                        }
                        else
                        {
                            Control.Measure(AvailableNoCaptionControlSize);
                        }

                        Size ControlSize = Control.DesiredSize;
                        Size CaptionSize = Caption?.DesiredSize ?? new Size(0, 0); //Null caption indicates, that the control shall receive the full line

                        double RowHeight = Math.Max(CaptionSize.Height, ControlSize.Height);
                        NeededWidth = Math.Max(NeededWidth,
                            Caption != null
                                ? CaptionWidth + ControlSize.Width + this.innerColumnMargin
                                : ControlSize.Width);
                        Height += RowHeight;
                    }

                    double NeededHeight = Height + (Controls.Count - 1)*this.InnerRowMargin;

                    return new Size(NeededWidth, NeededHeight);
                }
                else
                {
                    return new Size(0, 0);
                }
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement"/> derived class. 
        /// </summary>
        /// <returns>
        /// The actual size used.
        /// </returns>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            this.EnsureSynchronisationRootExists();

            double CaptionWidth = this.SynchronisationRoot.CaptionWidth;
            double ControlWidth = finalSize.Width - this.InnerColumnMargin - CaptionWidth;

            double Top = 0;
            double InnerRowMargin = this.InnerRowMargin;
            UIElementCollection Controls = this.Children;

            foreach (UIElement Control in Controls)
            {
                UIElement Caption = this.GetCaptionForControl(Control);

                bool IsLastControl = Controls.IndexOf(Control) == Controls.Count - 1;

                Size ControlSize = Control.DesiredSize;
                Size CaptionSize = Caption?.DesiredSize ?? new Size(0,0);
                double RowHeight = IsLastControl ? finalSize.Height - Top : Math.Max(CaptionSize.Height, ControlSize.Height);
                if (Caption != null)
                {
                    Caption.Arrange(new Rect(0, Top, CaptionWidth, RowHeight));
                    Control.Arrange(new Rect(CaptionWidth + this.InnerColumnMargin, Top, ControlWidth, RowHeight));
                }
                else
                {
                    //there is no caption, This means, the control shall have full width
                    Control.Arrange(new Rect(0, Top, finalSize.Width, RowHeight));
                }

                Top += RowHeight + InnerRowMargin;
            }

            return finalSize;
        }
    }
}