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
            {
                ((INotifyPropertyChanged) item).PropertyChanged += this.MyPropertyGroupDescription_PropertyChanged;
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
                this.viewSource.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action) this.RefreshView);
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