using System;

namespace LibrarySystem.Business.Exceptions
{
    public class DuplicateException : Exception
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public string EntityName { get; set; }

        public DuplicateException() : base("A duplicate record already exists.")
        {
            FieldName = string.Empty;
            FieldValue = string.Empty;
            EntityName = string.Empty;
        }

        public DuplicateException(string message) : base(message)
        {
            FieldName = string.Empty;
            FieldValue = string.Empty;
            EntityName = string.Empty;
        }

        public DuplicateException(string entityName, string fieldName, string fieldValue) 
            : base($"{entityName} with {fieldName} '{fieldValue}' already exists.")
        {
            EntityName = entityName;
            FieldName = fieldName;
            FieldValue = fieldValue;
        }

        public DuplicateException(string message, Exception innerException) 
            : base(message, innerException)
        {
            FieldName = string.Empty;
            FieldValue = string.Empty;
            EntityName = string.Empty;
        }
    }
}