using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace WhileTrue.Classes.Wpf.CrossThread
{
    /// <summary />
    public partial class CrossThreadCollectionWrapperWindow
    {
        private static readonly Random random = new Random();
        private readonly Data data;

        public CrossThreadCollectionWrapperWindow()
        {
            InitializeComponent();
            data = new Data();
            DataContext = data;
        }

        private void AddItems(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                for (var Index = 0; Index < 10; Index++)
                {
                    var InsertIndex = random.Next(0, data.Items.Count);
                    data.Items.Insert(InsertIndex, new DataItem($"Item {DateTime.Now.ToString()}"));
                }
            });
        }

        private void RemoveItems(object sender, RoutedEventArgs e)
        {
            if (data.Items.Count > 0)
                ThreadPool.QueueUserWorkItem(delegate
                {
                    for (var Index = 0; Index < 10; Index++)
                    {
                        var RemoveIndex = data.Items.Count == 1 ? 0 : random.Next(0, data.Items.Count - 1);
                        data.Items.RemoveAt(RemoveIndex);
                    }
                });
        }

        private void RemoveItemsSync(object sender, RoutedEventArgs e)
        {
            if (data.Items.Count > 0)
                for (var Index = 0; Index < 10; Index++)
                {
                    var RemoveIndex = data.Items.Count == 1 ? 0 : random.Next(0, data.Items.Count - 1);
                    data.Items.RemoveAt(RemoveIndex);
                }
        }

        private void AddItemsSync(object sender, RoutedEventArgs e)
        {
            for (var Index = 0; Index < 10; Index++)
            {
                var InsertIndex = random.Next(0, data.Items.Count);
                data.Items.Insert(InsertIndex, new DataItem($"Item {DateTime.Now.ToString()}"));
            }
        }

        public class Data
        {
            public Data()
            {
                Items = new ObservableCollection<DataItem>();
            }

            public ObservableCollection<DataItem> Items { get; }
        }

        public class DataItem
        {
            public DataItem(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}