using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace WhileTrue.Classes.Wpf._Unittest_Debug_
{
    /// <summary/>
    public partial class CrossThreadCollectionWrapperWindow
    {
        private Data data;
        private static readonly Random random = new Random();

        public CrossThreadCollectionWrapperWindow()
        {
            InitializeComponent();
            this.data = new Data();
            this.DataContext = this.data;
        }

        public class Data
        {
            public Data()
            {
                this.Items = new ObservableCollection<DataItem>();
            }

            public ObservableCollection<DataItem> Items { get; private set; }

        }

        public class DataItem
        {
            private readonly string name;

            public DataItem(string name)
            {
                this.name = name;
            }

            public string Name
            {
                get { return this.name; }
            }

            public override string ToString()
            {
                return this.name;
            }
        }

        private void AddItems(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
                {
                    for(int Index=0; Index<10;Index++)
                    {
                        int InsertIndex=random.Next(0, this.data.Items.Count);
                        this.data.Items.Insert(InsertIndex,new DataItem(string.Format("Item {0}",  DateTime.Now.ToString())));
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
                            int RemoveIndex = this.data.Items.Count == 1 ? 0 : random.Next(0, this.data.Items.Count - 1);
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
                    int RemoveIndex = this.data.Items.Count == 1 ? 0 : random.Next(0, this.data.Items.Count - 1);
                    this.data.Items.RemoveAt(RemoveIndex);
                }
            }
        }

        private void AddItemsSync(object sender, RoutedEventArgs e)
        {
            for (int Index = 0; Index < 10; Index++)
            {
                int InsertIndex = random.Next(0, this.data.Items.Count);
                this.data.Items.Insert(InsertIndex, new DataItem(string.Format("Item {0}", DateTime.Now.ToString())));
            }
        }
    }
}
