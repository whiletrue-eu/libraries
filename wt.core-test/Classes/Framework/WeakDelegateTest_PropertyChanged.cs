// ReSharper disable RedundantAssignment
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global
#pragma warning disable 1591
// ReSharper disable InconsistentNaming
using System;
using System.ComponentModel;
using System.Diagnostics;
using NUnit.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Framework
{
    [TestFixture]
    public class WeakDelegateTest_PropertyChanged
    {
        private class EventSource
        {
            public event PropertyChangedEventHandler Event;

            public bool HandlerIsRegistered => this.Event != null;

            public void InvokeEvent()
            {
                this.Event(this, new PropertyChangedEventArgs(""));
            }
        }

        private class EventSink
        {
            public EventSink(EventSource eventSource)
            {
                eventSource.Event += WeakDelegate.Connect<EventSink, EventSource, PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    this,
                    eventSource,
                    (target, sender, e) => target.EventInvoked(target, e),
                    (source,handler) => source.Event -= handler
                    );
            }




            private void EventInvoked(object sender, EventArgs e)
            {
                this.EventWasInvoked=true;
            }

            public bool EventWasInvoked { get; private set; }
        }

        [Test]
        public void target_shall_be_collected_if_only_event_handler_is_a_reference()
        {
            WeakReference<EventSink> Create(EventSource eventSource)
            {
                EventSink EventSink = new EventSink(eventSource);

                WeakReference<EventSink> sinkReference = new WeakReference<EventSink>(EventSink);
                return sinkReference;
            }

            EventSource EventSource = new EventSource();
            WeakReference<EventSink> SinkReference = Create(EventSource);

            GC.Collect();

            EventSink Target;
            Assert.IsFalse(SinkReference.TryGetTarget(out Target));
        }

        [Test]
        public void eventhandler_shall_be_called_if_event_is_fired()
        {
            EventSource EventSource = new EventSource();
            EventSink EventSink = new EventSink(EventSource);

            EventSource.InvokeEvent();

            Assert.IsTrue(EventSink.EventWasInvoked);
        }

        [Test]
        public void eventhandler_shall_be_unregistered_on_next_event_if_target_was_collected()
        {
            void Create(EventSource eventSource)
            {
                new EventSink(eventSource);
            }

            EventSource EventSource = new EventSource();
            
            Create(EventSource);
            GC.Collect(); //should collect the instance created but not saved

            EventSource.InvokeEvent();

            Assert.IsFalse(EventSource.HandlerIsRegistered);
        }

        [Test]
        public void performance_shall_not_degrade_extremely_when_using_weak_delegates()
        {
            Stopwatch Stopwatch = new Stopwatch();
            EventSource EventSource = new EventSource();

            PropertyChangedEventHandler[] Delegates = new PropertyChangedEventHandler[10000];
            for (int Index = 0; Index < Delegates.Length; Index++)
            {
                Delegates[Index] = delegate { };
            }

            Stopwatch.Reset();
            Stopwatch.Start();
            Delegates.ForEach(value => EventSource.Event += value);
            Stopwatch.Stop();

            long AddHandlers = Stopwatch.ElapsedTicks;

            Stopwatch.Reset();
            Stopwatch.Start();
            for (int Index = 0; Index < 1000; Index++)
            {
                EventSource.InvokeEvent();
            }

            Stopwatch.Stop();

            long InvokeHandlers = Stopwatch.ElapsedTicks;

            Stopwatch.Reset();
            Stopwatch.Start();
            Delegates.ForEach(value => EventSource.Event -= value);
            Stopwatch.Stop();

            long RemoveHandlers = Stopwatch.ElapsedTicks;
            long AddHandlersWeak;
            long InvokeHandlersWeak;

            void RunTest(object Target)
            {
                Stopwatch.Reset();
                Stopwatch.Start();
                Delegates.ForEach(value => EventSource.Event += WeakDelegate.Connect<object, EventSource, PropertyChangedEventHandler, PropertyChangedEventArgs>(Target, EventSource, (target, sender, e) => value(sender, e), (source, handler) => source.Event -= handler));
                Stopwatch.Stop();

                AddHandlersWeak = Stopwatch.ElapsedTicks;

                Stopwatch.Reset();
                Stopwatch.Start();
                for (int Index = 0; Index < 1000; Index++)
                {
                    EventSource.InvokeEvent();
                }

                Stopwatch.Stop();

                InvokeHandlersWeak = Stopwatch.ElapsedTicks;
            }

            RunTest(new object()); //use local function to make sure target object is really collected
            GC.Collect(int.MaxValue, GCCollectionMode.Forced, true);


            Stopwatch.Reset();
            Stopwatch.Start();
            EventSource.InvokeEvent();
            for (int Index = 0; Index < 1000; Index++)
            {
                EventSource.InvokeEvent();
            }
            Stopwatch.Stop();

            long RemoveHandlersWeak = Stopwatch.ElapsedTicks;

            Console.WriteLine($"Add: Normal:{AddHandlers}, Weak: {AddHandlersWeak} / {AddHandlersWeak / (double) AddHandlers:P}");
            Console.WriteLine($"Invoke: Normal:{InvokeHandlers}, Weak: {InvokeHandlersWeak} / {InvokeHandlersWeak / (double) InvokeHandlers:P}");
            Console.WriteLine($"Remove: Normal:{RemoveHandlers}, Weak: {RemoveHandlersWeak} / {RemoveHandlersWeak / (double) RemoveHandlers:P}");

            Assert.Less(AddHandlersWeak / (double)AddHandlers, 5.0);
            Assert.Less(InvokeHandlersWeak / (double)InvokeHandlers, 10.0);
            Assert.Less(RemoveHandlersWeak / (double)RemoveHandlers, 1.5);
        }
    }
}