using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Controls
{
    ///<summary>
    ///</summary>
    public class TablePanelRow : Panel
    {
// ReSharper disable MemberCanBePrivate.Global
        ///<summary>
        /// Column number. Controls tagged with the same column willb e layouted in the same column under the same root
        ///</summary>
        public static readonly DependencyProperty ColumnProperty;
// ReSharper restore MemberCanBePrivate.Global
        private double measuredHeight;
        [NotNull] private TablePanelRoot panelRoot = new TablePanelRoot();
        private double[] columnWidths;
        private double margin;
        private double[] columnOffsets;

        static TablePanelRow()
        {
            ColumnProperty = DependencyProperty.RegisterAttached(
                "Column",
                typeof(int),
                typeof(TablePanelRow),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange
                    )
                );
        }

// ReSharper disable MemberCanBePrivate.Global
        ///<summary/>
        public static int GetColumn(DependencyObject dependencyObject)
        {
            return (int) dependencyObject.GetValue(ColumnProperty);
        }

        ///<summary/>
        public static void SetColumn(DependencyObject dependencyObject, int column)
        {
            dependencyObject.SetValue(ColumnProperty, column);
        }

 // ReSharper restore MemberCanBePrivate.Global
       protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == TablePanelRoot.InternalTablePanelRootProperty)
            {
                this.UpdateTablePanelRoot();
            }
            if (e.Property == UIElement.IsVisibleProperty)
            {
                if ((bool)e.NewValue == false)
                {
                    this.panelRoot.Remove(this);
                }
                else
                {
                    this.NotifyLayoutNeeded();
                }
            }
        }

        private void UpdateTablePanelRoot()
        {
            TablePanelRoot PanelRoot = TablePanelRoot.GetPanelRoot(this) ?? new TablePanelRoot();

            if (PanelRoot != this.panelRoot)
            {
                if (this.IsVisible)
                {
                    this.panelRoot.Remove(this);
                }

                this.panelRoot = PanelRoot;

                if (this.IsVisible)
                {
                    this.UpdateLayout();
                }
            }


        }

        private void UpdateMargin()
        {
            double NewMargin;

            if (this.panelRoot.IsAncestorOf(this))
            {
                NewMargin = this.TransformToAncestor(this.panelRoot).Transform(new Point(0, 0)).X;
            }
            else
            {
                NewMargin = 0;
            }
            if (NewMargin != this.margin)
            {
                this.margin = NewMargin;
                this.NotifyLayoutNeeded();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            this.UpdateTablePanelRoot();
            this.UpdateMargin();

            int LastColumnIndex = 0;

            foreach (UIElement Child in this.Children)
            {
                int Column = TablePanelRow.GetColumn(Child);
                LastColumnIndex = Math.Max(LastColumnIndex, Column);

                Child.Measure(availableSize);
            }

            double[] ColumnWidths = new double[LastColumnIndex+1];
            this.measuredHeight = 0;

            foreach (UIElement Child in this.Children)
            {
                int Column = TablePanelRow.GetColumn(Child);
                Size DesiredSize = Child.DesiredSize;

                this.measuredHeight = Math.Max(this.measuredHeight, DesiredSize.Height);
                ColumnWidths[Column] = Math.Max(ColumnWidths[Column], DesiredSize.Width);
            }

            //Add the offset to the first cell
            ColumnWidths[0] += this.margin;
            // Update Column Widths and retrieve the accumulated ones
            this.panelRoot.UpdateColumnWidths(this, ref ColumnWidths);
            //Subtract the offset again to have the layout values
            ColumnWidths[0] -= this.margin;
            //Remember for arrange
            this.columnWidths = ColumnWidths;

            //Calculate cell offsets from widths for arrange
            this.columnOffsets = new double[ColumnWidths.Length];
            double ColumnOffset = 0;
            for (int Index = 0; Index < ColumnWidths.Length; Index++)
            {
                this.columnOffsets[Index] = ColumnOffset;
                ColumnOffset += ColumnWidths[Index];
            }

            Size NeededSize = new Size(ColumnWidths.Sum(), this.measuredHeight);
            return NeededSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement Child in this.Children)
            {
                int Column = TablePanelRow.GetColumn(Child);

                Rect ChildRect = new Rect(
                    Math.Min(this.columnOffsets[Column], finalSize.Width), 0,
                    Math.Min(this.columnWidths[Column], finalSize.Width - this.columnOffsets[Column]), finalSize.Height);
                Child.Arrange(ChildRect);
            }

            return finalSize;
        }

        internal void CheckMargin()
        {
            this.UpdateMargin();
        }

        internal void NotifyLayoutNeeded()
        {
            this.InvalidateMeasure();
        }
    }
}