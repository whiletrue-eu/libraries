// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable 1591 //xml doc

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace WhileTrue.Classes.Logging._Unittest
{
    [TestFixture]
    public class DebugLoggerTest
    {
        [TearDown]
        public void TearDown()
        {
            DebugLogger.DisableLogging();
        }

        [Test]
        public void Log_if_logging_is_enabled_for_type()
        {
            DebugLogger.EnableLogging(typeof(DebugLoggerTest),LoggingLevel.Normal);

            TestListener Listener = new TestListener();
            Trace.Listeners.Add(Listener);

            DebugLogger.Write(this, LoggingLevel.Normal, () => "Test");

            Assert.IsTrue(Regex.IsMatch(Listener.ToString(), @"#(\d*): ID #\1 was assigned to WhileTrue.Classes.Logging._Unittest.DebugLoggerTest\n#\1: Test"));
        }

        [Test(Description="Performance Test. To get real results: Set compiler options: [x] Optimize code, Advanced... | Debug info: [none]. Rebuild manually")]
        public void Logging_shall_not_have_performance_impact_if_not_enabled()
        {
            Stopwatch Stopwatch = new Stopwatch();
         
            object Work = new object();

            Stopwatch.Reset();
            Stopwatch.Start();

            for (int Index = 0; Index < 1000000; Index++)
            {
                DebugLogger.Write(this, LoggingLevel.Normal, () => "");
                Work.ToString();
            }

            Stopwatch.Stop();

            long WithLoggingCall = Stopwatch.ElapsedTicks;

            Stopwatch.Reset();
            Stopwatch.Start();

            for (int Index = 0; Index < 1000000; Index++)
            {
                Work.ToString();
            }

            Stopwatch.Stop();

            long WithoutLoggingCall = Stopwatch.ElapsedTicks;

            Trace.WriteLine(string.Format("WithLoggingCall: {0}",WithLoggingCall));
            Trace.WriteLine(string.Format("WithoutLoggingCall: {0}", WithoutLoggingCall));

            Assert.LessOrEqual(WithLoggingCall, WithoutLoggingCall*4); // max factor of 4
        }

        [Test]
        public void Dont_log_if_logging_is_not_enabled()
        {
            TestListener Listener = new TestListener();
            Trace.Listeners.Add(Listener);

            DebugLogger.Write(this, LoggingLevel.Normal, () => "Test");

            Assert.AreEqual("",Listener.ToString());
        }


        [Test]
        public void Dont_log_if_logging_is_enabled_but_not_for_the_type()
        {
            DebugLogger.EnableLogging(typeof(string), LoggingLevel.Normal);

            TestListener Listener = new TestListener();
            Trace.Listeners.Add(Listener);

            DebugLogger.Write(this, LoggingLevel.Normal, () => "Test");

            Assert.AreEqual("", Listener.ToString());
        }

        
        private class TestListener : TraceListener
        {
            readonly StringBuilder message = new StringBuilder();
            #region Overrides of TraceListener

            public override void Write(string message)
            {
                this.message.Append(message);
            }

            public override void WriteLine(string message)
            {
                this.message.Append(message+"\n");
            }

            public override string ToString()
            {
                return this.message.ToString();
            }

            #endregion
        }
    }
}

