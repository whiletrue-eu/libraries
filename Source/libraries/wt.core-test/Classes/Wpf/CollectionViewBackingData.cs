using System;
using System.Collections.Generic;
using System.Timers;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Wpf
{
    public class CollectionViewBackingData : ObservableObject
    {
        public CollectionViewBackingData()
        {
            this.Collection = new List<Data> { new Data("One", "G1"), new Data("Two", "G1"), new Data("Three", "G1"), new Data("Four", "G2"), new Data("Five", "G2"), new Data("Six", "G3"), new Data("Seven", "G3") };
            this.Collection2 = new List<Data2> { new Data2(), new Data2(), new Data2(), new Data2(), new Data2() };
        }

        public List<Data> Collection { get; }

        public List<Data2> Collection2 { get; }

        public class Data : ObservableObject
        {
            private static Random random = new Random();
            private string groupname;
            private readonly Timer timer;

            public Data(string name, string groupname)
            {
                this.Name = name;
                this.groupname = groupname;

                this.timer = new Timer(Data.random.Next(1000,3000));
                this.timer.Elapsed += this.timer_Elapsed;
                this.timer.Enabled = true;
            }

            void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                this.SetAndInvoke(nameof(this.Groupname), ref this.groupname, "G" + Data.random.Next(1, 4));

            }

            public string Name { get; }

            public string Groupname => this.groupname;
        }
        public class Data2 : ObservableObject
        {
            private static Random random = new Random();
            private string name;
            private readonly Timer timer;

            public Data2()
            {
                this.timer = new Timer(Data2.random.Next(1000, 3000));
                this.timer.Elapsed += this.timer_Elapsed;
                this.timer.Enabled = true;
            }

            void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                this.SetAndInvoke(nameof(this.Name), ref this.name, Data2.random.Next(1, 4).ToString());

            }

            public string Name => this.name;
        }
    }
}