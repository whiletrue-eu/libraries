// ReSharper disable RedundantAssignment
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global
#pragma warning disable 1591
// ReSharper disable InconsistentNaming
using System;
using System.Diagnostics;
using NUnit.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Framework
{
    [TestFixture]
    public class WeakDelegateTest
    {
        private delegate void MyEventHandler(object sender, MyEventArgs e);
        private class MyEventArgs : EventArgs{}

        private class EventSource
        {
            public event MyEventHandler Event;

            public bool HandlerIsRegistered => this.Event != null;

            public void InvokeEvent()
            {
                this.Event(this, new MyEventArgs());
            }
        }

        private class EventSink
        {
            public EventSink(EventSource eventSource)
            {
                eventSource.Event += WeakDelegate.Connect<EventSink, EventSource, MyEventHandler, MyEventArgs>(
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
            EventSource EventSource = new EventSource();
            EventSink EventSink = new EventSink(EventSource);

            WeakReference<EventSink> SinkReference = new WeakReference<EventSink>(EventSink);

            EventSink = null;
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
            EventSource EventSource = new EventSource();
            
            new EventSink(EventSource);
            GC.Collect(); //should collect the instance created but not saved

            EventSource.InvokeEvent();

            Assert.IsFalse(EventSource.HandlerIsRegistered);
        }

        [Test,Ignore("Manual test")]
        public void performance_shall_not_degrade_extremely_when_using_weak_delegates()
        {
            Stopwatch Stopwatch = new Stopwatch();
            EventSource EventSource = new EventSource();

            MyEventHandler[] Delegates = new MyEventHandler[10000];
            for (int Index = 0; Index < Delegates.Length; Index++) { Delegates[Index] = delegate { }; }

            Stopwatch.Reset();
            Stopwatch.Start();
            Delegates.ForEach(value => EventSource.Event += value);
            Stopwatch.Stop();

            long AddHandlers = Stopwatch.ElapsedTicks;

            Stopwatch.Reset();
            Stopwatch.Start();
            for (int Index = 0; Index < 1000; Index++) { EventSource.InvokeEvent(); }
            Stopwatch.Stop();

            long InvokeHandlers = Stopwatch.ElapsedTicks;

            Stopwatch.Reset();
            Stopwatch.Start();
            Delegates.ForEach(value => EventSource.Event -= value);
            Stopwatch.Stop();

            long RemoveHandlers = Stopwatch.ElapsedTicks;

            
            object Target = new object();


            Stopwatch.Reset();
            Stopwatch.Start();
            Delegates.ForEach(value => EventSource.Event += WeakDelegate.Connect<object, EventSource, MyEventHandler, MyEventArgs>(Target, EventSource, (target, sender, e) => value(sender, e), (source, handler) => source.Event -= handler));
            Stopwatch.Stop();

            long AddHandlersWeak = Stopwatch.ElapsedTicks;

            Stopwatch.Reset();
            Stopwatch.Start();
            for (int Index = 0; Index < 1000; Index++) { EventSource.InvokeEvent(); }
            Stopwatch.Stop();

            long InvokeHandlersWeak = Stopwatch.ElapsedTicks;

            Target = null;
            GC.Collect();

            Stopwatch.Reset();
            Stopwatch.Start();
            EventSource.InvokeEvent();
            Stopwatch.Stop();

            long RemoveHandlersWeak = Stopwatch.ElapsedTicks;

            Debug.WriteLine($"Add: Normal:{AddHandlers}, Weak: {AddHandlersWeak} / {AddHandlersWeak/(1d + AddHandlers)}");
            Debug.WriteLine($"Add: Normal:{InvokeHandlers}, Weak: {InvokeHandlersWeak} / {InvokeHandlersWeak/(1d + InvokeHandlers)}");
            Debug.WriteLine($"Add: Normal:{RemoveHandlers}, Weak: {RemoveHandlersWeak} / {RemoveHandlersWeak/(1d + RemoveHandlers)}");

            Assert.Less(AddHandlersWeak / (1d+AddHandlers), 50d);
            Assert.Less(InvokeHandlersWeak / (1d+InvokeHandlers), 15d);
            Assert.Less(RemoveHandlersWeak / (1d+RemoveHandlers), 5d);
        }
    }
}