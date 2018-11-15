﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using WhileTrue.Classes.Logging;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    ///     Factory class that creates an observable expression from a standard expression.
    ///     The observable expression can be called just like the standard expression, with the difference that an additional
    ///     parameter is added, the 'event sink'. This parameter receives a delegate that is called if
    ///     any of the properties or collections used when the expression evaluated has fired a change notification since the
    ///     last call.
    ///     The sink should be stored within the client of the expression so that it is not garbage collected, as the event
    ///     sink uses weak event listener, that allow any of the target objects to be garbage collected.
    /// </summary>
    [PublicAPI]
    public static class ObservableExpressionFactory
    {
        /// <summary>
        ///     Converts the given expression into a delegate that registeres notification handlers on every object that implements
        ///     <c>INotifyPropertyChanged</c> and <c>INotifyCollectionChanged</c>
        ///     while the expression is evaluated. The events are forwared to the event handler wrapped in the
        ///     <see cref="EventSink" /> parameter
        /// </summary>
        public static Action<EventSink> Compile(Expression<Action> expression)
        {
            return ((Expression<Action<EventSink>>) new ObservableExpressionVisitor().Instrument(expression, null))
                .Compile();
        }

        /// <summary>
        ///     Converts the given expression into a delegate that registeres notification handlers on every object that implements
        ///     <c>INotifyPropertyChanged</c> and <c>INotifyCollectionChanged</c>
        ///     while the expression is evaluated. The events are forwared to the event handler wrapped in the
        ///     <see cref="EventSink" /> parameter
        /// </summary>
        public static Action<TParam1, EventSink> Compile<TParam1>(Expression<Action<TParam1>> expression)
        {
            return ((Expression<Action<TParam1, EventSink>>) new ObservableExpressionVisitor().Instrument(expression,
                null)).Compile();
        }

        /// <summary>
        ///     Converts the given expression into a delegate that registeres notification handlers on every object that implements
        ///     <c>INotifyPropertyChanged</c> and <c>INotifyCollectionChanged</c>
        ///     while the expression is evaluated. The events are forwared to the event handler wrapped in the
        ///     <see cref="EventSink" /> parameter
        /// </summary>
        public static Action<TParam1, TParam2, EventSink> Compile<TParam1, TParam2>(
            Expression<Action<TParam1, TParam2>> expression)
        {
            return ((Expression<Action<TParam1, TParam2, EventSink>>) new ObservableExpressionVisitor().Instrument(
                expression, null)).Compile();
        }

        /// <summary>
        ///     Converts the given expression into a delegate that registeres notification handlers on every object that implements
        ///     <c>INotifyPropertyChanged</c> and <c>INotifyCollectionChanged</c>
        ///     while the expression is evaluated. The events are forwared to the event handler wrapped in the
        ///     <see cref="EventSink" /> parameter
        /// </summary>
        public static Func<EventSink, TReturn> Compile<TReturn>(Expression<Func<TReturn>> expression)
        {
            return ((Expression<Func<EventSink, TReturn>>) new ObservableExpressionVisitor().Instrument(expression,
                typeof(TReturn))).Compile();
        }

        /// <summary>
        ///     Converts the given expression into a delegate that registeres notification handlers on every object that implements
        ///     <c>INotifyPropertyChanged</c> and <c>INotifyCollectionChanged</c>
        ///     while the expression is evaluated. The events are forwared to the event handler wrapped in the
        ///     <see cref="EventSink" /> parameter
        /// </summary>
        public static Func<TParam1, EventSink, TReturn> Compile<TParam1, TReturn>(
            Expression<Func<TParam1, TReturn>> expression)
        {
            return ((Expression<Func<TParam1, EventSink, TReturn>>) new ObservableExpressionVisitor().Instrument(
                expression, typeof(TReturn))).Compile();
        }

        /// <summary>
        ///     Converts the given expression into a delegate that registeres notification handlers on every object that implements
        ///     <c>INotifyPropertyChanged</c> and <c>INotifyCollectionChanged</c>
        ///     while the expression is evaluated. The events are forwared to the event handler wrapped in the
        ///     <see cref="EventSink" /> parameter
        /// </summary>
        public static Func<TParam1, TParam2, EventSink, TReturn> Compile<TParam1, TParam2, TReturn>(
            Expression<Func<TParam1, TParam2, TReturn>> expression)
        {
            return ((Expression<Func<TParam1, TParam2, EventSink, TReturn>>) new ObservableExpressionVisitor()
                .Instrument(expression, typeof(TReturn))).Compile();
        }

        /// <summary>
        ///     Proxy between the objects queried within the expresson, and the single event handler given in the constructor
        ///     The sink should be stored within the client of the expression so that it is not garbage collected, as the event
        ///     sink uses weak event listener, that allow any of the target objects to be garbage collected.
        /// </summary>
        public class EventSink
        {
            private readonly List<CompareObjectByReferenceWrapper> collectionNotifications =
                new List<CompareObjectByReferenceWrapper>();

            private readonly List<Action> deregistrations = new List<Action>();
            private readonly Action<object, EventArgs> eventCallback;

            private readonly Dictionary<CompareObjectByReferenceWrapper, List<string>> propertyNotifications =
                new Dictionary<CompareObjectByReferenceWrapper, List<string>>();

            /// <summary>
            ///     creates the event sink, a proxy between the objects queried within the expresson, and the single event handler
            ///     given in the constructor
            ///     The sink should be stored within the client of the expression so that it is not garbage collected, as the event
            ///     sink uses weak event listener, that allow any of the target objects to be garbage collected.
            /// </summary>
            public EventSink(Action<object, EventArgs> eventCallback)
            {
                this.eventCallback = eventCallback;
            }

            private void NotifyEvent(object sender, EventArgs e)
            {
                DeregisterAndClearPropertyInfos();
                eventCallback(sender, e);
            }

            internal void NotifyValueRetrieved(object value)
            {
                var ValueAsKey = new CompareObjectByReferenceWrapper(value);
                if (value is INotifyCollectionChanged)
                {
                    var NotifyCollectionChanged = (INotifyCollectionChanged) value;
                    lock (this)
                    {
                        if (collectionNotifications.Contains(ValueAsKey) == false)
                        {
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose,
                                () => $"Attaching changing event on collection '{value}'");
                            var Handler = WeakDelegate
                                .Connect<EventSink, INotifyCollectionChanged, NotifyCollectionChangedEventHandler,
                                    NotifyCollectionChangedEventArgs>(
                                    this,
                                    NotifyCollectionChanged,
                                    (target, sender, e) => target.NotifyChangeExpression_CollectionChanged(sender, e),
                                    (source, handler) => source.CollectionChanged -= handler
                                );
                            NotifyCollectionChanged.CollectionChanged += Handler;
                            deregistrations.Add(delegate { NotifyCollectionChanged.CollectionChanged -= Handler; });

                            collectionNotifications.Add(ValueAsKey);
                        }
                    }
                }
            }

            internal void NotifyMemberAccess(object value, MemberInfo memberInfo)
            {
                var ValueAsKey = new CompareObjectByReferenceWrapper(value);
                if (memberInfo is PropertyInfo && value is INotifyPropertyChanged)
                {
                    var PropertyName = memberInfo.Name;
                    lock (this)
                    {
                        if (propertyNotifications.ContainsKey(ValueAsKey) == false)
                        {
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose,
                                () => $"Attaching changing event on value '{value}'");

                            var NotifyPropertyChanged = (INotifyPropertyChanged) value;

                            var Handler = WeakDelegate
                                .Connect<EventSink, INotifyPropertyChanged, PropertyChangedEventHandler,
                                    PropertyChangedEventArgs>(
                                    this,
                                    NotifyPropertyChanged,
                                    (target, sender, e) => target.NotifyPropertyChanged(sender, e),
                                    (source, handler) => source.PropertyChanged -= handler
                                );
                            NotifyPropertyChanged.PropertyChanged += Handler;
                            deregistrations.Add(delegate { NotifyPropertyChanged.PropertyChanged -= Handler; });

                            var RegisteredPropertyNames = new List<string>();
                            propertyNotifications.Add(ValueAsKey, RegisteredPropertyNames);
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose,
                                () => $"Adding property '{PropertyName}' to changing event of value '{value}'");
                            RegisteredPropertyNames.Add(PropertyName);
                        }
                        else
                        {
                            var RegisteredPropertyNames = propertyNotifications[ValueAsKey];
                            if (RegisteredPropertyNames.Contains(PropertyName) == false)
                            {
                                DebugLogger.WriteLine(this, LoggingLevel.Verbose,
                                    () => $"Adding property '{PropertyName}' to changing event of value '{value}'");
                                RegisteredPropertyNames.Add(PropertyName);
                            }
                            else
                            {
                                DebugLogger.WriteLine(this, LoggingLevel.Verbose,
                                    () => $"No add property '{PropertyName}' of value '{value}': is already added");
                            }
                        }
                    }
                }
            }

            private void InvokeChanged(object sender, EventArgs e)
            {
                DebugLogger.WriteLine(this, LoggingLevel.Normal,
                    () => $"Event received on {sender}: {DebugLogger.ToString(e)}");

                DeregisterAndClearPropertyInfos();
                NotifyEvent(sender, e);
            }

            private void NotifyChangeExpression_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                InvokeChanged(sender, e);
            }

            private void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                var SenderAsKey = new CompareObjectByReferenceWrapper(sender);
                var NeedToNotify = false;
                lock (this)
                {
                    if (propertyNotifications.ContainsKey(SenderAsKey))
                        if (propertyNotifications[SenderAsKey].Contains(e.PropertyName))
                            NeedToNotify = true;
                }

                if (NeedToNotify) InvokeChanged(sender, e);
            }

            private void DeregisterAndClearPropertyInfos()
            {
                lock (this)
                {
                    IEnumerable<Action> Deregistrations = deregistrations.ToArray();
                    deregistrations.Clear();
                    Deregistrations.ForEach(deregister => deregister());
                    propertyNotifications.Clear();
                    collectionNotifications.Clear();
                }
            }
        }


        private class ObservableExpressionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression eventSinkExpression = Expression.Parameter(typeof(EventSink));

            public Expression Instrument(LambdaExpression value, Type returnType)
            {
                var InstrumentedExpression = (LambdaExpression) Visit(value);

                var InnerParameters = InstrumentedExpression.Parameters.Union(new[] {eventSinkExpression}).ToArray();
                var Parameters = InnerParameters.Select(_ => Expression.Parameter(_.Type)).ToArray();

                var InnerLambdaExpression =
                    Expression.Lambda(
                        returnType != null
                            ? Expression.Convert(InstrumentedExpression.Body, returnType)
                            : InstrumentedExpression.Body, InstrumentedExpression.Name, false, InnerParameters);
                // ReSharper disable once CoVariantArrayConversion
                var LambdaInvokeExpression = Expression.Invoke(InnerLambdaExpression, Parameters);

                return Expression.Lambda(LambdaInvokeExpression, InstrumentedExpression.Name, false, Parameters);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != null)
                {
                    // Wraps the 'Expression' of the node, that returns the object whos proprty shall be retrieved with the notification call.
                    // The result is again wrapped, so that the resulting value is also notified.
                    Expression Member = Expression.Constant(node.Member);
                    Expression This = Expression.Constant(this, typeof(ObservableExpressionVisitor));
                    //MemberExpression NewExpression = node.Update(Expression.Call(This, "NotifyMemberAccess", new[] {node.Expression.Type}, node.Expression, Member));
                    var NewExpression = Expression.MakeMemberAccess(
                        Expression.Call(This, "NotifyMemberAccess", new[] {node.Expression.Type}, eventSinkExpression,
                            node.Expression, Member), node.Member);
                    return Expression.Call(This, "NotifyValueRetrieved", new[] {NewExpression.Type},
                        eventSinkExpression, base.VisitMember(NewExpression));
                }

                //Member access is on a static member - INotifyPropertyChanged is not usable with this - maybe I have to add support for XXXChanged events through reflection?
                return base.VisitMember(node);
            }

            // ReSharper disable UnusedMember.Local
            [UsedImplicitly]
            private T NotifyMemberAccess<T>(EventSink eventSink, T value, MemberInfo member)
            {
                eventSink.NotifyMemberAccess(value, member);
                return value;
            }

            [UsedImplicitly]
            private T NotifyValueRetrieved<T>(EventSink eventSink, T value)
            {
                eventSink.NotifyValueRetrieved(value);
                return value;
            }
            // ReSharper restore UnusedMember.Local
        }

        private class CompareObjectByReferenceWrapper
        {
            private readonly object value;

            public CompareObjectByReferenceWrapper(object value)
            {
                this.value = value;
            }

            public override bool Equals(object other)
            {
                var Other = other as CompareObjectByReferenceWrapper;
                return Other != null && ReferenceEquals(value, Other.value);
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }
        }
    }
}