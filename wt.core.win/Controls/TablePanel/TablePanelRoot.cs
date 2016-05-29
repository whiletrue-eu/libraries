using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WhileTrue.Classes.Utilities;
using Adorner = System.Windows.Documents.Adorner;

namespace WhileTrue.Controls
{
    ///<summary>
    /// The table panel root control marks the root of a panel, that is layed out using <see cref="TablePanelRow"/> controls.
    /// The TablePanel rows will all laid out with aligned columns, no matter how they are interleaved in the controls tree
    ///</summary>
    public class TablePanelRoot : ContentControl
    {
        internal static readonly DependencyProperty InternalTablePanelRootProperty; 

        private double[] columnWidths = new double[0];
        private readonly Dictionary<TablePanelRow, double[]> columnsWidthsPerRow = new Dictionary<TablePanelRow, double[]>();
        private readonly List<TablePanelRow> rowsToLayout = new List<TablePanelRow>();


        static TablePanelRoot()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TablePanelRoot), new FrameworkPropertyMetadata(typeof(TablePanelRoot)));

            TablePanelRoot.InternalTablePanelRootProperty = DependencyProperty.RegisterAttached(
                "InternalTablePanelRoot",
                typeof (TablePanelRoot),
                typeof (TablePanelRoot),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange
                    )
                );

            

        }

        ///<summary/>
        public TablePanelRoot()
        {
            this.SetValue(TablePanelRoot.InternalTablePanelRootProperty, this);
            this.LayoutUpdated += this.OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            this.ReLayoutWhenNeeded();
        }


        internal static TablePanelRoot GetPanelRoot(DependencyObject dependencyObject)
        {
            return (TablePanelRoot) dependencyObject.GetValue(TablePanelRoot.InternalTablePanelRootProperty);
        }

        internal void UpdateColumnWidths(TablePanelRow tablePanelRow, ref double[] columnWidths)
        {
            if (this.columnsWidthsPerRow.ContainsKey(tablePanelRow))
            {
                this.columnsWidthsPerRow.Remove(tablePanelRow);
            }
            this.columnsWidthsPerRow.Add(tablePanelRow, (double[]) columnWidths.Clone()); //Clone!! Arrays are per-ref!


            this.RecalculateColumns(tablePanelRow);

            Array.Copy(this.columnWidths, columnWidths, columnWidths.Length);
        }

        private void RecalculateColumns(TablePanelRow requestor)
        {
            if (this.columnsWidthsPerRow.Count != 0)
            {
                int NumberOfColumns = this.columnsWidthsPerRow.Max(entry => entry.Value.Length);

                double[] ColumnWidths = new double[NumberOfColumns];
                for (int Index = 0; Index < NumberOfColumns; Index++)
                {
                    // ReSharper disable AccessToModifiedClosure
                    ColumnWidths[Index] = this.columnsWidthsPerRow.Max(entry => Index < entry.Value.Length ? entry.Value[Index] : 0);
                    // ReSharper restore AccessToModifiedClosure
                }

                if (ColumnWidths.HasEqualValue(this.columnWidths, (v1, v2) => Math.Abs(v1 - v2)<0.0001 ) == false)
                {
                    this.columnWidths = ColumnWidths;
                    this.rowsToLayout.Clear();
                    this.rowsToLayout.AddRange(this.columnsWidthsPerRow.Keys);
                }
                //Row will already be layouted with current values -> remove from re-layout list
                this.rowsToLayout.Remove(requestor);
            }
            else
            {
                this.columnWidths = new double[0];
            }
        }

        private void ReLayoutWhenNeeded()
        {
            foreach (TablePanelRow PanelRow in this.columnsWidthsPerRow.Keys.ToArray())
            {
                PanelRow.CheckMargin();
            }
            foreach (TablePanelRow PanelRow in this.rowsToLayout.ToArray()) //to array is used to read out the complete enumeration. it is modified during layout
            {
                PanelRow.NotifyLayoutNeeded();
            }
            this.rowsToLayout.Clear();
        }

        internal void Remove(TablePanelRow tablePanelRow)
        {
            if (this.columnsWidthsPerRow.ContainsKey(tablePanelRow))
            {
                this.columnsWidthsPerRow.Remove(tablePanelRow);
            }

            this.RecalculateColumns(null);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"/> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> is set to true internally. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
#if SHOW_GRIDLINE
        System.Windows.Documents.AdornerLayer AdornerLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(this);
            if (AdornerLayer != null)
            {
                AdornerLayer.Add(new GridLineAdorner(this));
            }
#endif
        }

        // ReSharper disable once UnusedMember.Local
        private class GridLineAdorner : Adorner
        {
            private readonly TablePanelRoot tablePanelRoot;

            public GridLineAdorner(TablePanelRoot tablePanelRoot) : base(tablePanelRoot)
            {
                this.tablePanelRoot = tablePanelRoot;
                this.IsHitTestVisible = false;
            }

            protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                Rect Size = VisualTreeHelper.GetDescendantBounds(this.tablePanelRoot);

                double Offset = 0;
                foreach (double Width in this.tablePanelRoot.columnWidths)
                {
                    drawingContext.DrawLine(
                        new Pen(Brushes.Yellow, 1) { DashStyle = new DashStyle(new double[] { 6, 4 }, 4) },
                        new Point(Offset,0),
                        new Point(Offset,Size.Height)
                        );
                    drawingContext.DrawLine(
                        new Pen(Brushes.Blue, 1){DashStyle = new DashStyle(new double[]{4,6},0)},
                        new Point(Offset, 0),
                        new Point(Offset, Size.Height)
                        );
                    Offset += Width;
                }
            }
        }
    }
}