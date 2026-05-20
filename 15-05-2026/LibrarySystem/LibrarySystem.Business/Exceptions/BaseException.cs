using System;

namespace LibrarySystem.Business.Exceptions
{
    public abstract class BaseException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        protected BaseException(string message, string errorCode, int statusCode = 400) 
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        protected BaseException(string message, string errorCode, Exception innerException, int statusCode = 400) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
}