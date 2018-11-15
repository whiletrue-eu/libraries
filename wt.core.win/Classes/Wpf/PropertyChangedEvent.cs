using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    ///     Allows to attach to the model (using the <c>Attach</c> attached dependency property) in order to throw routed
    ///     events for property change notifications geenrated by the model.
    ///     Use in conjunction with <see cref="PropertyChangedEventExtension" /> to react on the routed events (e.g. through an
    ///     event trigger)
    /// </summary>
    public class PropertyChangedEvent
    {
        private static readonly DependencyPropertyEventManager attachChangedEventManager =
            new DependencyPropertyEventManager(); // ReSharper disable MemberCanBePrivate.Global

        /// <summary>
        ///     Register a drag and drop source handler for a given UI element
        /// </summary>
        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach",
            typeof(INotifyPropertyChanged), typeof(PropertyChangedEvent),
            new FrameworkPropertyMetadata(null, attachChangedEventManager.ChangedHandler));

        static PropertyChangedEvent()
        {
            attachChangedEventManager.Changed += AttachChanged;
        }


        private static void AttachChanged(object dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            dependencyObject.DbC_Assure(value => value is FrameworkElement || value is FrameworkContentElement,
                "Attach can only be used on Framework(Content)Element");

            if (e.OldValue is INotifyPropertyChanged)
            {
                var EventSource = (INotifyPropertyChanged) e.OldValue;
                EventConnector.GetEventConnector(EventSource).RemoveTarget((DependencyObject) dependencyObject);
            }

            if (e.NewValue is INotifyPropertyChanged)
            {
                var EventSource = (INotifyPropertyChanged) e.NewValue;
                EventConnector.GetEventConnector(EventSource).AddTarget((DependencyObject) dependencyObject);
            }
        }

        private class EventConnector
        {
            private static readonly Dictionary<INotifyPropertyChanged, EventConnector> eventHandlers =
                new Dictionary<INotifyPropertyChanged, EventConnector>();

            private readonly INotifyPropertyChanged eventSource;

            private readonly List<WeakReference<DependencyObject>>
                targets = new List<WeakReference<DependencyObject>>();

            private EventConnector(INotifyPropertyChanged eventSource)
            {
                this.eventSource = eventSource;
                this.eventSource.PropertyChanged += InvokeEvent;
            }

            public static EventConnector GetEventConnector(INotifyPropertyChanged eventSource)
            {
                if (eventHandlers.ContainsKey(eventSource) == false)
                {
                    var Connector = new EventConnector(eventSource);
                    eventHandlers.Add(eventSource, Connector);
                    return Connector;
                }

                return eventHandlers[eventSource];
            }

            private void InvokeEvent(object sender, PropertyChangedEventArgs eventArgs)
            {
                foreach (var Target in targets.ToArray())
                {
                    DependencyObject TheTarget; //save target to allow deferred call below
                    if (Target.TryGetTarget(out TheTarget))
                    {
                        if (TheTarget is FrameworkElement)
                            TheTarget.Dispatcher.BeginInvoke(
                                DispatcherPriority.Normal,
                                (Action) delegate
                                {
                                    ((FrameworkElement) TheTarget).RaiseEvent(
                                        new RoutedEventArgs(
                                            PropertyChangedRoutedEventFactory.GetRoutedEvent(
                                                $"{eventArgs.PropertyName}Changed")));
                                });
                        else if (TheTarget is FrameworkContentElement)
                            TheTarget.Dispatcher.BeginInvoke(
                                DispatcherPriority.Normal,
                                (Action) delegate
                                {
                                    ((FrameworkContentElement) TheTarget).RaiseEvent(
                                        new RoutedEventArgs(
                                            PropertyChangedRoutedEventFactory.GetRoutedEvent(
                                                $"{eventArgs.PropertyName}Changed")));
                                });
                        else
                            throw new InvalidOperationException();
                    }
                    else
                    {
                        targets.Remove(Target);
                    }
                }
            }

            public void AddTarget(DependencyObject dependencyObject)
            {
                targets.Add(new WeakReference<DependencyObject>(dependencyObject));
            }

            public void RemoveTarget(DependencyObject dependencyObject)
            {
                foreach (var Target in targets.ToArray())
                {
                    DependencyObject TheTarget;
                    if (Target.TryGetTarget(out TheTarget) == false) targets.Remove(Target);
                    // ReSharper disable once PossibleUnintendedReferenceComparison
                    if (TheTarget == dependencyObject) targets.Remove(Target);
                }

                // Clean up if no targets are left
                if (targets.Count == 0) eventHandlers.Remove(eventSource);
            }
        }

        // ReSharper disable UnusedMember.Global
        /// <summary>
        ///     Attach a model to generate property notification routed events
        /// </summary>
        public static void SetAttach(DependencyObject element, INotifyPropertyChanged source)
        {
            element.SetValue(AttachProperty, source);
        }

        /// <summary>
        ///     Gets the attached model
        /// </summary>
        public static INotifyPropertyChanged GetAttach(DependencyObject element)
        {
            return (INotifyPropertyChanged) element.GetValue(AttachProperty);
        }

        // ReSharper enable UnusedMember.Global
    }
}