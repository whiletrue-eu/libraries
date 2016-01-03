using System;
using System.ComponentModel;
using NUnit.Framework;

namespace WhileTrue.Classes.Framework
{
    [TestFixture]
    public class ObservableObjectTestModelValidation
    {
        private class TestValidationObject : ObservableObject
        {
            private int property1;
            private int property2;
            private string messagePart;

            public TestValidationObject()
            {
                this.AddValidationForProperty(() => this.Property1)
                    .AddValidation(
                        property1 => property1 == this.Property2,
                        _ => $"not equal{this.MessagePart}"
                    );
                this.AddValidationForProperty(() => this.Property2)
                    .AddValidation(
                        property2 => property2 == this.Property1,
                        _ => $"not equal{this.MessagePart}"
                    );
                this.AddValidationForProperty(() => this.ExceptionProperty)
                    .AddValidation(
                        value => TestValidationObject.Validate(value),
                        value => TestValidationObject.Message(value)
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
                    this.SetAndInvoke(nameof(this.Property1), ref this.property1, value);
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
                    this.SetAndInvoke(nameof(this.Property2), ref this.property2, value);
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
                    this.SetAndInvoke(nameof(this.ExceptionProperty), ref this.exceptionProperty, value);
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
                    this.SetAndInvoke(nameof(this.MessagePart), ref this.messagePart, value);
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
            Assert.AreEqual(new[] { ""}, ((INotifyDataErrorInfo)Object).GetErrors("Property1"));
            Assert.AreEqual(new[] { ""}, ((INotifyDataErrorInfo)Object).GetErrors("Property2"));

            Object.Property1 = 42;

            Assert.AreEqual(new[] { "not equal"}, ((INotifyDataErrorInfo)Object).GetErrors("Property1"));
            Assert.AreEqual(new[] { "not equal"}, ((INotifyDataErrorInfo)Object).GetErrors("Property2"));

            Object.MessagePart = ": must be both 42";

            Assert.AreEqual(new[] { "not equal: must be both 42"}, ((INotifyDataErrorInfo)Object).GetErrors("Property1"));
            Assert.AreEqual(new[] { "not equal: must be both 42"}, ((INotifyDataErrorInfo)Object).GetErrors("Property2"));

            Object.Property2 = 42;

            Assert.AreEqual(new[] { ""}, ((INotifyDataErrorInfo)Object).GetErrors("Property1"));
            Assert.AreEqual(new[] { ""}, ((INotifyDataErrorInfo)Object).GetErrors("Property2"));
        }

        [Test]
        public void message_shall_be_ignored_if_exception_occurs_during_validation_or_message()
        {
            TestValidationObject Object = new TestValidationObject();
            
            Object.ExceptionProperty = TestValidationObject.ExceptionTest.DontThrow;
            Assert.AreEqual(new[] { "Message"}, ((INotifyDataErrorInfo)Object).GetErrors("ExceptionProperty"));
            
            Object.ExceptionProperty = TestValidationObject.ExceptionTest.ThrowInValidation;
            Assert.AreEqual(new[] { ""}, ((INotifyDataErrorInfo)Object).GetErrors("ExceptionProperty"));

            Object.ExceptionProperty = TestValidationObject.ExceptionTest.ThrowInMessage;
            Assert.AreEqual(new[] { ""}, ((INotifyDataErrorInfo)Object).GetErrors("ExceptionProperty"));
        }

    }
}