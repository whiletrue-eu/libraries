using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using WhileTrue.Classes.CodeInspection;
using WhileTrue.Classes.Logging;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// This class wraps a lambda expression in a way that captures <see cref="INotifyPropertyChanged "/> and
    /// <see cref="INotifyCollectionChanged"/> events throughout the complete path of the expression.<br/>
    /// The events are attached during the execution of the expression through <see cref="Invoke"/>. The thrown
    /// event is then routed through the <see cref="Changed"/> event, preserving the original sender and
    /// event arguments. When fired, all event handlers are deregistered and will only be registered again on the 
    /// next call to <see cref="Invoke"/>.
    /// </summary>
    /// <typeparam name="TypeOfExpression">expression to be wrapped</typeparam>
    public class NotifyChangeExpression<TypeOfExpression>
    {
        private readonly EventBindingMode eventBindingMode;
        private readonly TypeOfExpression action;
        private readonly Dictionary<CompareObjectByReferenceWrapper, List<string>> propertyNotifications = new Dictionary<CompareObjectByReferenceWrapper, List<string>>();
        private readonly List<CompareObjectByReferenceWrapper> collectionNotifications = new List<CompareObjectByReferenceWrapper>();
        private readonly List<Action> deregistrations = new List<Action>();

        /// <summary>
        /// Creates the wrapper of the lambda expression
        /// </summary>
        /// <param name="value">expression to be wrapped</param>
        /// <param name="eventBindingMode">
        /// event binding mode:
        /// <list>
        /// <item>
        /// <term><see cref="EventBindingMode.Strong"/></term>
        /// <description>Uses standard .Net event handlers</description>
        /// </item>
        /// <item>
        /// <term><see cref="EventBindingMode.Weak"/></term>
        /// <description>Uses weak event handlers that allow garbage collection of the expression, see <see cref="WeakDelegate"/> for details</description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="synchronizeThreads">forces thread synchronisation for changed events</param>
        public NotifyChangeExpression(Expression<TypeOfExpression> value, EventBindingMode eventBindingMode)
        {
            this.eventBindingMode = eventBindingMode;
            this.action = ((Expression<TypeOfExpression>)new NotifyChangeExpressionVisitor(this.NotifyMemberAccess, this.NotifyValueRetrieved).Instrument(value)).Compile();
        }

        private void NotifyValueRetrieved(object value)
        {
            CompareObjectByReferenceWrapper ValueAsKey = new CompareObjectByReferenceWrapper(value);
            if (value is INotifyCollectionChanged)
            {
                INotifyCollectionChanged NotifyCollectionChanged = (INotifyCollectionChanged)value;
                lock (this.collectionNotifications)
                {
                    if (this.collectionNotifications.Contains(ValueAsKey) == false)
                    {
                        DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Attaching changing event on collection '{0}'", value));
                        switch (this.eventBindingMode)
                        {
                            case EventBindingMode.Strong:
                                NotifyCollectionChanged.CollectionChanged += this.NotifyChangeExpression_CollectionChanged;
                                lock (this.deregistrations)
                                {
                                    this.deregistrations.Add(() => NotifyCollectionChanged.CollectionChanged -= this.NotifyChangeExpression_CollectionChanged);
                                }
                                break;
                            case EventBindingMode.Weak:
                                NotifyCollectionChangedEventHandler Handler = WeakDelegate.Connect<NotifyChangeExpression<TypeOfExpression>, INotifyCollectionChanged, NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
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
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        this.collectionNotifications.Add(ValueAsKey);
                    }
                }
            }
        }

        private void NotifyMemberAccess(object value, MemberInfo memberInfo)
        {
            CompareObjectByReferenceWrapper ValueAsKey = new CompareObjectByReferenceWrapper(value);
            if (memberInfo is PropertyInfo && value is INotifyPropertyChanged)
            {
                string PropertyName = memberInfo.Name;
                lock (this.propertyNotifications)
                {
                    if (this.propertyNotifications.ContainsKey(ValueAsKey) == false)
                    {
                        DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Attaching changing event on value '{1}'", PropertyName, value));
                        DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Adding property '{0}' to changing event of value '{1}'", PropertyName, value));

                        INotifyPropertyChanged NotifyPropertyChanged = (INotifyPropertyChanged) value;

                        switch (this.eventBindingMode)
                        {
                            case EventBindingMode.Strong:
                                NotifyPropertyChanged.PropertyChanged += this.NotifyPropertyChanged;
                                lock (this.deregistrations)
                                {
                                    this.deregistrations.Add(() => NotifyPropertyChanged.PropertyChanged -= this.NotifyPropertyChanged);
                                }
                                break;
                            case EventBindingMode.Weak:
                                PropertyChangedEventHandler Handler = WeakDelegate.Connect<NotifyChangeExpression<TypeOfExpression>, INotifyPropertyChanged, PropertyChangedEventHandler, PropertyChangedEventArgs>(
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
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        List<string> RegisteredPropertyNames = new List<string>();
                        RegisteredPropertyNames.Add(PropertyName);
                        this.propertyNotifications.Add(ValueAsKey, RegisteredPropertyNames);
                    }

                    else
                    {
                        List<string> RegisteredPropertyNames = this.propertyNotifications[ValueAsKey];
                        if (RegisteredPropertyNames.Contains(PropertyName) == false)
                        {
                            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => string.Format("Adding property '{0}' to chaning event of value '{1}'", PropertyName, value));
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

        /// <summary>
        /// provides the event fired from within the expression. The sender and event args are
        /// the original ones from the original event. The arguments are either of type
        /// <see cref="PropertyChangedEventArgs"/> or <see cref="CollectionChangeEventArgs"/>,
        /// depending on the original event
        /// </summary>
        public event EventHandler<EventArgs> Changed = delegate { };

        private void NotifyChangeExpression_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.InvokeChanged(sender, e);
        }

        private void InvokeChanged(object sender, EventArgs e)
        {
            DebugLogger.WriteLine(this, LoggingLevel.Normal, () => string.Format("Event received on {0}: {1}", sender, DebugLogger.ToString(e) ));

            this.DeregisterAndClearPropertyInfos();
            this.Changed(sender, e);
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
            lock (this.deregistrations)
            {
                this.deregistrations.ForEach(deregister => deregister());
                this.deregistrations.Clear();
            }
            lock (this.propertyNotifications)
            {
                this.propertyNotifications.Clear();
            }
            lock (this.collectionNotifications)
            {
                this.collectionNotifications.Clear();
            }
        }

        /// <summary>
        /// Converts the ChangeNotifyExpression to a delegate that can be called directly
        /// </summary>
        public static implicit operator TypeOfExpression(NotifyChangeExpression<TypeOfExpression> value)
        {
            return value.action;
        }

        /// <summary>
        /// returns the wrapped expression as a delegate that can be directly called
        /// </summary>
        public TypeOfExpression Invoke
        {
            get { return this.action; }
        }

        private class NotifyChangeExpressionVisitor : ExpressionVisitor
        {
            private readonly Action<object, MemberInfo> notifyMemberAccess;
            private readonly Action<object> notifyValueRetrieved;

            public NotifyChangeExpressionVisitor(Action<object, MemberInfo> notifyMemberAccess, Action<object> notifyValueRetrieved)
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
                    MemberExpression NewExpression = Expression.MakeMemberAccess(Expression.Call(This, "NotifyMemberAccess", new[] { node.Expression.Type }, node.Expression, Member), node.Member);
                    return Expression.Call(This, "NotifyValueRetrieved", new[] { NewExpression.Type }, base.VisitMember(NewExpression));
                }
                else
                {
                    //Member access is on a static member - INotifyPropertyChanged is not usable with this - maybe I have to add support for XXXChanged events through reflection?
                    return base.VisitMember(node);
                }
            }

            // ReSharper disable UnusedMember.Local
            [UsedImplicitly]
            private T NotifyMemberAccess<T>(T value, MemberInfo member)
            {
                this.notifyMemberAccess(value, member);
                return value;
            }

            [UsedImplicitly]
            private T NotifyValueRetrieved<T>(T value)
            {
                this.notifyValueRetrieved(value);
                return value;
            }
            // ReSharper restore UnusedMember.Local

            public Expression Instrument(Expression value)
            {
                return this.Visit(value);
            }
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
                return Other != null && object.ReferenceEquals(this.value,Other.value);
            }

            public override int GetHashCode()
            {
                return this.value.GetHashCode();
            }
        }
    }
}