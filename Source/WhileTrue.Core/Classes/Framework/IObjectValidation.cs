using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace WhileTrue.Classes.Framework
{
    public interface IObjectValidation : IDataErrorInfo
    {
        event EventHandler<ValidationEventArgs> ValidationChanged;
        string PreviewErrors(string propertyName, object value);
        IEnumerable<ValidationMessage> GetValidationMessages(string propertyName);
        bool HasErrors(string propertyName);

        string PreviewErrors<PropertyType>(Expression<Func<PropertyType>> property, object value);
        IEnumerable<ValidationMessage> GetValidationMessages<PropertyType>(Expression<Func<PropertyType>> property);
        bool HasErrors<PropertyType>(Expression<Func<PropertyType>> property);
    }

    public class ValidationEventArgs : EventArgs
    {
        private readonly string propertyName;

        public ValidationEventArgs(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }
    }
}