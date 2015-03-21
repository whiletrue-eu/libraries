using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mz.Classes.Framework;

namespace Mz.Classes.Controls
{
    ///<summary>
    ///</summary>
    public class DialogPanel : Panel
    {
        private static readonly AttachedDependencyPropertyEventManager captionChangedEventManager = new AttachedDependencyPropertyEventManager();
        private static readonly AttachedDependencyPropertyEventManager isSynchronisationRootChangedEventManager = new AttachedDependencyPropertyEventManager();

        public static readonly DependencyProperty CaptionProperty;
        public static readonly DependencyProperty IsSynchronisationScopeProperty;
        private static readonly DependencyProperty privateSynchronisationRootProperty; 

        public static readonly DependencyProperty StrechModeProperty;
        private readonly Dictionary<UIElement, UIElement> captionControls = new Dictionary<UIElement, UIElement>();
        private SynchronisationRoot synchronisationRoot;
        private double maxRowHeight;

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

            IsSynchronisationScopeProperty = DependencyProperty.RegisterAttached(
                "IsSynchronisationScope",
                typeof (bool),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    isSynchronisationRootChangedEventManager.ChangedHandler
                    )
                );
            isSynchronisationRootChangedEventManager.Changed += IsSynchronisationRootChangedEventManagerChanged;

