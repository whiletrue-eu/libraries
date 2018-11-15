using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Threading;

namespace WhileTrue.Classes.Wpf
{
    internal class DynamicPropertyGroupDescription : PropertyGroupDescription
    {
        private readonly string groupname;
        private readonly System.Windows.Data.CollectionViewSource viewSource;
        private bool refreshNeeded;

        public DynamicPropertyGroupDescription(string groupname, System.Windows.Data.CollectionViewSource viewSource)
            : base(groupname)
        {
            this.groupname = groupname;
            this.viewSource = viewSource;
        }

        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            if (item is INotifyPropertyChanged)
                ((INotifyPropertyChanged) item).PropertyChanged += MyPropertyGroupDescription_PropertyChanged;
            return base.GroupNameFromItem(item, level, culture);
        }

        private void MyPropertyGroupDescription_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == groupname)
            {
                ((INotifyPropertyChanged) sender).PropertyChanged -= MyPropertyGroupDescription_PropertyChanged;
                ScheduleRefreshView();
            }
        }

        private void ScheduleRefreshView()
        {
            lock (this)
            {
                refreshNeeded = true;
                viewSource.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action) RefreshView);
            }
        }

        private void RefreshView()
        {
            lock (this)
            {
                if (refreshNeeded)
                {
                    viewSource.View.Refresh();
                    refreshNeeded = false;
                }
            }
        }
    }
}