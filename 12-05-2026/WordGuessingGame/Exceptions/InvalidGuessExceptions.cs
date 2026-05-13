using System;

namespace WordGuessingGame.Exceptions{
    public class InvalidGuessException : Exception 
    {
        public InvalidGuessException() : base() { }

        public InvalidGuessException(string message) : base(message) { }

        public InvalidGuessException(string message, Exception innerException) 
            :  base(message, innerException) { }
    }
}