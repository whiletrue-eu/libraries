using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace WhileTrue.Classes.Wpf.CrossThread
{
    /// <summary/>
    public partial class CrossThreadCollectionWrapperWindow
    {
        private Data data;
        private static readonly Random random = new Random();

        public CrossThreadCollectionWrapperWindow()
        {
            this.InitializeComponent();
            this.data = new Data();
            this.DataContext = this.data;
        }

        public class Data
        {
            public Data()
            {
                this.Items = new ObservableCollection<DataItem>();
            }

            public ObservableCollection<DataItem> Items { get; }

        }

        public class DataItem
        {
            public DataItem(string name)
            {
                this.Name = name;
            }

            public string Name { get; }

            public override string ToString()
            {
                return this.Name;
            }
        }

        private void AddItems(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
                {
                    for(int Index=0; Index<10;Index++)
                    {
                        int InsertIndex=CrossThreadCollectionWrapperWindow.random.Next(0, this.data.Items.Count);
                        this.data.Items.Insert(InsertIndex,new DataItem($"Item {DateTime.Now.ToString()}"));
                    }
                });
        }

        private void RemoveItems(object sender, RoutedEventArgs e)
        {
            if (this.data.Items.Count > 0)
            {
                ThreadPool.QueueUserWorkItem(delegate
                    {
                        for (int Index = 0; Index < 10; Index++)
                        {
                            int RemoveIndex = this.data.Items.Count == 1 ? 0 : CrossThreadCollectionWrapperWindow.random.Next(0, this.data.Items.Count - 1);
                            this.data.Items.RemoveAt(RemoveIndex);
                        }
                    });
            }
        }

        private void RemoveItemsSync(object sender, RoutedEventArgs e)
        {
            if (this.data.Items.Count > 0)
            {
                for (int Index = 0; Index < 10; Index++)
                {
                    int RemoveIndex = this.data.Items.Count == 1 ? 0 : CrossThreadCollectionWrapperWindow.random.Next(0, this.data.Items.Count - 1);
                    this.data.Items.RemoveAt(RemoveIndex);
                }
            }
        }

        private void AddItemsSync(object sender, RoutedEventArgs e)
        {
            for (int Index = 0; Index < 10; Index++)
            {
                int InsertIndex = CrossThreadCollectionWrapperWindow.random.Next(0, this.data.Items.Count);
                this.data.Items.Insert(InsertIndex, new DataItem($"Item {DateTime.Now.ToString()}"));
            }
        }
    }
}
