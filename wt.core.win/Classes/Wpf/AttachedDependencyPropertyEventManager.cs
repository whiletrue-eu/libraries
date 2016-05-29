using System.Collections.Generic;
using System.Windows;

namespace WhileTrue.Classes.Wpf
{
    internal class DependencyPropertyEventManager
    {
        private readonly Dictionary<DependencyObject, List<DependencyPropertyChangedEventHandler>> eventHandler = new Dictionary<DependencyObject, List<DependencyPropertyChangedEventHandler>>();
        private DependencyPropertyChangedEventHandler changedHandler = delegate{};

        public void ChangedHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            this.changedHandler(dependencyObject, e);
            
            if (this.eventHandler.ContainsKey(dependencyObject))
            {
                foreach (DependencyPropertyChangedEventHandler Handler in this.eventHandler[dependencyObject].ToArray())
                {
                    Handler(dependencyObject, e);
                }
            }

        }

        public void AddEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            if (!this.eventHandler.ContainsKey(dependencyObject))
            {
                this.eventHandler.Add(dependencyObject, new List<DependencyPropertyChangedEventHandler>());
            }

            List<DependencyPropertyChangedEventHandler> Handler = this.eventHandler[dependencyObject];
            Handler.Add(handler);
        }

        public void RemoveEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            if (this.eventHandler.ContainsKey(dependencyObject))
            {
                List<DependencyPropertyChangedEventHandler> Handler = this.eventHandler[dependencyObject];
                if (Handler.Contains(handler))
                {
                    Handler.Remove(handler);
                }

                if (Handler.Count == 0)
                {
                    //Cleanup
                    this.eventHandler.Remove(dependencyObject);
                }
            }
        }

        public event DependencyPropertyChangedEventHandler Changed
        {
            add
            {
                this.changedHandler += value;
            }
            remove
            {
                this.changedHandler -= value;
            }
        }
    }
}