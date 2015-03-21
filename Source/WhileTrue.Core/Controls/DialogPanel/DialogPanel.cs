using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls
{
    ///<summary>
    ///</summary>
    public partial class DialogPanel : Panel
    {
        private static readonly DependencyPropertyEventManager captionChangedEventManager = new DependencyPropertyEventManager();
        private static readonly DependencyPropertyEventManager isSynchronisationRootChangedEventManager = new DependencyPropertyEventManager();

        public static readonly DependencyProperty CaptionProperty;
        public static readonly DependencyProperty CaptionTemplateProperty;
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
            CaptionProperty = DependencyProperty.RegisterAttached(
                "Caption",
                typeof (object),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                    captionChangedEventManager.ChangedHandler
                    )
                );      
            
                CaptionTemplateProperty = DependencyProperty.RegisterAttached(
                "CaptionTemplate",
                typeof (DataTemplate),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange                    )
                );

            IsSynchronisationScopeProperty = DependencyProperty.RegisterAttached(
                "IsSynchronisationScope",
                typeof (bool),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                    isSynchronisationRootChangedEventManager.ChangedHandler
                    )
                );
            isSynchronisationRootChangedEventManager.Changed += IsSynchronisationRootChangedEventManagerChanged;

            privateSynchronisationRootProperty = DependencyProperty.RegisterAttached(
                "PrivateSynchronisationRoot",
                typeof (PanelSynchronisationRoot),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange
                    )
                );
            privateSynchronisationRootProperty.OverrideMetadata(
                typeof(DialogPanel),
                new FrameworkPropertyMetadata(OnPrivateSynchronisationRootPropertyChanged)
                );
        }

        private static void IsSynchronisationRootChangedEventManagerChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue.Equals(true))
            {
                ((DependencyObject)sender).ClearValue(privateSynchronisationRootProperty);
            }
            if (e.NewValue != null && e.NewValue.Equals(true))
            {
                ((DependencyObject) sender).SetValue(privateSynchronisationRootProperty, new PanelSynchronisationRoot());
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

        public double InnerColumnMargin
        {
            get { return this.innerColumnMargin; }
            set
            {
                this.innerColumnMargin = value;
                this.InvalidateMeasure();
            }
        }

        protected override int VisualChildrenCount
        {
            get { return this.Children.Count + this.captionControls.Count; }
        }

        public static void SetCaption(DependencyObject element, object caption)
        {
            element.SetValue(CaptionProperty, caption);
        }

        public static object GetCaption(DependencyObject element)
        {
            return element.GetValue(CaptionProperty);
        }

        public static void SetIsSynchronisationScope(DependencyObject element, bool isScope)
        {
            element.SetValue(IsSynchronisationScopeProperty, isScope);
        }

        public static bool GetIsSynchronisationContext(DependencyObject element)
        {
            return (bool) element.GetValue(IsSynchronisationScopeProperty);
        }

        public static void AddCaptionChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            captionChangedEventManager.AddEventHandler(dependencyObject, handler );
        }

        public static void RemoveCaptionChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            captionChangedEventManager.RemoveEventHandler(dependencyObject,handler);
        }

        public static void AddIsSynchronisationContextChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            isSynchronisationRootChangedEventManager.AddEventHandler(dependencyObject, handler);
        }

        public static void RemoveIsSynchronisationContextChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            isSynchronisationRootChangedEventManager.RemoveEventHandler(dependencyObject, handler);
        }


        public DataTemplate CaptionTemplate
        {
            get
            {
                return (DataTemplate) this.GetValue(CaptionTemplateProperty);
            }
            set 
            {
                this.SetValue(CaptionTemplateProperty,value);
            }
        }

        public double CalculatedCaptionWidth
        {
            get { return this.calculatedCaptionWidth; }
            private set
            {
                if (this.calculatedCaptionWidth != value)
                {
                    this.calculatedCaptionWidth = value;
                    this.SynchronisationRoot.NotifyMeasured(this);
                }
            }
        }

        public double UsedCaptionWidth { get; private set; }

        private void ControlCaptionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement Control = (UIElement) sender;
            this.DisposeCaptionForControl(Control);
            this.CreateCaptionForControl(Control);
        }

        private void CreateCaptionForControl(UIElement control)
        {
            AddCaptionChangedEventHandler(control, this.ControlCaptionChanged);

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

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
        }

        private void DisposeCaptionForControl(UIElement control)
        {
            RemoveCaptionChangedEventHandler(control, this.ControlCaptionChanged);
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

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded as UIElement != null)
            {
                UIElement Element = (UIElement) visualAdded;
                if (!this.GetIsCaption(Element))
                {
                    this.CreateCaptionForControl(Element);
                }
            }
            if (visualRemoved as UIElement != null)
            {
                UIElement Element = (UIElement) visualRemoved;
                if (!this.GetIsCaption(Element))
                {
                    this.DisposeCaptionForControl(Element);
                }
            }
        }

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
                        Size CaptionSize = Caption != null ? Caption.DesiredSize : new Size(0, 0); //Null caption indicates, that the control shall receive the full line

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
                Size CaptionSize = Caption != null?Caption.DesiredSize:new Size(0,0);
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