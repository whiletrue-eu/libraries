using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// This utility class contains an attachable property (<see cref="FixProperty"/>) that can be set on CollectionViewSource instances.
    /// If set to true, it fixes the issue that grouping on the standard collection view source does not react onto change notifications of the corresponding property on the collection items.
    /// </summary>
    [UsedImplicitly]
    public class CollectionViewSource
    {
        ///<summary>
        /// used on CollectionViewSource instances to fix the grouping property event reception issue
        ///</summary>
        public static readonly DependencyProperty FixProperty = DependencyProperty.RegisterAttached("Fix", typeof(string), typeof(CollectionViewSource), new FrameworkPropertyMetadata(CollectionViewSource.FixChanged));

        /// <summary/>
        public static void SetFix(DependencyObject d, string value)
        {
            d.SetValue(CollectionViewSource.FixProperty,value);
        }
        
        /// <summary/>
        public static string GetFix(DependencyObject d)
        {
            return (string) d.GetValue(CollectionViewSource.FixProperty);

        }

        private static void FixChanged(DependencyObject d, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (d is System.Windows.Data.CollectionViewSource)
            {
                System.Windows.Data.CollectionViewSource ViewSource = (System.Windows.Data.CollectionViewSource)d;
                ((INotifyCollectionChanged)((System.Windows.Data.CollectionViewSource)d).GroupDescriptions).CollectionChanged +=
                    delegate
                    {
                        CollectionViewSource.FixGroupDescriptions(ViewSource);
                    };

                CollectionViewSource.FixGroupDescriptions(ViewSource);
            }
            else
            {
                throw new InvalidOperationException("Fix can only be set on CollectionViewSource");
            }
        }

        private static readonly List<System.Windows.Data.CollectionViewSource> avoidReentry = new List<System.Windows.Data.CollectionViewSource>();

        private static void FixGroupDescriptions(System.Windows.Data.CollectionViewSource viewSource)
        {
            if (CollectionViewSource.avoidReentry.Contains(viewSource) == false)
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
                                CollectionViewSource.avoidReentry.Add(viewSource);
                                viewSource.GroupDescriptions.RemoveAt(ChangeIndex);
                                viewSource.GroupDescriptions.Insert(ChangeIndex, NewDescription);
                                CollectionViewSource.avoidReentry.Remove(viewSource);
                            });
                    }
                }
            }
        }
    }
}