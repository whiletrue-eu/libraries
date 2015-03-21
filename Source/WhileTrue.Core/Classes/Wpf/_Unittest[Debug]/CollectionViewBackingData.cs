using System;
using System.Collections.Generic;
using System.Timers;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes._Unittest
{
    public class CollectionViewBackingData : ObservableObject
    {
        private readonly List<Data> collection;
        private readonly List<Data2> collection2;

        public CollectionViewBackingData()
        {
            this.collection = new List<Data> { new Data("One", "G1"), new Data("Two", "G1"), new Data("Three", "G1"), new Data("Four", "G2"), new Data("Five", "G2"), new Data("Six", "G3"), new Data("Seven", "G3") };
            this.collection2 = new List<Data2> { new Data2(), new Data2(), new Data2(), new Data2(), new Data2() };
        }

        public List<Data> Collection
        {
            get { return this.collection; }
        }
        public List<Data2> Collection2
        {
            get { return this.collection2; }
        }

        public class Data : ObservableObject
        {
            private static Random random = new Random();
            private readonly string name;
            private string groupname;
            private readonly Timer timer;

            public Data(string name, string groupname)
            {
                this.name = name;
                this.groupname = groupname;

                this.timer = new Timer(random.Next(1000,3000));
                this.timer.Elapsed += this.timer_Elapsed;
                this.timer.Enabled = true;
            }

            void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                this.SetAndInvoke(()=>Groupname, ref this.groupname, "G" + random.Next(1, 4));

            }

            public string Name
            {
                get { return this.name; }
            }

            public string Groupname
            {
                get { return this.groupname; }
            }
        }
        public class Data2 : ObservableObject
        {
            private static Random random = new Random();
            private string name;
            private readonly Timer timer;

            public Data2()
            {
                this.timer = new Timer(random.Next(1000, 3000));
                this.timer.Elapsed += this.timer_Elapsed;
                this.timer.Enabled = true;
            }

            void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                this.SetAndInvoke(() => Name, ref this.name, random.Next(1, 4).ToString());

            }

            public string Name
            {
                get { return this.name; }
            }
        }
    }
}