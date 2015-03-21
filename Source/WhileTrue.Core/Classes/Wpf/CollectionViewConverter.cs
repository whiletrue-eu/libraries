using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Wpf
{
    public class CollectionViewSource
    {
        ///<summary>
        /// 
        ///</summary>
        public static readonly DependencyProperty FixProperty = DependencyProperty.RegisterAttached("Fix", typeof(string), typeof(CollectionViewSource), new FrameworkPropertyMetadata(FixChanged));

        public static void SetFix(DependencyObject d, string value)
        {
            d.SetValue(FixProperty,value);
        }
        public static string GetFix(DependencyObject d)
        {
            return (string) d.GetValue(FixProperty);

        }

        private static void FixChanged(DependencyObject d, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (d is System.Windows.Data.CollectionViewSource)
            {
                System.Windows.Data.CollectionViewSource ViewSource = (System.Windows.Data.CollectionViewSource)d;
                ((INotifyCollectionChanged)((System.Windows.Data.CollectionViewSource)d).GroupDescriptions).CollectionChanged +=
                    delegate
                        {
                            FixGroupDescriptions(ViewSource);
                        };

                FixGroupDescriptions(ViewSource);
            }
            else
            {
                throw new InvalidOperationException("Fix can only be set on CollectionViewSource");
            }
        }

        private static readonly List<System.Windows.Data.CollectionViewSource> avoidReentry = new List<System.Windows.Data.CollectionViewSource>();

        private static void FixGroupDescriptions(System.Windows.Data.CollectionViewSource viewSource)
        {
            if (avoidReentry.Contains(viewSource) == false)
            {
                GroupDescription[] Groups = viewSource.GroupDescriptions.ToArray();
                for (int Index = 0; Index < Groups.Length; Index++)
                {
                    if (Groups[Index] is PropertyGroupDescription)
                    {
                        PropertyGroupDescription Description = (PropertyGroupDescription) Groups[Index];

                        DynamicPropertyGroupDescription NewDescription = new DynamicPropertyGroupDescription(Description.PropertyName, viewSource);
                        NewDescription.StringComparison = Description.StringComparison;
                        NewDescription.Converter = Description.Converter;

                        int ChangeIndex = Index;
                        viewSource.Dispatcher.BeginInvoke(
                            (Action) delegate
                                         {
                                             avoidReentry.Add(viewSource);
                                             viewSource.GroupDescriptions.RemoveAt(ChangeIndex);
                                             viewSource.GroupDescriptions.Insert(ChangeIndex, NewDescription);
                                             avoidReentry.Remove(viewSource);
                                         });
                    }
                }
            }
        }
    }

    public class CollectionViewConverter:IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Data.CollectionViewSource ViewSource;
            if (parameter is System.Windows.Data.CollectionViewSource)
            {
                ViewSource = (System.Windows.Data.CollectionViewSource)parameter;
            }
            else if (parameter is string)
            {
                ViewSource = new System.Windows.Data.CollectionViewSource();
                string Parameter = (string) parameter;
                //"[Group] -SortDesc- +SortAsc+"
                Match Match = Regex.Match(Parameter.Replace('/','\n'), @"((\[(?<Group>.*?)\]|(?<SortAsc>.*?)\+|(?<SortDesc>.*?)-)\w*)*");
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
                throw new ArgumentException("A CollectionViewSource must be given as ConverterParameter","parameter");
            }

            ViewSource.Source = value;

            CollectionSortDescriptionRefreshAdapter.Attach(ViewSource);

            return ViewSource.View;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("ConvertBack not supported");
        }
    }

    internal class CollectionSortDescriptionRefreshAdapter
    {
        private readonly System.Windows.Data.CollectionViewSource collectionViewSource;
        private bool refreshNeeded;

        public static CollectionSortDescriptionRefreshAdapter Attach(System.Windows.Data.CollectionViewSource collectionViewSource)
        {
            return new CollectionSortDescriptionRefreshAdapter(collectionViewSource);
        }

        private CollectionSortDescriptionRefreshAdapter(System.Windows.Data.CollectionViewSource collectionViewSource)
        {
            this.collectionViewSource = collectionViewSource;
            this.collectionViewSource.View.CollectionChanged += this.View_CollectionChanged;
            this.collectionViewSource.View.SourceCollection.ForEach(this.AttachItem);
        }

        void View_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                e.OldItems.ForEach(this.DetachItem);
            }
            if (e.NewItems != null)
            {
                e.NewItems.ForEach(this.AttachItem);
            }
        }

        private void DetachItem(object item)
        {
            if (item is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)item).PropertyChanged -= this.PropertyChanged;
            }
        }

        private void AttachItem(object item)
        {
            if (item is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)item).PropertyChanged += this.PropertyChanged;
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
                this.collectionViewSource.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)this.RefreshView);
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

    internal class DynamicPropertyGroupDescription : PropertyGroupDescription
    {
        private readonly string groupname;
        private readonly System.Windows.Data.CollectionViewSource viewSource;
        private bool refreshNeeded;

        ///<summary>
        ///</summary>
        public DynamicPropertyGroupDescription()
        {
        }

        public DynamicPropertyGroupDescription(string groupname, System.Windows.Data.CollectionViewSource viewSource)
            : base(groupname)
        {
            this.groupname = groupname;
            this.viewSource = viewSource;
        }

        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            if (item is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)item).PropertyChanged += this.MyPropertyGroupDescription_PropertyChanged;
            }
            return base.GroupNameFromItem(item, level, culture);
        }

        void MyPropertyGroupDescription_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == this.groupname)
            {
                ((INotifyPropertyChanged) sender).PropertyChanged -= this.MyPropertyGroupDescription_PropertyChanged;
                this.ScheduleRefreshView();
            }
        }

        void ScheduleRefreshView()
        {
            lock (this)
            {
                this.refreshNeeded = true;
                this.viewSource.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)this.RefreshView);
            }
        }

        void RefreshView()
        {
            lock (this)
            {
                if (this.refreshNeeded)
                {
                    this.viewSource.View.Refresh();
                    this.refreshNeeded = false;
                }
            }
        }
    }
}