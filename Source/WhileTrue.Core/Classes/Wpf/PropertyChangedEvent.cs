using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Wpf
{
    public class PropertyChangedEvent
    {
        private static readonly DependencyPropertyEventManager attachChangedEventManager = new DependencyPropertyEventManager();

        // ReSharper disable MemberCanBePrivate.Global
        ///<summary>
        /// Register a drag and drop source handler for a given UI element
        ///</summary>
        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach", typeof(INotifyPropertyChanged), typeof(PropertyChangedEvent), new FrameworkPropertyMetadata(null, attachChangedEventManager.ChangedHandler));

        static PropertyChangedEvent()
        {
            attachChangedEventManager.Changed += AttachChanged;
        }


        private static void AttachChanged(object dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            dependencyObject.DbC_Assure(value => value is FrameworkElement || value is FrameworkContentElement, "Attach can only be used on Framework(Content)Element");

            if( e.OldValue as INotifyPropertyChanged != null)
            {
                INotifyPropertyChanged EventSource = (INotifyPropertyChanged)e.OldValue;
                EventConnector.GetEventConnector(EventSource).RemoveTarget((DependencyObject)dependencyObject);
            }

            if( e.NewValue as INotifyPropertyChanged != null)
            {
                INotifyPropertyChanged EventSource = (INotifyPropertyChanged) e.NewValue;
                EventConnector.GetEventConnector(EventSource).AddTarget((DependencyObject)dependencyObject);
            }
        }

        private class EventConnector
        {
            private static readonly Dictionary<INotifyPropertyChanged, EventConnector> eventHandlers = new Dictionary<INotifyPropertyChanged, EventConnector>();

            public static EventConnector GetEventConnector(INotifyPropertyChanged eventSource)
            {
                if (eventHandlers.ContainsKey(eventSource) == false)
                {
                    EventConnector Connector = new EventConnector(eventSource);
                    eventHandlers.Add(eventSource, Connector);
                    return Connector;
                }
                else
                {
                    return eventHandlers[eventSource];
                }
            }

            private readonly INotifyPropertyChanged eventSource;
            private readonly List<WeakReference<DependencyObject>> targets = new List<WeakReference<DependencyObject>>();

            private EventConnector(INotifyPropertyChanged eventSource)
            {
                this.eventSource = eventSource;
                this.eventSource.PropertyChanged += InvokeEvent;
            }

            private void InvokeEvent( object sender, PropertyChangedEventArgs eventArgs)
            {
                foreach( WeakReference<DependencyObject> Target in this.targets.ToArray() )
                {
                    DependencyObject TheTarget; //save target to allow deferred call below
                    if (Target.TryGetTarget(out TheTarget))
                    {
                        if (TheTarget is FrameworkElement)
                        {
                            TheTarget.Dispatcher.BeginInvoke(
                                DispatcherPriority.Normal,
                                (Action) delegate
                                    {
                                        ((FrameworkElement) TheTarget).RaiseEvent(new RoutedEventArgs(PropertyChangedRoutedEventFactory.GetRoutedEvent(string.Format("{0}Changed", eventArgs.PropertyName))));
                                    });
                        }
                        else if (TheTarget is FrameworkContentElement)
                        {
                            TheTarget.Dispatcher.BeginInvoke(
                                DispatcherPriority.Normal,
                                (Action) delegate
                                    {
                                        ((FrameworkContentElement) TheTarget).RaiseEvent(new RoutedEventArgs(PropertyChangedRoutedEventFactory.GetRoutedEvent(string.Format("{0}Changed", eventArgs.PropertyName))));
                                    });
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    else
                    {
                        this.targets.Remove(Target);
                    }
                }
            }

            public void AddTarget(DependencyObject dependencyObject)
            {
                this.targets.Add(new WeakReference<DependencyObject>(dependencyObject));
            }

            public void RemoveTarget(DependencyObject dependencyObject)
            {
                foreach( WeakReference<DependencyObject> Target in this.targets.ToArray() )
                {
                    DependencyObject TheTarget;
                    if (Target.TryGetTarget(out TheTarget) == false)
                    {
                        // Remove garbage collected items
                        this.targets.Remove(Target);
                    }
                    if (TheTarget == dependencyObject)
                    {
                        // Remove the requested item
                        this.targets.Remove(Target);
                    }
                }
                // Clean up if no targets are left
                if (this.targets.Count == 0)
                {
                    eventHandlers.Remove(this.eventSource);
                }
            }
        }

        // ReSharper disable UnusedMember.Global
        public static void SetAttach(DependencyObject element, INotifyPropertyChanged source)
        {
            element.SetValue(AttachProperty, source);
        }

        public static INotifyPropertyChanged GetAttach(DependencyObject element)
        {
            return (INotifyPropertyChanged)element.GetValue(AttachProperty);
        }
        // ReSharper enable UnusedMember.Global


    }
}