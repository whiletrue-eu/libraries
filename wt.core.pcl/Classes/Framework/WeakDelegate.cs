using System;
using System.Linq.Expressions;
using System.Reflection;

namespace WhileTrue.Classes.Framework
{
    ///<summary>
    /// Provides a method to wrap a eventhandler so that it is a weak event handler which does not keep alive the target object
    ///</summary>
    public static class WeakDelegate
    {
                /// <summary>
        /// Creates the weak event handler and wires it to the source event. This version is simpler to use, but only supports the <c>EventHandler</c> generic event handler type.
        /// </summary>
        /// <typeparam name="TEventArgsType">Type of the EventArgs used by the handler</typeparam>
        /// <typeparam name="TArgetType">Type of the target type that implements the event handler to be wrapped</typeparam>
        /// <typeparam name="TSourceType">Type of the source type that implements the event handler that should be connected</typeparam>
        /// <param name="target">the instance on the target that shall receive the event</param>
        /// <param name="source">the instance that shall be used to register the event</param>
        /// <param name="handler">A delegate that calls the event handler. Delegate parameters are: (target, sender, event args). The target instance as well as parameters are supplied in the handler call</param>
        /// <param name="unregister">A delegate to unregister the event handler in case the target instance was collected by the GC</param>
        /// <returns></returns>
        public static EventHandler<TEventArgsType> Connect<TArgetType, TSourceType, TEventArgsType>(
            TArgetType target,
            TSourceType source,
            Action<TArgetType, object, TEventArgsType> handler,
            Action<TSourceType, EventHandler<TEventArgsType>> unregister
            )
            where TSourceType : class
            where TArgetType : class
            where TEventArgsType : EventArgs
                {
                    return WeakDelegate.Connect<TArgetType, TSourceType, EventHandler<TEventArgsType>, TEventArgsType>(target, source, handler, unregister);
                }

        /// <summary>
        /// Creates the weak event handler and wires it to the source event
        /// </summary>
        /// <typeparam name="THandlerType">Type of the EventHandler. Must be compatible to the event</typeparam>
        /// <typeparam name="TEventArgsType">Type of the EventArgs used by the handler</typeparam>
        /// <typeparam name="TArgetType">Type of the target type that implements the event handler to be wrapped</typeparam>
        /// <typeparam name="TSourceType">Type of the source type that implements the event handler that should be connected</typeparam>
        /// <param name="target">the instance on the target that shall receive the event</param>
        /// <param name="source">the instance that shall be used to register the event</param>
        /// <param name="handler">A delegate that calls the event handler. Delegate parameters are: (target, sender, event args). The target instance as well as parameters are supplied in the handler call</param>
        /// <param name="unregister">A delegate to unregister the event handler in case the target instance was collected by the GC</param>
        /// <returns></returns>
        public static THandlerType Connect<TArgetType, TSourceType, THandlerType, TEventArgsType>(
            TArgetType target,
            TSourceType source,
            Action<TArgetType, object, TEventArgsType> handler,
            Action<TSourceType, THandlerType> unregister
            )
            where TSourceType : class
            where TArgetType : class
            where TEventArgsType : class
            where THandlerType : class
        {
            //Create the Weak Reference on the target
            WeakReference<TArgetType> TargetReference = new WeakReference<TArgetType>(target);

            //Create the handler and initialize with null. This is needed to use it in the delegate below. 
            //The real value will be written later. As it is a closure, it will be updated inside the handler
            //(Which is then a self-reference ;-)
            THandlerType EventHandler = null;

            //Create the event handler that is attached to the real event.
            //This handler has to be some known delegate, because the generic parameter HandlerType
            //cannot be constraint to delegate type (not supported by C#/.Net)
            Action<object, TEventArgsType> Action = (sender, e) =>
                                                       {
                                                           TArgetType Target;
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

            ParameterExpression SenderParameter = Expression.Parameter(typeof (object));
            ParameterExpression EventArgsParameter = Expression.Parameter(typeof (TEventArgsType));
            EventHandler = Expression.Lambda<THandlerType>(Expression.Call(Expression.Constant(Action.Target), Action.GetMethodInfo(), SenderParameter, EventArgsParameter), SenderParameter, EventArgsParameter).Compile();
            return EventHandler;
        }
    }
}