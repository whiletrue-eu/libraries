using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Provides an easy way to specify grouping and sorting for a collection binding using a converter and a formatting string as Converter Parameter.
    /// The parameter is formaated in the following way:
    /// [PropertyName] - uses Property Name for grouping
    /// PropertyName+ - Sorts ascending
    /// PropertyName- - Sorts descending
    /// Multiple values can be concatenated in a comma-seperated list
    /// </summary>
    public class CollectionViewConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Data.CollectionViewSource ViewSource;
            if (parameter is System.Windows.Data.CollectionViewSource)
            {
                ViewSource = (System.Windows.Data.CollectionViewSource) parameter;
            }
            else if (parameter is string)
            {
                ViewSource = new System.Windows.Data.CollectionViewSource();
                string Parameter = (string) parameter;
                //"[Group] -SortDesc- +SortAsc+"
                Match Match = Regex.Match(Parameter.Replace('/', '\n'), @"((\[(?<Group>.*?)\]|(?<SortAsc>.*?)\+|(?<SortDesc>.*?)-)\w*)*");
                while (Match.Success)
                {
                    if (Match.Groups["Group"].Success)
                    {
                        ViewSource.GroupDescriptions.Add(new DynamicPropertyGroupDescription(Match.Groups["Group"].Value, ViewSource));
                    }
                    else if (Match.Groups["SortAsc"].Success)
                    {
                        ViewSource.SortDescriptions.Add(new SortDescription(Match.Groups["SortAsc"].Value, ListSortDirection.Ascending));
                    }
                    else if (Match.Groups["SortDesc"].Success)
                    {
                        ViewSource.SortDescriptions.Add(new SortDescription(Match.Groups["SortDesc"].Value, ListSortDirection.Descending));
                    }

                    Match = Match.NextMatch();
                }
            }
            else
            {
                throw new ArgumentException("A CollectionViewSource must be given as ConverterParameter", nameof(parameter));
            }

            ViewSource.Source = value;

            CollectionSortDescriptionRefreshAdapter.Attach(ViewSource);

            return ViewSource.View;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("ConvertBack not supported");
        }


        private class CollectionSortDescriptionRefreshAdapter
        {
            private readonly System.Windows.Data.CollectionViewSource collectionViewSource;
            private bool refreshNeeded;

            public static void Attach(System.Windows.Data.CollectionViewSource collectionViewSource)
            {
                // ReSharper disable once ObjectCreationAsStatement - object is attached to event handlers to keep it alive
                new CollectionSortDescriptionRefreshAdapter(collectionViewSource);
            }

            private CollectionSortDescriptionRefreshAdapter(System.Windows.Data.CollectionViewSource collectionViewSource)
            {
                this.collectionViewSource = collectionViewSource;
                this.collectionViewSource.View.CollectionChanged += this.View_CollectionChanged;
                this.collectionViewSource.View.SourceCollection.ForEach(this.AttachItem);
            }

            void View_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                e.OldItems?.ForEach(this.DetachItem);
                e.NewItems?.ForEach(this.AttachItem);
            }

            private void DetachItem(object item)
            {
                if (item is INotifyPropertyChanged)
                {
                    ((INotifyPropertyChanged) item).PropertyChanged -= this.PropertyChanged;
                }
            }

            private void AttachItem(object item)
            {
                if (item is INotifyPropertyChanged)
                {
                    ((INotifyPropertyChanged) item).PropertyChanged += this.PropertyChanged;
                }
            }

            void PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (this.collectionViewSource.SortDescriptions.Any(sort => sort.PropertyName == e.PropertyName))
                {
                    this.ScheduleRefreshView();
                }
            }

            void ScheduleRefreshView()
            {
                lock (this)
                {
                    this.refreshNeeded = true;
                    this.collectionViewSource.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action) this.RefreshView);
                }
            }

            void RefreshView()
            {
                lock (this)
                {
                    if (this.refreshNeeded)
                    {
                        this.collectionViewSource.View.Refresh();
                        this.refreshNeeded = false;
                    }
                }
            }
        }
    }
}
