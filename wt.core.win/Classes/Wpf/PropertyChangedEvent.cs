using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Allows to attach to the model (using the <c>Attach</c> attached dependency property) in order to throw routed events for property change notifications geenrated by the model.
    /// Use in conjunction with <see cref="PropertyChangedEventExtension"/> to react on the routed events (e.g. through an event trigger)
    /// </summary>
    public class PropertyChangedEvent
    {
        private static readonly DependencyPropertyEventManager attachChangedEventManager = new DependencyPropertyEventManager();

        // ReSharper disable MemberCanBePrivate.Global
        ///<summary>
        /// Register a drag and drop source handler for a given UI element
        ///</summary>
        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach", typeof(INotifyPropertyChanged), typeof(PropertyChangedEvent), new FrameworkPropertyMetadata(null, PropertyChangedEvent.attachChangedEventManager.ChangedHandler));

        static PropertyChangedEvent()
        {
            PropertyChangedEvent.attachChangedEventManager.Changed += PropertyChangedEvent.AttachChanged;
        }


        private static void AttachChanged(object dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            dependencyObject.DbC_Assure(value => value is FrameworkElement || value is FrameworkContentElement, "Attach can only be used on Framework(Content)Element");

            if( e.OldValue is INotifyPropertyChanged)
            {
                INotifyPropertyChanged EventSource = (INotifyPropertyChanged)e.OldValue;
                EventConnector.GetEventConnector(EventSource).RemoveTarget((DependencyObject)dependencyObject);
            }

            if( e.NewValue is INotifyPropertyChanged)
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
                if (EventConnector.eventHandlers.ContainsKey(eventSource) == false)
                {
                    EventConnector Connector = new EventConnector(eventSource);
                    EventConnector.eventHandlers.Add(eventSource, Connector);
                    return Connector;
                }
                else
                {
                    return EventConnector.eventHandlers[eventSource];
                }
            }

            private readonly INotifyPropertyChanged eventSource;
            private readonly List<WeakReference<DependencyObject>> targets = new List<WeakReference<DependencyObject>>();

            private EventConnector(INotifyPropertyChanged eventSource)
            {
                this.eventSource = eventSource;
                this.eventSource.PropertyChanged += this.InvokeEvent;
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
                                        ((FrameworkElement) TheTarget).RaiseEvent(new RoutedEventArgs(PropertyChangedRoutedEventFactory.GetRoutedEvent($"{eventArgs.PropertyName}Changed")));
                                    });
                        }
                        else if (TheTarget is FrameworkContentElement)
                        {
                            TheTarget.Dispatcher.BeginInvoke(
                                DispatcherPriority.Normal,
                                (Action) delegate
                                    {
                                        ((FrameworkContentElement) TheTarget).RaiseEvent(new RoutedEventArgs(PropertyChangedRoutedEventFactory.GetRoutedEvent($"{eventArgs.PropertyName}Changed")));
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
                    // ReSharper disable once PossibleUnintendedReferenceComparison
                    if (TheTarget == dependencyObject)
                    {
                        // Remove the requested item
                        this.targets.Remove(Target);
                    }
                }
                // Clean up if no targets are left
                if (this.targets.Count == 0)
                {
                    EventConnector.eventHandlers.Remove(this.eventSource);
                }
            }
        }

        // ReSharper disable UnusedMember.Global
        /// <summary>
        /// Attach a model to generate property notification routed events
        /// </summary>
        public static void SetAttach(DependencyObject element, INotifyPropertyChanged source)
        {
            element.SetValue(PropertyChangedEvent.AttachProperty, source);
        }

        /// <summary>
        /// Gets the attached model
        /// </summary>
        public static INotifyPropertyChanged GetAttach(DependencyObject element)
        {
            return (INotifyPropertyChanged)element.GetValue(PropertyChangedEvent.AttachProperty);
        }
        // ReSharper enable UnusedMember.Global


    }
}