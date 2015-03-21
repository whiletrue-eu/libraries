using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Framework
{
    ///<summary>
    /// Provides a method to wrap a eventhandler so that it is a weak event handler which does not keep alive the target object
    ///</summary>
    public static class WeakDelegate
    {
                /// <summary>
        /// Creates the weak event handler and wires it to the source event. This version is simpler to use, but only supports the <c>EventHandler<...></c> generic event handler type.
        /// </summary>
        /// <typeparam name="EventArgsType">Type of the EventArgs used by the handler</typeparam>
        /// <typeparam name="TargetType">Type of the target type that implements the event handler to be wrapped</typeparam>
        /// <typeparam name="SourceType">Type of the source type that implements the event handler that should be connected</typeparam>
        /// <param name="target">the instance on the target that shall receive the event</param>
        /// <param name="source">the instance that shall be used to register the event</param>
        /// <param name="handler">A delegate that calls the event handler. Delegate parameters are: (target, sender, event args). The target instance as well as parameters are supplied in the handler call</param>
        /// <param name="unregister">A delegate to unregister the event handler in case the target instance was collected by the GC</param>
        /// <returns></returns>
        public static EventHandler<EventArgsType> Connect<TargetType, SourceType, EventArgsType>(
            TargetType target,
            SourceType source,
            Action<TargetType, object, EventArgsType> handler,
            Action<SourceType, EventHandler<EventArgsType>> unregister
            )
            where SourceType : class
            where TargetType : class
            where EventArgsType : EventArgs
                {
                    return WeakDelegate.Connect<TargetType, SourceType, EventHandler<EventArgsType>, EventArgsType>(target, source, handler, unregister);
                }

        /// <summary>
        /// Creates the weak event handler and wires it to the source event
        /// </summary>
        /// <typeparam name="HandlerType">Type of the EventHandler. Must be compatible to the event</typeparam>
        /// <typeparam name="EventArgsType">Type of the EventArgs used by the handler</typeparam>
        /// <typeparam name="TargetType">Type of the target type that implements the event handler to be wrapped</typeparam>
        /// <typeparam name="SourceType">Type of the source type that implements the event handler that should be connected</typeparam>
        /// <param name="target">the instance on the target that shall receive the event</param>
        /// <param name="source">the instance that shall be used to register the event</param>
        /// <param name="handler">A delegate that calls the event handler. Delegate parameters are: (target, sender, event args). The target instance as well as parameters are supplied in the handler call</param>
        /// <param name="unregister">A delegate to unregister the event handler in case the target instance was collected by the GC</param>
        /// <returns></returns>
        public static HandlerType Connect<TargetType, SourceType, HandlerType, EventArgsType>(
            TargetType target,
            SourceType source,
            Action<TargetType, object, EventArgsType> handler,
            Action<SourceType, HandlerType> unregister
            )
            where SourceType : class
            where TargetType : class
            where EventArgsType : class
            where HandlerType : class
        {
            //Create the Weak Reference on the target
            WeakReference<TargetType> TargetReference = new WeakReference<TargetType>(target);

            //Create the handler and initialize with null. This is needed to use it in the delegate below. 
            //The real value will be written later. As it is a closure, it will be updated inside the handler
            //(Which is then a self-reference ;-)
            HandlerType EventHandler = null;

            //Create the event handler that is attached to the real event.
            //This handler has to be some known delegate, because the generic parameter HandlerType
            //cannot be constraint to delegate type (not supported by C#/.Net)
            Action<object, EventArgsType> Action = (sender, e) =>
                                                       {
                                                           TargetType Target;
                                                           if (TargetReference.TryGetTarget(out Target))
                                                           {
                                                               handler(Target, sender, e);
                                                           }
                                                           else
                                                           {
                                                               // ReSharper disable AccessToModifiedClosure
                                                               unregister(source, EventHandler);
                                                               // ReSharper restore AccessToModifiedClosure
                                                           }
                                                       };

            //Cast of delegates is not possible. But creating a new one works.
            //This works, because we used compatible types on the action above!
            Delegate[] Delegates = Action.GetInvocationList();
            EventHandler = Delegate.CreateDelegate(typeof (HandlerType), Delegates[0].Target, Delegates[0].Method) as HandlerType;

            return EventHandler;
        }
    }
}