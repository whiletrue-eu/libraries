using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace WhileTrue.Classes.Logging
{
    ///<summary>
    ///</summary>
    public static class DebugLogger
    {
        private static bool isLoggingEnabled;
        private static readonly Dictionary<Type, LoggingLevel> loggingEnabledFor = new Dictionary<Type, LoggingLevel>();
        private static ObjectIDGenerator objectIDGenerator = new ObjectIDGenerator();
        private static readonly List<WeakReference> objects = new List<WeakReference>();

        ///<summary>
        /// Enable logging witht he given logging level for instances of the given type or instances of types derived from the given type
        ///</summary>
        public static void EnableLogging( Type forType, LoggingLevel loggingLevel)
        {
            isLoggingEnabled = true;

            if (loggingEnabledFor.ContainsKey(forType))
            {
                loggingEnabledFor.Remove(forType);
            }
            loggingEnabledFor.Add(forType, loggingLevel);

            //NotifyTypeIfAwareOfLogging(forType);
        }

        ///<summary>
        /// Enable logging witht he given logging level for instances of the given type or instances of types derived from the given type
        ///</summary>
        public static void EnableLogging<Type>(LoggingLevel loggingLevel)
        {
            DebugLogger.EnableLogging(typeof(Type), loggingLevel);
        }

        ///<summary>
        ///</summary>
        public static void DisableLogging()
        {
            isLoggingEnabled = false;
            loggingEnabledFor.Clear();
            objects.Clear();
            objectIDGenerator = new ObjectIDGenerator();
        }

        /*private static void NotifyTypeIfAwareOfLogging(Type type)
        {
            MethodInfo NotifyLoggingEnabledMethodInfo = type.GetMethod("NotifyLoggingEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            if (NotifyLoggingEnabledMethodInfo != null)
            {
                NotifyLoggingEnabledMethodInfo.Invoke(null, null);
            }
        }
*/

        ///<summary>
        ///</summary>
        ///<param name="caller"></param>
        ///<param name="loggingLevel"></param>
        ///<param name="message"></param>
        public static void Write(object caller, LoggingLevel loggingLevel, Func<string> message)
        {
            if (isLoggingEnabled) InternalWrite(caller, loggingLevel, message);
        }

        private static void InternalWrite(object caller, LoggingLevel loggingLevel, Func<string> message)
        {
            InternalDoIfLoggingEnabled(caller, loggingLevel,() => Trace.Write(message(), GetCallerID(caller)));
        }

        private static string GetCallerID(object caller)
        {
            //Clean up a bit first
            (from Object in objects where Object.IsAlive == false select Object).ToArray().Select(x => objects.Remove(x));
            
            //Find reference to object. Create one if none exists
            WeakReference CallerReference = objects.Find(reference => reference.Target == caller);
            if( CallerReference==null)
            {
                CallerReference = new WeakReference(caller);
                objects.Add(CallerReference);
            }

            //Create or get object ID of reference
            bool FirstTime;
            long ID = objectIDGenerator.GetId(CallerReference, out FirstTime);
            string CallerID = string.Format("#{0}", ID);

            Trace.WriteLineIf(FirstTime, String.Format("ID #{0} was assigned to {1}", ID, caller.ToString()), CallerID);

            return CallerID;
        }

        private static void InternalDoIfLoggingEnabled(object caller, LoggingLevel loggingLevel, Action action)
        {
            Type CallerType = caller.GetType();
            LoggingLevel? LoggingLevel = GetLoggingLevel(CallerType);

            if (LoggingLevel.HasValue && LoggingLevel >= loggingLevel)
            {
                action();
            }
        }

        private static LoggingLevel? GetLoggingLevel([CodeInspection.NotNull] Type callerType)
        {
            LoggingLevel LoggingLevel = LoggingLevel.Normal; //to avoid compiler error. From algorithm point of view, logging level will always be set if TypeFound is true...
            bool TypeFound = false;
            Type TypeToSearch = callerType;

            while( TypeFound == false && TypeToSearch != null )
            {
                //try to find the given type
                TypeFound = loggingEnabledFor.TryGetValue(TypeToSearch, out LoggingLevel);
                //if not found and type is generic, also try to find generic base type
                if (TypeFound == false && TypeToSearch.IsGenericType)
                {
                    TypeFound = loggingEnabledFor.TryGetValue(TypeToSearch.GetGenericTypeDefinition(), out LoggingLevel);
                }

                //for next round, try base type
                TypeToSearch = TypeToSearch.BaseType;
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
            if (isLoggingEnabled) InternalWriteLine(caller, loggingLevel, message);
        }

        private static void InternalWriteLine(object caller, LoggingLevel loggingLevel, Func<string> message)
        {
            InternalDoIfLoggingEnabled(caller, loggingLevel, () => Trace.WriteLine(message(), GetCallerID(caller)));
        }

        ///<summary>
        /// Returns a meaningful log message for the given event args
        ///</summary>
        public static string ToString(EventArgs e)
        {
            if(e is NotifyCollectionChangedEventArgs)
            {
                return string.Format("Collection changed (Action: {0})",((NotifyCollectionChangedEventArgs)e).Action);
            }
            else if (e is PropertyChangedEventArgs)
            {
                return string.Format("Property '{0}' changed", ((PropertyChangedEventArgs) e).PropertyName);
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
