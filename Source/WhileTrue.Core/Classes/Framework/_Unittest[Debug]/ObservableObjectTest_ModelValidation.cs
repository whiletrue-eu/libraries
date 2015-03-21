using System;
using System.ComponentModel;
using NUnit.Framework;

namespace WhileTrue.Classes.Framework._Unittest
{
    [TestFixture]
    public class ObservableObjectTest_ModelValidation
    {
        private class TestValidationObject : ObservableObject
        {
            private int property1;
            private int property2;
            private string messagePart;

            public TestValidationObject()
            {
                this.AddValidationForProperty(() => Property1)
                    .AddValidation(
                        property1 => property1 == this.Property2,
                        _ => string.Format("not equal{0}", this.MessagePart)
                    );
                this.AddValidationForProperty(() => Property2)
                    .AddValidation(
                        property2 => property2 == this.Property1,
                        _ => string.Format("not equal{0}", this.MessagePart)
                    );
                this.AddValidationForProperty(() => ExceptionProperty)
                    .AddValidation(
                        value => Validate(value),
                        value => Message(value)
                    );
            }

            private static string Message(ExceptionTest value)
            {
                if (value == ExceptionTest.ThrowInMessage)
                {
                    throw new InvalidOperationException();
                }
                return "Message";
            }

            private static bool Validate(ExceptionTest value)
            {
                if (value == ExceptionTest.ThrowInValidation)
                {
                    throw new InvalidOperationException();
                }
                return false;
            }


            public int Property1
            {
                set
                {
                    this.SetAndInvoke(()=>Property1, ref this.property1, value);
                }
                private get
                {
                    return this.property1;
                }
            }

            public int Property2
            {
                set
                {
                    this.SetAndInvoke(()=>Property2, ref this.property2, value);
                }
                private get
                {
                    return this.property2;
                }
            }    
            
            public enum ExceptionTest
            {
                DontThrow,
                ThrowInValidation,
                ThrowInMessage
            }

            private ExceptionTest exceptionProperty;
            public ExceptionTest ExceptionProperty
            {
                set
                {
                    this.SetAndInvoke(() => ExceptionProperty, ref this.exceptionProperty, value);
                }
                private get
                {
                    return this.exceptionProperty;
                }
            }

            public string MessagePart
            {
                set
                {
                    this.SetAndInvoke(()=>MessagePart, ref this.messagePart, value);
                }
                private get
                {
                    return this.messagePart;
                }
            }
        }

        [Test]
        public void DataErrorInfo_shall_be_reevaluated_when_properties_change()
        {
            TestValidationObject Object = new TestValidationObject();
            Assert.AreEqual("", ((IDataErrorInfo)Object)["Property1"]);
            Assert.AreEqual("", ((IDataErrorInfo)Object)["Property2"]);

            Object.Property1 = 42;

            Assert.AreEqual("not equal", ((IDataErrorInfo)Object)["Property1"]);
            Assert.AreEqual("not equal", ((IDataErrorInfo)Object)["Property2"]);

            Object.MessagePart = ": must be both 42";

            Assert.AreEqual("not equal: must be both 42", ((IDataErrorInfo)Object)["Property1"]);
            Assert.AreEqual("not equal: must be both 42", ((IDataErrorInfo)Object)["Property2"]);

            Object.Property2 = 42;

            Assert.AreEqual("", ((IDataErrorInfo)Object)["Property1"]);
            Assert.AreEqual("", ((IDataErrorInfo)Object)["Property2"]);
        }

        [Test]
        public void message_shall_be_ignored_if_exception_occurs_during_validation_or_message()
        {
            TestValidationObject Object = new TestValidationObject();
            
            Object.ExceptionProperty = TestValidationObject.ExceptionTest.DontThrow;
            Assert.AreEqual("Message", ((IDataErrorInfo)Object)["ExceptionProperty"]);
            
            Object.ExceptionProperty = TestValidationObject.ExceptionTest.ThrowInValidation;
            Assert.AreEqual("", ((IDataErrorInfo)Object)["ExceptionProperty"]);

            Object.ExceptionProperty = TestValidationObject.ExceptionTest.ThrowInMessage;
            Assert.AreEqual("", ((IDataErrorInfo)Object)["ExceptionProperty"]);
        }

    }
}