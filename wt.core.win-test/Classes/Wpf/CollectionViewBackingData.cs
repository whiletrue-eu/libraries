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
            Collection = new List<Data>
            {
                new Data("One", "G1"), new Data("Two", "G1"), new Data("Three", "G1"), new Data("Four", "G2"),
                new Data("Five", "G2"), new Data("Six", "G3"), new Data("Seven", "G3")
            };
            Collection2 = new List<Data2> {new Data2(), new Data2(), new Data2(), new Data2(), new Data2()};
        }

        public List<Data> Collection { get; }

        public List<Data2> Collection2 { get; }

        public class Data : ObservableObject
        {
            private static readonly Random random = new Random();
            private readonly Timer timer;
            private string groupname;

            public Data(string name, string groupname)
            {
                Name = name;
                this.groupname = groupname;

                timer = new Timer(random.Next(1000, 3000));
                timer.Elapsed += timer_Elapsed;
                timer.Enabled = true;
            }

            public string Name { get; }

            public string Groupname => groupname;

            private void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                SetAndInvoke(nameof(Groupname), ref groupname, "G" + random.Next(1, 4));
            }
        }

        public class Data2 : ObservableObject
        {
            private static readonly Random random = new Random();
            private readonly Timer timer;
            private string name;

            public Data2()
            {
                timer = new Timer(random.Next(1000, 3000));
                timer.Elapsed += timer_Elapsed;
                timer.Enabled = true;
            }

            public string Name => name;

            private void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                SetAndInvoke(nameof(Name), ref name, random.Next(1, 4).ToString());
            }
        }
    }
}