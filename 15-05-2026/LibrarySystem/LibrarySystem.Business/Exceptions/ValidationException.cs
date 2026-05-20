using System;

namespace LibrarySystem.Business.Exceptions
{
    public class ValidationException : Exception
    {
        public string? FieldName { get; set; }

        public ValidationException() : base("Validation error occurred.")
        {
        }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string fieldName, string message) : base(message)
        {
            FieldName = fieldName;
        }

        public ValidationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}