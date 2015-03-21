using System.Collections.Generic;
using System.Windows;

namespace WhileTrue.Classes.Framework
{
    internal class DependencyPropertyEventManager
    {
        private readonly Dictionary<DependencyObject, List<DependencyPropertyChangedEventHandler>> eventHandler = new Dictionary<DependencyObject, List<DependencyPropertyChangedEventHandler>>();
        private DependencyPropertyChangedEventHandler changedHandler = delegate{};

        public void ChangedHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            this.changedHandler(dependencyObject, e);
            
            if (eventHandler.ContainsKey(dependencyObject))
            {
                foreach (DependencyPropertyChangedEventHandler Handler in eventHandler[dependencyObject].ToArray())
                {
                    Handler(dependencyObject, e);
                }
            }

        }

        public void AddEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            if (!eventHandler.ContainsKey(dependencyObject))
            {
                eventHandler.Add(dependencyObject, new List<DependencyPropertyChangedEventHandler>());
            }

            List<DependencyPropertyChangedEventHandler> Handler = eventHandler[dependencyObject];
            Handler.Add(handler);
        }

        public void RemoveEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            if (eventHandler.ContainsKey(dependencyObject))
            {
                List<DependencyPropertyChangedEventHandler> Handler = eventHandler[dependencyObject];
                if (Handler.Contains(handler))
                {
                    Handler.Remove(handler);
                }

                if (Handler.Count == 0)
                {
                    //Cleanup
                    eventHandler.Remove(dependencyObject);
                }
            }
        }

        public event DependencyPropertyChangedEventHandler Changed
        {
            add
            {
                changedHandler += value;
            }
            remove
            {
                changedHandler -= value;
            }
        }
    }
}