            privateSynchronisationRootProperty = DependencyProperty.RegisterAttached(
                "PrivateSynchronisationRoot",
                typeof (SynchronisationRoot),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.Inherits
                    )
                );
            privateSynchronisationRootProperty.OverrideMetadata(
                typeof(DialogPanel),
                new FrameworkPropertyMetadata(OnPrivateSynchronisationRootPropertyChanged)
                );


            StrechModeProperty = DependencyProperty.Register(
                "StrechMode",
                typeof (DialogPanelModeStyle),
                typeof (DialogPanel),
                new FrameworkPropertyMetadata(
                    DialogPanelModeStyle.GrowControl,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange
                    )
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
                ((DependencyObject) sender).SetValue(privateSynchronisationRootProperty, new SynchronisationRoot());
            }
        }

        private static void OnPrivateSynchronisationRootPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            DialogPanel Panel = (DialogPanel) dependencyObject;

            SynchronisationRoot SynchronisationRoot = (SynchronisationRoot) e.NewValue;

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

        public DialogPanelModeStyle StrechMode
        {
            get { return (DialogPanelModeStyle) this.GetValue(StrechModeProperty); }
            set { this.SetValue(StrechModeProperty, value); }
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

        public bool EnforceSameRowHeights
        {
            get;
            set;
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


        private void CreateCaptionForControl(UIElement control)
        {
            object Caption = GetCaption(control);
            AddCaptionChangedEventHandler(control, this.ControlCaptionChanged);

            UIElement CaptionControl;
            if (Caption == null)
            {
                CaptionControl = new TextBlock();
            }
            else if (Caption is UIElement)
            {
                CaptionControl = (UIElement) Caption;
            }
            else
            {
                TextBlock TextBlock = new TextBlock();
                TextBlock.Text = Caption.ToString();
                CaptionControl = TextBlock;
            }

            this.captionControls.Add(control, CaptionControl);
            this.AddVisualChild(CaptionControl);
        }

        private void ControlCaptionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement Control = (UIElement) sender;
            this.DisposeCaptionForControl(Control);
            this.CreateCaptionForControl(Control);
        }

        private void DisposeCaptionForControl(UIElement control)
        {
            UIElement Caption = this.captionControls[control];

            RemoveCaptionChangedEventHandler(control, this.ControlCaptionChanged);

            this.RemoveVisualChild(Caption);
            this.captionControls.Remove(control);
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
                return this.captionControls[this.Children[index - ChildrenCount]];
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
            return Measure();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return Arrange(finalSize);
        }

        private List<UIElement> GetControls()
        {
            List<UIElement> Controls = new List<UIElement>();

            foreach (UIElement Control in this.Children)
            {
                //UIElement Caption = this.GetCaptionForControl(Control);
                //if (Caption != null)
                //{
                Controls.Add(Control);
                //}
                //else
                //{
                //Control is a caption itsself and will be layouted through its corresponding control
                //}
            }
            return Controls;
        }

        private Size Measure()
        {
            List<UIElement> Controls = this.GetControls();
            if (Controls.Count > 0)
            {
                this.EnsureSynchronisationRootExists();
                this.synchronisationRoot.UpdateSizes();

                Size InfiniteSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

                double Height = 0;
                this.maxRowHeight = 0;

                foreach (UIElement Control in Controls)
                {
                    UIElement Caption = this.GetCaptionForControl(Control);

                    Control.Measure(InfiniteSize);
                    Caption.Measure(InfiniteSize);

                    Size ControlSize = Control.DesiredSize;
                    Size CaptionSize = Caption.DesiredSize;

                    double RowHeight = Math.Max(CaptionSize.Height, ControlSize.Height);
                    Height += RowHeight;
                    this.maxRowHeight = Math.Max(this.maxRowHeight, RowHeight);
                }

                if( this.EnforceSameRowHeights )
                {
                    Height = Controls.Count*this.maxRowHeight;
                }

                return new Size(this.synchronisationRoot.CaptionWidth +this.InnerColumnMargin+ this.synchronisationRoot.ControlWidth, Height + (Controls.Count - 1)*this.InnerRowMargin);
            }
            else
            {
                return new Size();
            }
        }

        private void EnsureSynchronisationRootExists()
        {
            if( this.synchronisationRoot == null )
            {
                this.synchronisationRoot = new SynchronisationRoot();
                this.synchronisationRoot.AddMember(this);
            }
        }

        private Size Arrange(Size finalSize)
        {
            double CaptionWidth;
            double ControlWidth;
            switch (this.StrechMode)
            {
                case DialogPanelModeStyle.Proportional:
                    // Calculate width of the columns for the caption and control. 
                    // Both columns will be streched proportionally
                    double NeededWidth = this.synchronisationRoot.CaptionWidth + this.synchronisationRoot.ControlWidth;
                    CaptionWidth = this.synchronisationRoot.CaptionWidth*finalSize.Width/NeededWidth;
                    ControlWidth = this.synchronisationRoot.ControlWidth * finalSize.Width / NeededWidth;
                    break;
                case DialogPanelModeStyle.GrowControl:
                    CaptionWidth = this.synchronisationRoot.CaptionWidth;
                    ControlWidth = finalSize.Width - this.synchronisationRoot.CaptionWidth;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            double Top = 0;
            double InnerRowMargin = this.InnerRowMargin;
            List<UIElement> Controls = this.GetControls();
            foreach (UIElement Control in Controls)
            {
                UIElement Caption = this.GetCaptionForControl(Control);

                bool IsLastControl = Controls.IndexOf(Control) == Controls.Count - 1;

                Size ControlSize = Control.DesiredSize;
                Size CaptionSize = Caption.DesiredSize;
                double ControlHeight;
                if (IsLastControl)
                {
                    ControlHeight = finalSize.Height - Top;
                }
                else
                {
                    if (this.EnforceSameRowHeights)
                    {
                        ControlHeight = this.maxRowHeight;
                    }
                    else
                    {
                        ControlHeight = Math.Max(CaptionSize.Height, ControlSize.Height);
                    }
                }

                Caption.Arrange(new Rect(0, Top, CaptionWidth, ControlHeight));
                Control.Arrange(new Rect(CaptionWidth + this.InnerColumnMargin, Top, ControlWidth, ControlHeight));

                Top += ControlHeight + InnerRowMargin;
            }

            return finalSize;
        }

        private class SynchronisationRoot
        {
            private readonly List<DialogPanel> members = new List<DialogPanel>();
            private double controlWidth;
            private double captionWidth;

            public void AddMember(DialogPanel panel)
            {
                this.members.Add(panel);
                panel.IsVisibleChanged += this.PanelIsVisibleChanged;
            }

            public void RemoveMember(DialogPanel panel)
            {
                this.members.Remove(panel);
                panel.IsVisibleChanged += this.PanelIsVisibleChanged;
            }

            void PanelIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                this.RemeasureMembers();
            }

            private void RemeasureMembers()
            {
                foreach (DialogPanel Panel in this.members)
                {
                    Panel.InvalidateMeasure();
                }
            }

            public void UpdateSizes()
            {
                Size InfiniteSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

                this.controlWidth = 0;
                this.captionWidth = 0;

                foreach (ControlPair ControlPair in this.GetControls())
                {
                    UIElement Control = ControlPair.Control;
                    UIElement Caption = ControlPair.Caption;

                    Control.Measure(InfiniteSize);
                    Caption.Measure(InfiniteSize);

                    Size ControlSize = Control.DesiredSize;
                    Size CaptionSize = Caption.DesiredSize;

                    this.controlWidth = Math.Max(ControlSize.Width, this.controlWidth);
                    this.captionWidth = Math.Max(CaptionSize.Width, this.captionWidth);
                }
            }

            private class ControlPair
            {
                public UIElement Control { get; set; }

                public UIElement Caption { get; set; }
            }

            private IEnumerable GetControls()
            {
                List<object> ControlPairs = new List<object>();
                foreach (DialogPanel Panel in this.members)
                {
                    if (Panel.IsVisible)
                    {
                        foreach (UIElement Control in Panel.GetControls())
                        {
                            UIElement Caption = Panel.GetCaptionForControl(Control);
                            ControlPairs.Add(new ControlPair {Control = Control, Caption = Caption});
                        }
                    }
                }
                return ControlPairs;
            }

            public double ControlWidth
            {
                get { return controlWidth; }
            }

            public double CaptionWidth
            {
                get { return captionWidth; }
            }
        }   
    }


}