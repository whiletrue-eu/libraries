using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using WhileTrue.Classes.Logging;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    ///     This class wraps a lambda expression in a way that captures <see cref="INotifyPropertyChanged " /> and
    ///     <see cref="INotifyCollectionChanged" /> events throughout the complete path of the expression.<br />
    ///     The events are attached during the execution of the expression through <see cref="Invoke" />. The thrown
    ///     event is then routed through the <see cref="Changed" /> event, preserving the original sender and
    ///     event arguments. When fired, all event handlers are deregistered and will only be registered again on the
    ///     next call to <see cref="Invoke" />.
    /// </summary>
    /// <typeparam name="TYpeOfExpression">expression to be wrapped</typeparam>
    public class NotifyChangeExpression<TYpeOfExpression>
    {
        private readonly List<object> collectionNotifications = new List<object>();
        private readonly List<Action> deregistrations = new List<Action>();

        private readonly Dictionary<object, List<MemberInfo>> propertyNotifications =
            new Dictionary<object, List<MemberInfo>>();

        /// <summary>
        ///     Creates the wrapper of the lambda expression
        /// </summary>
        /// <param name="value">expression to be wrapped</param>
        public NotifyChangeExpression(Expression<TYpeOfExpression> value)
        {
            Invoke = ((Expression<TYpeOfExpression>) new NotifyChangeExpressionVisitor(NotifyMemberAccess,
                NotifyValueRetrieved).Instrument(value)).Compile();
        }


        /// <summary>
        ///     returns the wrapped expression as a delegate that can be directly called
        /// </summary>
        public TYpeOfExpression Invoke { get; }

        private void NotifyValueRetrieved(object value)
        {
            if (value is INotifyCollectionChanged)
            {
                var NotifyCollectionChanged = (INotifyCollectionChanged) value;
                lock (this)
                {
                    if (collectionNotifications.Contains(value) == false)
                    {
                        DebugLogger.WriteLine(this, LoggingLevel.Verbose,
                            () => $"Attaching changing event on collection '{value}'");
                        var Handler = WeakDelegate
                            .Connect<NotifyChangeExpression<TYpeOfExpression>, INotifyCollectionChanged,
                                NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                                this,
                                NotifyCollectionChanged,
                                (target, sender, e) => target.NotifyChangeExpression_CollectionChanged(sender, e),
                                (source, handler) => source.CollectionChanged -= handler
                            );
                        NotifyCollectionChanged.CollectionChanged += Handler;
                        deregistrations.Add(delegate { NotifyCollectionChanged.CollectionChanged -= Handler; });
                        collectionNotifications.Add(value);
                    }
                }
            }
        }

        private void NotifyMemberAccess(object value, MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo && value is INotifyPropertyChanged)
                lock (this)
                {
                    List<MemberInfo> RegisteredProperties;

                    if (propertyNotifications.TryGetValue(value, out RegisteredProperties) == false)
                    {
                        DebugLogger.WriteLine(this, LoggingLevel.Verbose,
                            () => $"Attaching changing event on value '{value}'");
                        DebugLogger.WriteLine(this, LoggingLevel.Verbose,
                            () => $"Adding property '{memberInfo.Name}' to changing event of value '{value}'");

                        var NotifyPropertyChanged = (INotifyPropertyChanged) value;

                        var Handler = WeakDelegate
                            .Connect<NotifyChangeExpression<TYpeOfExpression>, INotifyPropertyChanged,
                                PropertyChangedEventHandler, PropertyChangedEventArgs>(
                                this,
                                NotifyPropertyChanged,
                                (target, sender, e) => target.NotifyPropertyChanged(sender, e),
                                (source, handler) => source.PropertyChanged -= handler
                            );
                        NotifyPropertyChanged.PropertyChanged += Handler;
                        deregistrations.Add(delegate { NotifyPropertyChanged.PropertyChanged -= Handler; });

                        RegisteredProperties = new List<MemberInfo>();
                        propertyNotifications.Add(value, RegisteredProperties);
                    }

                    RegisteredProperties.Add(memberInfo);
                }
        }

        /// <summary>
        ///     provides the event fired from within the expression. The sender and event args are
        ///     the original ones from the original event. The arguments are either of type
        ///     <see cref="PropertyChangedEventArgs" /> or <see cref="NotifyCollectionChangedEventArgs" />,
        ///     depending on the original event
        /// </summary>
        public event EventHandler<EventArgs> Changed = delegate { };

        private void NotifyChangeExpression_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvokeChanged(sender, e);
        }

        private void InvokeChanged(object sender, EventArgs e)
        {
            DebugLogger.WriteLine(this, LoggingLevel.Normal,
                () => $"Event received on {sender}: {DebugLogger.ToString(e)}");

            DeregisterAndClearPropertyInfos();
            Changed(sender, e);
        }

        private void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            lock (this)
            {
                if (propertyNotifications.ContainsKey(sender))
                    if (propertyNotifications[sender].Any(_ => _.Name == e.PropertyName))
                        InvokeChanged(sender, e);
            }
        }

        private void DeregisterAndClearPropertyInfos()
        {
            lock (this)
            {
                deregistrations.ForEach(deregister => deregister());
                deregistrations.Clear();
                propertyNotifications.Clear();
                collectionNotifications.Clear();
            }
        }

        /// <summary>
        ///     Converts the ChangeNotifyExpression to a delegate that can be called directly
        /// </summary>
        public static implicit operator TYpeOfExpression(NotifyChangeExpression<TYpeOfExpression> value)
        {
            return value.Invoke;
        }

        private class NotifyChangeExpressionVisitor : ExpressionVisitor
        {
            private readonly Action<object, MemberInfo> notifyMemberAccess;
            private readonly Action<object> notifyValueRetrieved;

            public NotifyChangeExpressionVisitor(Action<object, MemberInfo> notifyMemberAccess,
                Action<object> notifyValueRetrieved)
            {
                this.notifyMemberAccess = notifyMemberAccess;
                this.notifyValueRetrieved = notifyValueRetrieved;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != null)
                {
                    // Wraps the 'Expression' of the node, that returns the object whos proprty shall be retrieved with the notification call.
                    // The result is again wrapped, so that the resulting value is also notified.
                    Expression Member = Expression.Constant(node.Member);
                    Expression This = Expression.Constant(this, typeof(NotifyChangeExpressionVisitor));
                    //MemberExpression NewExpression = node.Update(Expression.Call(This, "NotifyMemberAccess", new[] {node.Expression.Type}, node.Expression, Member));
                    var NewExpression = Expression.MakeMemberAccess(
                        Expression.Call(This, "NotifyMemberAccess", new[] {node.Expression.Type}, node.Expression,
                            Member), node.Member);
                    return Expression.Call(This, "NotifyValueRetrieved", new[] {NewExpression.Type},
                        base.VisitMember(NewExpression));
                }

                //Member access is on a static member - INotifyPropertyChanged is not usable with this - maybe I have to add support for XXXChanged events through reflection?
                return base.VisitMember(node);
            }

            public Expression Instrument(Expression value)
            {
                return Visit(value);
            }

            // ReSharper disable UnusedMember.Local
            [UsedImplicitly]
            private T NotifyMemberAccess<T>(T value, MemberInfo member)
            {
                notifyMemberAccess(value, member);
                return value;
            }

            [UsedImplicitly]
            private T NotifyValueRetrieved<T>(T value)
            {
                notifyValueRetrieved(value);
                return value;
            }
            // ReSharper restore UnusedMember.Local
        }
    }
}