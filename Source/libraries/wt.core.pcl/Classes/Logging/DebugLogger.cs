using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Logging
{
    ///<summary>
    /// Implements logging for release builds that can be enabled for specific types, using the class hierarchy.
    /// The API is designed in a way, that it has virtually no procesinng overhead if not enabled.
    /// This is achieved by an API design that, before anything is processed, first checks whether logging is enabled.
    /// As this check is inlined by the compiler, only one conditional jump is left within the code
    ///</summary>
    [PublicAPI]
    public static class DebugLogger
    {
        private static bool isLoggingEnabled;
        private static readonly Dictionary<Type, LoggingLevel> loggingEnabledFor = new Dictionary<Type, LoggingLevel>();
        private static readonly List<ObjectReference> objects = new List<ObjectReference>();

        private class ObjectReference : WeakReference
        {
            private static long nextId;
            public ObjectReference(object target) : base(target)
            {
                this.Id = ObjectReference.nextId++;
            }

            public long Id { get; }
        }

        ///<summary>
        /// Enable logging witht he given logging level for instances of the given type or instances of types derived from the given type
        ///</summary>
        public static void EnableLogging( Type forType, LoggingLevel loggingLevel)
        {
            DebugLogger.isLoggingEnabled = true;

            if (DebugLogger.loggingEnabledFor.ContainsKey(forType))
            {
                DebugLogger.loggingEnabledFor.Remove(forType);
            }
            DebugLogger.loggingEnabledFor.Add(forType, loggingLevel);
        }

        ///<summary>
        /// Enable logging with the given logging level for instances of the given type or instances of types derived from the given type
        ///</summary>
        public static void EnableLogging<T>(LoggingLevel loggingLevel)
        {
            DebugLogger.EnableLogging(typeof(T), loggingLevel);
        }

        ///<summary>
        ///</summary>
        public static void DisableLogging()
        {
            DebugLogger.isLoggingEnabled = false;
            DebugLogger.loggingEnabledFor.Clear();
            DebugLogger.objects.Clear();
        }


        private static string GetCallerId(object caller)
        {
            //Clean up a bit first
            (from Object in DebugLogger.objects where Object.IsAlive == false select Object).ToArray().ForEach(x => DebugLogger.objects.Remove(x));
            
            //Find reference to object. Create one if none exists
            ObjectReference CallerReference = DebugLogger.objects.Find(reference => reference.Target == caller);
            if( CallerReference==null)
            {
                CallerReference = new ObjectReference(caller);
                DebugLogger.objects.Add(CallerReference);
                Debug.WriteLine( $"#{CallerReference.Id}: ID #{CallerReference.Id} was assigned to {caller}");
            }
            return $"#{CallerReference.Id}";
        }

        private static void InternalDoIfLoggingEnabled(object caller, LoggingLevel loggingLevel, Action action)
        {
            Type CallerType = caller.GetType();
            LoggingLevel? LoggingLevel = DebugLogger.GetLoggingLevel(CallerType);

            if (LoggingLevel.HasValue && LoggingLevel >= loggingLevel)
            {
                action();
            }
        }

        private static LoggingLevel? GetLoggingLevel([NotNull] Type callerType)
        {
            LoggingLevel LoggingLevel = LoggingLevel.Normal; //to avoid compiler error. From algorithm point of view, logging level will always be set if TypeFound is true...
            bool TypeFound = false;
            Type TypeToSearch = callerType;

            while( TypeFound == false && TypeToSearch != null )
            {
                //try to find the given type
                TypeFound = DebugLogger.loggingEnabledFor.TryGetValue(TypeToSearch, out LoggingLevel);
                //if not found and type is generic, also try to find generic base type
                if (TypeFound == false && TypeToSearch.IsGenericType())
                {
                    TypeFound = DebugLogger.loggingEnabledFor.TryGetValue(TypeToSearch.GetGenericTypeDefinition(), out LoggingLevel);
                }

                //for next round, try base type
                TypeToSearch = null; //TypeToSearch.BaseType;
            }
            
            return TypeFound ? LoggingLevel : (LoggingLevel?)null;
        }

        ///<summary>
        ///</summary>
        ///<param name="caller"></param>
        ///<param name="loggingLevel"></param>
        ///<param name="message"></param>
        public static void WriteLine(object caller, LoggingLevel loggingLevel, Func<string> message)
        {
            if (DebugLogger.isLoggingEnabled) DebugLogger.InternalWriteLine(caller, loggingLevel, message);
        }

        private static void InternalWriteLine(object caller, LoggingLevel loggingLevel, Func<string> message)
        {
            DebugLogger.InternalDoIfLoggingEnabled(caller, loggingLevel, () => Debug.WriteLine($"{DebugLogger.GetCallerId(caller)}: {message()}"));
        }

        ///<summary>
        /// Returns a meaningful log message for the given event args
        ///</summary>
        public static string ToString(EventArgs e)
        {
            if(e is NotifyCollectionChangedEventArgs)
            {
                return $"Collection changed (Action: {((NotifyCollectionChangedEventArgs) e).Action})";
            }
            else if (e is PropertyChangedEventArgs)
            {
                return $"Property '{((PropertyChangedEventArgs) e).PropertyName}' changed";
            }
            else
            {
                return e.ToString();
            }
        }
    }

    ///<summary>
    ///</summary>
    public enum LoggingLevel
    {
        ///<summary/>
        Normal,
        ///<summary/>
        Verbose,
    }
}
