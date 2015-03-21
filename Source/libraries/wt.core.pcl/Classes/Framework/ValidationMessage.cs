using System;
using System.Collections.Generic;
using System.Linq;

namespace WhileTrue.Classes.Framework
{
    ///<summary>
    /// Encapsulates a string message with a severity
    ///</summary>
    /// <remarks>
    /// The class allows the transport of the message through ordinary strings. 
    /// In case of casting, a prefix marker is appended to the string (see also <see cref="ErrorMarker"/>, 
    /// <see cref="WarningMarker"/> and <see cref="InfoMarker"/>). If the string is casted back into a 
    /// validation message, the marker is converted back into the corresponding severity
    /// </remarks>
    public class ValidationMessage
    {
        internal const string Separator="\f";

        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedMember.Global
        private const int MarkerLength = 4;
        /// <summary/>
        public const string ErrorMarker = @"(X) ";
        /// <summary/>
        public const string InfoMarker = @"(i) ";
        /// <summary/>
        public const string WarningMarker = @"/!\ ";

        ///<summary>
        /// Constructs a validation message with the given message and <see cref="ValidationSeverity.ImplicitError"/> severity.
        ///</summary>
        public ValidationMessage(string message)
            :this(ValidationSeverity.Error, message)
        {
        }

        ///<summary>
        /// Constructs a validation message with the given message and <see cref="ValidationSeverity.ImplicitError"/> severity.
        ///</summary>
        public ValidationMessage(string messageFormat, params object[] messageArgs)
            :this(ValidationSeverity.Error, messageFormat, messageArgs)
        {
        }

        ///<summary>
        /// Constructs a validation message with the given message and the given severity.
        ///</summary>
        public ValidationMessage(ValidationSeverity severity, string message)
        {
            this.Message = message;
            this.Severity = severity;
        }

        ///<summary>
        /// Constructs a validation message with the given message and the given severity allowing string.Format style message.
        ///</summary>
        public ValidationMessage(ValidationSeverity severity, string messageFormat,params object[] messageArgs)
        {
            this.Message = string.Format(messageFormat, messageArgs);
            this.Severity = severity;
        }

        ///<summary>
        /// Parses the given string and converts the severity markers into the severity of the message
        ///</summary>
        public static ValidationMessage Parse(string value)
        {
            if (value.Length >= ValidationMessage.MarkerLength)
            {
                string Message;
                ValidationSeverity Severity;
                switch (value.Substring(0, ValidationMessage.MarkerLength))
                {
                    case ValidationMessage.InfoMarker:
                        Message = value.Substring(ValidationMessage.MarkerLength);
                        Severity = ValidationSeverity.Info;
                        break;
                    case ValidationMessage.WarningMarker:
                        Message = value.Substring(ValidationMessage.MarkerLength);
                        Severity = ValidationSeverity.Warning;
                        break;
                    case ValidationMessage.ErrorMarker:
                        Message = value.Substring(ValidationMessage.MarkerLength);
                        Severity = ValidationSeverity.Error;
                        break;
                    default:
                        Message = value;
                        Severity = ValidationSeverity.ImplicitError;
                        break;
                }

                return new ValidationMessage(Severity, Message);
            }
            else
            {
                return new ValidationMessage(ValidationSeverity.ImplicitError, value);
            }
        } 
        
        ///<summary>
        /// Parses the given string and converts all the messages into ValidationMessage objects. 
        ///</summary>
        /// <remarks>
        /// Messages must be separated by <see cref="ValidationMessage.Separator"/> and may be
        /// marked with a severity marker prefix like <see cref="ValidationMessage.ErrorMarker"/>
        /// </remarks>
        public static IEnumerable<ValidationMessage> ParseMultiple(string value)
        {
            return
                from MessagePart in value.Split(ValidationMessage.Separator[0])
                select ValidationMessage.Parse(MessagePart.Trim());
        }

        /// <summary/>
        public string Message { get; }

        /// <summary/>
        public ValidationSeverity Severity { get; }

        ///<summary/>
        public override string ToString()
        {
            switch(this.Severity)
            {
                case ValidationSeverity.Info:
                    return $"{ValidationMessage.InfoMarker}{this.Message}";
                case ValidationSeverity.Warning:
                    return $"{ValidationMessage.WarningMarker}{this.Message}";
                case ValidationSeverity.Error:
                    return $"{ValidationMessage.ErrorMarker}{this.Message}";
                case ValidationSeverity.ImplicitError:
            return this.Message;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Converts the given message into a string by appending the severity as text
        /// </summary>
        public static implicit operator string(ValidationMessage value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts the given string into a message, converting the severity marker
        /// into the corresponding severity. if no marker is found, <see cref="ValidationSeverity.Error"/>
        /// is assumed.
        /// </summary>
        public static implicit operator ValidationMessage(string value)
        {
            return ValidationMessage.Parse(value);
        }

        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore UnusedMember.Global
    }
}