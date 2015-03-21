using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using WhileTrue.Classes.CodeInspection;
using WhileTrue.Classes.Logging;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Framework
{
    public static class ObservableExpressionFactory
    {
        public class EventSink
        {
            private readonly Action<object, EventArgs> eventCallback;
            private readonly Dictionary<CompareObjectByReferenceWrapper, List<string>> propertyNotifications = new Dictionary<CompareObjectByReferenceWrapper, List<string>>();
            private readonly List<CompareObjectByReferenceWrapper> collectionNotifications = new List<CompareObjectByReferenceWrapper>();
            private readonly List<Action> deregistrations = new List<Action>();

            public EventSink(Action<object,EventArgs> eventCallback)
            {
                this.eventCallback = eventCallback;
            }

            private void NotifyEvent(object sender, EventArgs e)
            {
                this.DeregisterAndClearPropertyInfos();
                this.eventCallback(sender, e);
            }

            internal void NotifyValueRetrieved(object value)
            {
                CompareObjectByReferenceWrapper ValueAsKey = new CompareObjectByReferenceWrapper(value);
                if (value is INotifyCollectionChanged)
                {
                    INotifyCollectionChanged NotifyCollectionChanged = (INotifyCollectionChanged) value;
                    lock (this.collectionNotifications)
                    {
                        if (this.collectionNotifications.Contains(ValueAsKey) == false)
                        {
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Attaching changing event on collection '{0}'", value));
                            NotifyCollectionChangedEventHandler Handler = WeakDelegate.Connect<EventSink, INotifyCollectionChanged, NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                                this,
                                NotifyCollectionChanged,
                                (target, sender, e) => target.NotifyChangeExpression_CollectionChanged(sender, e),
                                (source, handler) => source.CollectionChanged -= handler
                                );
                            NotifyCollectionChanged.CollectionChanged += Handler;
                            lock (this.deregistrations)
                            {
                                this.deregistrations.Add(delegate { NotifyCollectionChanged.CollectionChanged -= Handler; });
                            }

                            this.collectionNotifications.Add(ValueAsKey);
                        }
                    }
                }
            }

            internal void NotifyMemberAccess(object value, MemberInfo memberInfo)
            {
                CompareObjectByReferenceWrapper ValueAsKey = new CompareObjectByReferenceWrapper(value);
                if (memberInfo is PropertyInfo && value is INotifyPropertyChanged)
                {
                    string PropertyName = memberInfo.Name;
                    lock (this.propertyNotifications)
                    {
                        if (this.propertyNotifications.ContainsKey(ValueAsKey) == false)
                        {
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Attaching changing event on value '{0}'", value));

                            INotifyPropertyChanged NotifyPropertyChanged = (INotifyPropertyChanged)value;

                            PropertyChangedEventHandler Handler = WeakDelegate.Connect<EventSink, INotifyPropertyChanged, PropertyChangedEventHandler, PropertyChangedEventArgs>(
                                this,
                                NotifyPropertyChanged,
                                (target, sender, e) => target.NotifyPropertyChanged(sender, e),
                                (source, handler) => source.PropertyChanged -= handler
                                );
                            NotifyPropertyChanged.PropertyChanged += Handler;
                            lock (this.deregistrations)
                            {
                                this.deregistrations.Add(delegate { NotifyPropertyChanged.PropertyChanged -= Handler; });
                            }

                            List<string> RegisteredPropertyNames = new List<string>();
                            this.propertyNotifications.Add(ValueAsKey, RegisteredPropertyNames);
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Adding property '{0}' to changing event of value '{1}'", PropertyName, value));
                            RegisteredPropertyNames.Add(PropertyName);
                        }
                        else
                        {
                            List<string> RegisteredPropertyNames = this.propertyNotifications[ValueAsKey];
                            if (RegisteredPropertyNames.Contains(PropertyName) == false)
                            {
                                DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Adding property '{0}' to changing event of value '{1}'", PropertyName, value));
                                RegisteredPropertyNames.Add(PropertyName);
                            }
                            else
                            {
                                DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("No add property '{0}' of value '{1}': is already added", PropertyName, value));
                            }
                        }
                    }
                }
            }
            private void InvokeChanged(object sender, EventArgs e)
            {
                DebugLogger.WriteLine(this, LoggingLevel.Normal, () => string.Format("Event received on {0}: {1}", sender, DebugLogger.ToString(e)));

                this.DeregisterAndClearPropertyInfos();
                this.NotifyEvent(sender, e);
            }

            private void NotifyChangeExpression_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                this.InvokeChanged(sender, e);
            }

            private void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                CompareObjectByReferenceWrapper SenderAsKey = new CompareObjectByReferenceWrapper(sender);
                lock (this.propertyNotifications)
                {
                    if (this.propertyNotifications.ContainsKey(SenderAsKey))
                    {
                        if (this.propertyNotifications[SenderAsKey].Contains(e.PropertyName))
                        {
                            this.InvokeChanged(sender, e);
                        }
                    }
                }
            }

            private void DeregisterAndClearPropertyInfos()
            {
               IEnumerable<Action> Deregistrations;
                lock (this.deregistrations)
                {
                    Deregistrations = this.deregistrations.ToArray();
                    this.deregistrations.Clear();
                }
                Deregistrations.ForEach(deregister => deregister());
                lock (this.propertyNotifications)
                {
                    this.propertyNotifications.Clear();
                }
                lock (this.collectionNotifications)
                {
                    this.collectionNotifications.Clear();
                }
            }
        }

        public static Action<EventSink> Compile(Expression<Action> expression)
        {
            return ((Expression<Action<EventSink>>)new ObservableExpressionVisitor().Instrument(expression,null)).Compile();
        }

        public static Action<TParam1,EventSink> Compile<TParam1>(Expression<Action<TParam1>> expression)
        {
            return ((Expression<Action<TParam1, EventSink>>)new ObservableExpressionVisitor().Instrument(expression, null)).Compile();
        }

        public static Action<TParam1,TParam2,EventSink> Compile<TParam1,TParam2>(Expression<Action<TParam1,TParam2>> expression)
        {
            return ((Expression<Action<TParam1, TParam2, EventSink>>)new ObservableExpressionVisitor().Instrument(expression,null)).Compile();
        }

        public static Func<EventSink, TReturn> Compile<TReturn>(Expression<Func<TReturn>> expression)
        {
            return ((Expression<Func<EventSink, TReturn>>)new ObservableExpressionVisitor().Instrument(expression, typeof(TReturn))).Compile();
        }

        public static Func<TParam1, EventSink, TReturn> Compile<TParam1, TReturn>(Expression<Func<TParam1, TReturn>> expression)
        {
            return ((Expression<Func<TParam1, EventSink, TReturn>>)new ObservableExpressionVisitor().Instrument(expression, typeof(TReturn))).Compile();
        }

        public static Func<TParam1, TParam2, EventSink, TReturn> Compile<TParam1, TParam2, TReturn>(Expression<Func<TParam1, TParam2, TReturn>> expression)
        {
            return ((Expression<Func<TParam1, TParam2, EventSink, TReturn>>)new ObservableExpressionVisitor().Instrument(expression, typeof(TReturn))).Compile();
        }


        private class ObservableExpressionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression eventSinkExpression = Expression.Parameter(typeof (EventSink));

            public Expression Instrument(LambdaExpression value, Type returnType)
            {
                LambdaExpression InstrumentedExpression = (LambdaExpression) this.Visit(value);

                ParameterExpression[] InnerParameters = InstrumentedExpression.Parameters.Union(new[]{this.eventSinkExpression}).ToArray();
                ParameterExpression[] Parameters = InnerParameters.Select(_ => Expression.Parameter(_.Type)).ToArray();

                LambdaExpression InnerLambdaExpression = Expression.Lambda(returnType !=null?Expression.Convert(InstrumentedExpression.Body, returnType):InstrumentedExpression.Body, InstrumentedExpression.Name, false, InnerParameters);
                InvocationExpression LambdaInvokeExpression = Expression.Invoke(InnerLambdaExpression, Parameters);

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
                    MemberExpression NewExpression = Expression.MakeMemberAccess(Expression.Call(This, "NotifyMemberAccess", new[] { node.Expression.Type }, this.eventSinkExpression, node.Expression, Member), node.Member);
                    return Expression.Call(This, "NotifyValueRetrieved", new[] { NewExpression.Type }, this.eventSinkExpression, base.VisitMember(NewExpression));
                }
                else
                {
                    //Member access is on a static member - INotifyPropertyChanged is not usable with this - maybe I have to add support for XXXChanged events through reflection?
                    return base.VisitMember(node);
                }
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
                CompareObjectByReferenceWrapper Other = other as CompareObjectByReferenceWrapper;
                return Other != null && object.ReferenceEquals(this.value, Other.value);
            }

            public override int GetHashCode()
            {
                return this.value.GetHashCode();
            }
        }
    }
}