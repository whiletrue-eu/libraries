using System.Collections.Generic;
using System.Windows;

namespace WhileTrue.Classes.Wpf
{
    internal class DependencyPropertyEventManager
    {
        private readonly Dictionary<DependencyObject, List<DependencyPropertyChangedEventHandler>> eventHandler =
            new Dictionary<DependencyObject, List<DependencyPropertyChangedEventHandler>>();

        private DependencyPropertyChangedEventHandler changedHandler = delegate { };

        public void ChangedHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            changedHandler(dependencyObject, e);

            if (eventHandler.ContainsKey(dependencyObject))
                foreach (var Handler in eventHandler[dependencyObject].ToArray())
                    Handler(dependencyObject, e);
        }

        public void AddEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            if (!eventHandler.ContainsKey(dependencyObject))
                eventHandler.Add(dependencyObject, new List<DependencyPropertyChangedEventHandler>());

            var Handler = eventHandler[dependencyObject];
            Handler.Add(handler);
        }

        public void RemoveEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            if (eventHandler.ContainsKey(dependencyObject))
            {
                var Handler = eventHandler[dependencyObject];
                if (Handler.Contains(handler)) Handler.Remove(handler);

                if (Handler.Count == 0) eventHandler.Remove(dependencyObject);
            }
        }

        public event DependencyPropertyChangedEventHandler Changed
        {
            add => changedHandler += value;
            remove => changedHandler -= value;
        }
    }
}